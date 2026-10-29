#if BICUTIL_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace BicUtil.Ads{
    public class AdmobManager : IAdsPlatform
    {
        public string TermsURL => "https://policies.google.com/privacy/update";

        #region InstantData
        private Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion

        private bool initialized;
        private bool initializing;

        // Low-volume lifecycle signals, never emitted by readiness polling. Consumers
        // can distinguish no-fill/load failures from ads loaded but never displayed.
        public event Action<string, string, int> Diagnostic;

        private void report(string format, string stage, int errorCode = -1)
        {
            try { Diagnostic?.Invoke(format, stage, errorCode); }
            catch (Exception error) { Debug.LogException(error); }
        }

        private static bool TrackingAllowed
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
#if BICUTIL_iOS_SUPPORT
                return Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                    Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
#else
                return false;
#endif
#else
                return true;
#endif
            }
        }

        private bool CanServeAds => initialized && ConsentInformation.CanRequestAds();

        #region Logic

        public AdmobManager(bool _initialize){
            callbackContext = System.Threading.SynchronizationContext.Current;
            if (_initialize)
                Initialize(null);
        }

        // Capture Unity's process-wide context on startup, rather than resolving a
        // scene-owned TweenPool from SDK worker threads during a scene transition.
        private System.Threading.SynchronizationContext callbackContext;
        private void Dispatch(Action action)
        {
            if (callbackContext != null) callbackContext.Post(_ => action(), null);
            else BicTween.RunOnMainThread(action); // Standalone/non-Unity hosts.
        }

        public void Initialize(Action _action){
            if (callbackContext == null) callbackContext = System.Threading.SynchronizationContext.Current;
            if (initialized)
            {
                _action?.Invoke();
                return;
            }
            if (initializing)
                return;
            initializing = true;
            UpdateConsent(_result => {
                Debug.Log("[Admob] UpdateConsent result: " + _result);
                // A previous valid consent may still allow requests after an update error.
                if (!ConsentInformation.CanRequestAds())
                {
                    initializing = false;
                    _action?.Invoke();
                    return;
                }
#if UNITY_IOS && !UNITY_EDITOR
                // ATT denial still permits ads, but never enables first-party ad identifiers.
                var configuration = MobileAds.GetRequestConfiguration();
                configuration.PublisherFirstPartyIdEnabled = false;
                MobileAds.SetRequestConfiguration(configuration);
#endif
                MobileAds.Initialize(_initState => {
                    Dispatch(() => {
                        initialized = true;
                        initializing = false;
                        _action?.Invoke();
                    });
                });
            });
        }

        public void UpdateConsent(Action<ConsentResult> _callback){
            Debug.Log("[Admob] start UpdateConsent");
            var debugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds =
                new List<string>
                {
                    "A9178F1E-C3E8-407D-9A54-844E7CDF7FA8",
                    "366fac27-a1ef-4c95-b5eb-e313cf98d35c",
                    "9104b117-10b9-46ac-835f-a89cf6ef4f37",
                    "83049E61DFD375AB64940E8710FE94A1"
                }
            };

            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = debugSettings,
            };

            // UMP callbacks may arrive on an Android worker thread. Both the form
            // API and MobileAds' Unity-side initialization must run on Unity's thread.
            ConsentInformation.Update(request, error =>
                Dispatch(() => OnConsentInfoUpdated(error, _callback)));
        }

        void OnConsentInfoUpdated(FormError consentError, Action<ConsentResult> _callback)
        {

            Debug.Log("[Admob] start OnConsentInfoUpdated");
            if (consentError != null)
            {
                // Handle the error.
                UnityEngine.Debug.LogError("[Admob] consentError1 " + consentError);
                _callback(ConsentResult.ConsentUpdateError);
                return;
            }


            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) => Dispatch(() =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    UnityEngine.Debug.LogError("[Admob] consentError2 " + formError);
                    _callback(ConsentResult.ConsentShowFormError);
                    return;
                }

                // Consent has been gathered.
                if (ConsentInformation.CanRequestAds())
                {
                   _callback(ConsentResult.Success);
                }else{
                    _callback(ConsentResult.ConsentRequestError);
                }
            }));
        }

        public void SetAdsSettingIOSOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingIOSOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingIOSOnly(string _adsId, object _type){
            #if UNITY_IOS
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }


        public void SetAdsSettingAndroidOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingAndroidOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingAndroidOnly(string _adsId, object _type){
            #if UNITY_ANDROID
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }


        public void SetAdsSettingEditorOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingEditorOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingEditorOnly(string _adsId, object _type){
            #if UNITY_EDITOR
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }
        
        #endregion

        // Placements using the same unit ID share one native ad and one request/retry.
        // Readiness checks must never bypass a pending request or its backoff.
        private sealed class AdSlot<T> where T : class
        {
            public T Ad;
            public bool Pending;
            public bool Showing;
            public int Failures;
            public float RetryAfter;
            public float LoadedAt;
        }

        private readonly Dictionary<string, AdSlot<InterstitialAd>> interstitialSlots =
            new Dictionary<string, AdSlot<InterstitialAd>>();
        private readonly Dictionary<string, AdSlot<RewardedAd>> rewardedSlots =
            new Dictionary<string, AdSlot<RewardedAd>>();
        private bool fullScreenShowing;
        private const int MaxAutomaticRetries = 5;
        private const float AdCacheLifetimeSeconds = 3600f;

        private static AdSlot<T> getSlot<T>(Dictionary<string, AdSlot<T>> slots, string id) where T : class
        {
            if (!slots.TryGetValue(id, out var slot))
            {
                slot = new AdSlot<T>();
                slots.Add(id, slot);
            }
            return slot;
        }

        private static bool beginLoad<T>(AdSlot<T> slot) where T : class
        {
            if (slot.Pending || slot.Showing || slot.Ad != null || Time.realtimeSinceStartup < slot.RetryAfter)
                return false;
            slot.Pending = true;
            return true;
        }

        private static void retryLoad<T>(AdSlot<T> slot, Action load, float firstDelay) where T : class
        {
            slot.Failures++;
            float delay = Mathf.Min(Mathf.Max(1f, firstDelay) * Mathf.Pow(2f, Mathf.Min(slot.Failures - 1, 9)), 300f);
            slot.RetryAfter = Time.realtimeSinceStartup + delay;
            if (slot.Failures > MaxAutomaticRetries)
            {
                // Stop the automatic loop. A later readiness check may retry after a cooldown.
                slot.Pending = false;
                slot.RetryAfter = Time.realtimeSinceStartup + 300f;
                Debug.LogWarning("[Admob] Automatic load retries stopped; waiting for a later readiness check after 300 seconds.");
                return;
            }
            // Keep Pending set throughout the wait: UI polling cannot create another retry chain.
            BicTween.Delay(delay).SubscribeComplete(() => {
                slot.Pending = false;
                load();
            });
        }

        private static void reloadAfterShow<T>(AdSlot<T> slot, Action load) where T : class
        {
            slot.Pending = true;
            slot.RetryAfter = Time.realtimeSinceStartup + 0.5f;
            BicTween.Delay(0.5f).SubscribeComplete(() => {
                slot.Pending = false;
                load();
            });
        }

        #region Interstitial
        public bool IsReadyInterstitial(object _adsType)
        {
            if (!CanServeAds || fullScreenShowing) return false;
            var slot = getSlot(interstitialSlots, adsData[_adsType].PlatformId);
            if (slot.Ad != null)
            {
                bool expired = Time.realtimeSinceStartup - slot.LoadedAt >= AdCacheLifetimeSeconds;
                if (!expired && slot.Ad.CanShowAd()) return true;
                slot.Ad.Destroy();
                slot.Ad = null;
                report("interstitial", expired ? "cache_expired" : "cache_invalid");
            }
            LoadInterstitial(_adsType);
            return false;
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            if (!IsReadyInterstitial(_adsType)) { _callback?.Invoke(AdsResult.Failed); return; }
            var slot = getSlot(interstitialSlots, adsData[_adsType].PlatformId);
            var ad = slot.Ad;
            slot.Ad = null;
            slot.Showing = true;
            fullScreenShowing = true;
            bool completed = false;
            int showErrorCode = -1;
            Action<AdsResult> finish = result => Dispatch(() => {
                if (completed) return;
                completed = true;
                report("interstitial", result == AdsResult.Failed ? "show_failed" : "closed", showErrorCode);
                ad.Destroy();
                slot.Showing = false;
                fullScreenShowing = false;
                reloadAfterShow(slot, () => LoadInterstitial(_adsType));
                _callback?.Invoke(result);
            });
            ad.OnAdFullScreenContentClosed += () => finish(AdsResult.Finished);
            ad.OnAdFullScreenContentFailed += error => {
                showErrorCode = error.GetCode();
                finish(AdsResult.Failed);
            };
            try { ad.Show(); }
            catch (Exception error) { Debug.LogException(error); finish(AdsResult.Failed); }
        }

        public void LoadInterstitial(object _adsType) { loadInterstitial(_adsType, 1f); }

        public void loadInterstitial(object _adsType, float _time)
        {
            if (!CanServeAds) return;
            var id = adsData[_adsType].PlatformId;
            var slot = getSlot(interstitialSlots, id);
            if (!beginLoad(slot)) return;
            try
            {
                Debug.Log("[Admob] Interstitial load started");
                InterstitialAd.Load(id, buildRequest(), (ad, error) => Dispatch(() => {
                    if (error != null || ad == null)
                    {
                        ad?.Destroy();
                        Debug.LogWarning("[Admob] Interstitial load failed: " + error);
                        report("interstitial", "load_failed", error == null ? -1 : error.GetCode());
                        retryLoad(slot, () => loadInterstitial(_adsType, _time), _time);
                        return;
                    }
                    slot.Ad = ad;
                    slot.Pending = false;
                    slot.Failures = 0;
                    slot.RetryAfter = 0;
                    slot.LoadedAt = Time.realtimeSinceStartup;
                    ad.OnAdImpressionRecorded += () => Dispatch(() => report("interstitial", "impression"));
                    report("interstitial", "loaded");
                }));
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                report("interstitial", "load_exception");
                retryLoad(slot, () => loadInterstitial(_adsType, _time), _time);
            }
        }
        #endregion

        #region RewardBased
        public void LoadRewardBased(object _adsType)
        {
            if (!CanServeAds) return;
            loadRewardBased(adsData[_adsType].PlatformId, 1f);
        }

        public void loadRewardBased(string _adsId, float _time)
        {
            if (!CanServeAds) return;
            var slot = getSlot(rewardedSlots, _adsId);
            if (!beginLoad(slot)) return;
            try
            {
                Debug.Log("[Admob] Rewarded load started");
                RewardedAd.Load(_adsId, buildRequest(), (ad, error) => Dispatch(() => {
                    if (error != null || ad == null)
                    {
                        ad?.Destroy();
                        Debug.LogWarning("[Admob] Rewarded load failed: " + error);
                        report("rewarded", "load_failed", error == null ? -1 : error.GetCode());
                        retryLoad(slot, () => loadRewardBased(_adsId, _time), _time);
                        return;
                    }
                    slot.Ad = ad;
                    slot.Pending = false;
                    slot.Failures = 0;
                    slot.RetryAfter = 0;
                    slot.LoadedAt = Time.realtimeSinceStartup;
                    ad.OnAdImpressionRecorded += () => Dispatch(() => report("rewarded", "impression"));
                    report("rewarded", "loaded");
                }));
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                report("rewarded", "load_exception");
                retryLoad(slot, () => loadRewardBased(_adsId, _time), _time);
            }
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            if (!IsReadyRewardBased(_adsType)) { _callback?.Invoke(AdsResult.Failed); return; }
            var slot = getSlot(rewardedSlots, adsData[_adsType].PlatformId);
            var ad = slot.Ad;
            slot.Ad = null;
            slot.Showing = true;
            fullScreenShowing = true;
            bool earned = false;
            bool completed = false;
            int showErrorCode = -1;
            Action<bool> finish = failed => Dispatch(() => {
                if (completed) return;
                completed = true;
                report("rewarded", failed ? "show_failed" : "closed", showErrorCode);
                ad.Destroy();
                slot.Showing = false;
                fullScreenShowing = false;
                reloadAfterShow(slot, () => LoadRewardBased(_adsType));
                _callback?.Invoke(failed ? AdsResult.Failed : (earned ? AdsResult.Finished : AdsResult.Skipped));
            });
            ad.OnAdFullScreenContentClosed += () => finish(false);
            ad.OnAdFullScreenContentFailed += error => {
                showErrorCode = error.GetCode();
                finish(true);
            };
            try { ad.Show(reward => Dispatch(() => earned = true)); }
            catch (Exception error) { Debug.LogException(error); finish(true); }
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            if (!CanServeAds || fullScreenShowing) return false;
            var slot = getSlot(rewardedSlots, adsData[_adsType].PlatformId);
            if (slot.Ad != null)
            {
                bool expired = Time.realtimeSinceStartup - slot.LoadedAt >= AdCacheLifetimeSeconds;
                if (!expired && slot.Ad.CanShowAd()) return true;
                slot.Ad.Destroy();
                slot.Ad = null;
                report("rewarded", expired ? "cache_expired" : "cache_invalid");
            }
            LoadRewardBased(_adsType);
            return false;
        }
        #endregion

        #region Banner
        private BannerView bannerView;

        public bool IsReadyBanner(object _adsType){
            return CanServeAds;
        }
        public IAdsBanner CreateBanner(object _adsType, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            if(CanServeAds && adsData.ContainsKey(_adsType) == true){
                var _banner = MonoBehaviour.Instantiate(Resources.Load<AdmobBannerController>("AdmobBanner"));
                _banner.Load(adsData[_adsType].PlatformId, _adsType, _onLoadBannerAction, buildRequest());
                return _banner;
            }else{
                return null;
            }
        }

        #endregion

        #region UserConsent
        private bool isUserConsent = true;
        public void SetUserConsent(bool _isEnabled){
            isUserConsent = _isEnabled;
        }

        private AdRequest buildRequest()
        {
            var _request = new AdRequest();
            // UMP controls whether ads may be requested. ATT independently limits tracking.
            // Never interpret UMP consent as overriding an ATT denial or restriction.
            if (!TrackingAllowed || !isUserConsent)
                _request.Extras["npa"] = "1";
            return _request;
        }
        #endregion
    }
}
#endif

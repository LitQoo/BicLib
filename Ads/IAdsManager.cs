using System;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.Ads
{
    public interface IAdsPlatform {
        void LoadInterstitial(object _adsPlacement);
        bool IsReadyInterstitial(object _adsPlacement);
        void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsPlacement);
        bool IsReadyRewardBased(object _adsPlacement);
        void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback);
        
        bool IsReadyBanner(object _adsPlacement);
        IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction);
        
        string TermsURL{get;}
        void SetUserConsent(bool _isEnabled);
    }

    public interface IAdsManager
    {
        void LoadInterstitial(object _adsPlacement);
        bool IsReadyInterstitial(object _adsPlacement);
        void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsPlacement);
        bool IsReadyRewardBased(object _adsPlacement);
        void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback);

        void SetAdsSetting(object _adsPlacement, int _playTimeInterval);
        void AddAdsPlatform(IAdsPlatform _platform);
        IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction);
        void RemoveBanner(IAdsBanner _banner);
    }

    public interface IAdsBanner {
        bool IsReady();
        void Show();
        void Hide();
        void Destroy();
        void SetPosition(Transform _parent, Vector2 _position);
    }

    public enum AdsResult
    {
        Finished,
        Skipped,
        Cancel,
        Failed
    }

    public enum AdsType{
        RewardBase,
        Interstital,
        Banner
    }

    public class AdsPlatformInfo{
        public string PlatformId;
        public object AdsPlacement;
        public object Data = null;

        public AdsPlatformInfo(string _platformAdsId, object _adsPlacement){
            this.PlatformId = _platformAdsId;
            this.AdsPlacement = _adsPlacement;
        }
    }

    public class AdsInfo{
        public object AdsPlacement;
        public IVariable TimeInterval;
        public long LastPlayedAdsTime;
        public object Data = null;

        public AdsInfo(object _adsPlacement, IVariable _playTimeInterval){
            this.AdsPlacement = _adsPlacement;
            this.TimeInterval = _playTimeInterval;
            this.LastPlayedAdsTime = getTimestamp();
        }

        private long getTimestamp(){
            return System.DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        public void UpdateLastPlayedAdsTime(){
            this.LastPlayedAdsTime = getTimestamp();
        }

        public bool IsPossiblePlay{
            get{
                if(Mathf.Max(getTimestamp() - LastPlayedAdsTime, 0) >= TimeInterval.AsInt){
                    return true;
                }else{
                    return false;
                }
            }
        }
    }
}
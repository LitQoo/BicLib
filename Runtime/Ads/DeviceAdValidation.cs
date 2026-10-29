#if BICLIB_AD_VALIDATION && DEVELOPMENT_BUILD && !UNITY_EDITOR && BICUTIL_ADMOB
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BicUtil.Ads
{
    // Explicit local-development builds only. Never present in store binaries.
    public sealed class DeviceAdValidation : MonoBehaviour
    {
        private AdmobManager manager;
        private object interstitial, rewarded;
        private bool intLoaded, rvLoaded, busy;
        private string result = "Waiting for game startup and demo ad preloads";

        public static void Attach(AdmobManager manager, object interstitial, object rewarded,
            object banner = null)
        {
            // Map every placement sharing a unit, including automatic banners.
            // Official units: https://developers.google.com/admob/unity/test-ads
            var field = typeof(AdmobManager).GetField("adsData", BindingFlags.Instance | BindingFlags.NonPublic);
            var data = (Dictionary<object, AdsPlatformInfo>)field.GetValue(manager);
            var originalInt = data[interstitial].PlatformId;
            var originalRv = data[rewarded].PlatformId;
            string originalBanner = banner != null && data.ContainsKey(banner) ? data[banner].PlatformId : null;
#if UNITY_IOS
            const string intId = "ca-app-pub-3940256099942544/4411468910";
            const string rvId = "ca-app-pub-3940256099942544/1712485313";
            const string bannerId = "ca-app-pub-3940256099942544/2934735716";
#else
            const string intId = "ca-app-pub-3940256099942544/1033173712";
            const string rvId = "ca-app-pub-3940256099942544/5224354917";
            const string bannerId = "ca-app-pub-3940256099942544/6300978111";
#endif
            foreach (var info in data.Values)
            {
                if (info.PlatformId == originalInt) info.PlatformId = intId;
                else if (info.PlatformId == originalRv) info.PlatformId = rvId;
                else if (originalBanner != null && info.PlatformId == originalBanner) info.PlatformId = bannerId;
                else throw new InvalidOperationException("Unmapped production unit in diagnostic build: " + info.AdsPlacement);
            }
            var probe = new GameObject("Local demo ad validation").AddComponent<DeviceAdValidation>();
            probe.manager = manager;
            probe.interstitial = interstitial;
            probe.rewarded = rewarded;
            manager.Diagnostic += probe.OnDiagnostic;
            DontDestroyOnLoad(probe.gameObject);
            Debug.Log("BICLIB_AD_QA_ENABLED: " + Application.identifier + "; all registered units replaced with official demos.");
        }

        private void OnDiagnostic(string format, string stage, int code)
        {
            Debug.Log($"BICLIB_AD_QA_EVENT: {format} {stage} {code}");
            if (format == "interstitial" && stage == "loaded") intLoaded = true;
            if (format == "rewarded" && stage == "loaded") rvLoaded = true;
        }

        private void OnDestroy()
        {
            if (manager != null) manager.Diagnostic -= OnDiagnostic;
        }

        private void OnGUI()
        {
#if !BICLIB_UI_SIMULATION
            GUI.depth = -1000;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * (Screen.width / 420f));
            GUI.Box(new Rect(8, 65, 404, 195), "LOCAL AD TEST / " + Application.productName);
            GUI.Label(new Rect(18, 95, 390, 25), $"Demo preload: int={intLoaded}, reward={rvLoaded}");
            GUI.Label(new Rect(18, 120, 390, 25), result);
            GUI.enabled = !busy && intLoaded;
            if (GUI.Button(new Rect(18, 150, 184, 55), "Test interstitial"))
            {
                busy = true;
                intLoaded = false;
                manager.ShowInterstitial(interstitial, value => Complete("interstitial", value));
            }
            GUI.enabled = !busy && rvLoaded;
            if (GUI.Button(new Rect(216, 150, 184, 55), "Test rewarded"))
            {
                busy = true;
                rvLoaded = false;
                manager.ShowRewardBased(rewarded, value => Complete("rewarded", value));
            }
            GUI.enabled = !busy;
            if (GUI.Button(new Rect(110, 215, 200, 35), "Hide test panel")) Destroy(gameObject);
            GUI.enabled = true;
#endif
        }

        private void Complete(string format, AdsResult value)
        {
            result = format + " closed: " + value;
            Debug.Log("BICLIB_AD_QA_CLOSED: " + format + " " + value);
            busy = false;
        }
    }
}
#endif

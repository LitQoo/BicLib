// Standalone test doubles. Compile only with run.sh, never import into Unity.
#if !UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
namespace UnityEngine {
    public class Color { }
    public class MonoBehaviour {
        public static T Instantiate<T>(T value) { return value; }
    }
    public static class Resources { public static T Load<T>(string name) { return default(T); } }
    public static class Time { public static float realtimeSinceStartup; }
    public static class Mathf {
        public static float Min(float a, float b) { return Math.Min(a,b); }
        public static float Max(float a, float b) { return Math.Max(a,b); }
        public static float Pow(float a, float b) { return (float)Math.Pow(a,b); }
    }
    public static class Debug {
        public static void Log(object value) { }
        public static void LogError(object value) { }
        public static void LogWarning(object value) { }
        public static void LogException(Exception value) { }
    }
}
namespace BicUtil.Tween {
    public sealed class Timer {
        public float At; public Action Callback;
        public Timer SubscribeComplete(Action callback) { Callback = callback; return this; }
    }
    public static class BicTween {
        public static readonly List<Timer> Timers = new List<Timer>();
        public static readonly Queue<Action> Dispatch = new Queue<Action>();
        public static Timer Delay(float delay) {
            var timer = new Timer {At=UnityEngine.Time.realtimeSinceStartup+delay};
            Timers.Add(timer); return timer;
        }
        public static void RunOnMainThread(Action action) { Dispatch.Enqueue(action); }
        public static void Pump() { while(Dispatch.Count>0) Dispatch.Dequeue()(); }
        public static void Advance(float seconds) {
            UnityEngine.Time.realtimeSinceStartup += seconds;
            var due = Timers.FindAll(t=>t.At<=UnityEngine.Time.realtimeSinceStartup);
            foreach(var timer in due) { Timers.Remove(timer); timer.Callback(); }
            Pump();
        }
        public static void Reset() { Timers.Clear(); Dispatch.Clear(); UnityEngine.Time.realtimeSinceStartup=0; }
    }
}
namespace GoogleMobileAds.Api {
    public class AdRequest { public Dictionary<string,string> Extras = new Dictionary<string,string>(); }
    public class LoadAdError { public int Code=3; public int GetCode() { return Code; } }
    public class AdError { public int GetCode() { return 9; } }
    public class Reward { }
    public static class MobileAds { public static void Initialize(Action<object> callback) { callback(null); } }
    public class InterstitialAd {
        public static int Requests;
        public static Action<InterstitialAd,LoadAdError> Complete;
        public static bool ThrowLoad;
        public bool Ready=true;
        public int DestroyCount, ShowCount;
        public event Action OnAdFullScreenContentClosed;
        public event Action OnAdImpressionRecorded;
        public event Action<AdError> OnAdFullScreenContentFailed;
        public static void Load(string id,AdRequest request,Action<InterstitialAd,LoadAdError> callback) {
            Requests++; if(ThrowLoad) throw new Exception("load failed"); Complete=callback;
        }
        public bool CanShowAd() { return Ready; }
        public void Show() { ShowCount++; Ready=false; }
        public void Destroy() { DestroyCount++; Ready=false; }
        public void Close() { OnAdFullScreenContentClosed?.Invoke(); }
        public void Impression() { OnAdImpressionRecorded?.Invoke(); }
        public void Fail() { OnAdFullScreenContentFailed?.Invoke(new AdError()); }
    }
    public class RewardedAd {
        public static int Requests;
        public static Action<RewardedAd,LoadAdError> Complete;
        public bool Ready=true;
        public int DestroyCount, ShowCount;
        public Action<Reward> Reward;
        public event Action OnAdFullScreenContentClosed;
        public event Action OnAdImpressionRecorded;
        public event Action<AdError> OnAdFullScreenContentFailed;
        public static void Load(string id,AdRequest request,Action<RewardedAd,LoadAdError> callback) {
            Requests++; Complete=callback;
        }
        public bool CanShowAd() { return Ready; }
        public void Show(Action<Reward> callback) { ShowCount++; Ready=false; Reward=callback; }
        public void Destroy() { DestroyCount++; Ready=false; }
        public void Close() { OnAdFullScreenContentClosed?.Invoke(); }
        public void Impression() { OnAdImpressionRecorded?.Invoke(); }
        public void Fail() { OnAdFullScreenContentFailed?.Invoke(new AdError()); }
    }
    public class BannerView { }
}
namespace GoogleMobileAds.Ump.Api {
    public enum DebugGeography { EEA }
    public class ConsentDebugSettings { public DebugGeography DebugGeography; public List<string> TestDeviceHashedIds; }
    public class ConsentRequestParameters { public bool TagForUnderAgeOfConsent; public ConsentDebugSettings ConsentDebugSettings; }
    public class FormError { }
    public static class ConsentInformation {
        public static bool Allowed=true;
        public static bool CanRequestAds() { return Allowed; }
        public static void Update(ConsentRequestParameters request,Action<FormError> callback) { callback(null); }
    }
    public static class ConsentForm {
        public static void LoadAndShowConsentFormIfRequired(Action<FormError> callback) { callback(null); }
    }
}
namespace BicUtil.Ads {
    public interface IAdsPlatform { }
    public interface IAdsBanner { }
    public enum AdsResult { Finished,Skipped,Failed }
    public enum ConsentResult { Success,ConsentUpdateError,ConsentShowFormError,ConsentRequestError }
    public class AdsPlatformInfo {
        public string PlatformId; public object Data;
        public AdsPlatformInfo(string id,object placement) {PlatformId=id;}
    }
    public class AdmobBannerController : UnityEngine.MonoBehaviour,IAdsBanner {
        public void Load(string id,object placement,Action<IAdsBanner> callback,GoogleMobileAds.Api.AdRequest request) { }
    }
}
#endif

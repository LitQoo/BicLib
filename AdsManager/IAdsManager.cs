using System;
using UnityEngine;

namespace BicUtil.AdsManager
{
    public interface IAdsPlatform {
        void SetPlatformAndroid(string _androidId);
        void SetPlatformIos(string _iosId);
        void SetAdsSettingAndroidOnly(string _adsId, object _type);
        void SetAdsSettingIOSOnly(string _adsId, object _type);

        void LoadInterstitial(object _adsType);
        bool IsReadyInterstitial(object _adsType);
        void ShowInterstitial(object _adsType, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsType);
        bool IsReadyRewardBased(object _adsType);
        void ShowRewardBased(object _adsType, Action<AdsResult> _callback);
        IAdsBanner CreateBanner(object _adsType);
    }

    public interface IAdsManager
    {
        void LoadInterstitial(object _adsType);
        bool IsReadyInterstitial(object _adsType);
        void ShowInterstitial(object _adsType, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsType);
        bool IsReadyRewardBased(object _adsType);
        void ShowRewardBased(object _adsType, Action<AdsResult> _callback);

        void SetAdsSetting(string _adsId, object _type, int _playTimeInterval);
        void AddAdsPlatform(IAdsPlatform _platform);
        IAdsBanner CreateBanner(object _adsType);
        void RemoveBanner(IAdsBanner _adsType);
    }

    public interface IAdsBanner {
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


    public class AdsPlatformInfo{
        public string Id;
        public object AdsType;
        public object Data = null;

        public AdsPlatformInfo(string _id, object _type){
            this.Id = _id;
            this.AdsType = _type;
        }
    }

    public class AdsInfo{
        public string Id;
        public object AdsType;
        public int TimeInterval;
        public long LastPlayedAdsTime;
        public object Data = null;

        public AdsInfo(string _id, object _type, int _playTimeInterval){
            this.Id = _id;
            this.AdsType = _type;
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
                if(getTimestamp() - LastPlayedAdsTime > TimeInterval){
                    return true;
                }else{
                    return false;
                }
            }
        }
    }
}
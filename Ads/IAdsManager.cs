using System;
using UnityEngine;

namespace BicUtil.Ads
{
    public interface IAdsPlatform {
        void LoadInterstitial(object _adsType);
        bool IsReadyInterstitial(object _adsType);
        void ShowInterstitial(object _adsType, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsType);
        bool IsReadyRewardBased(object _adsType);
        void ShowRewardBased(object _adsType, Action<AdsResult> _callback);
        
        IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction);
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
        IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction);
        void RemoveBanner(IAdsBanner _adsType);
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


    public class AdsPlatformInfo{
        public string PlatformId;
        public object AdsType;
        public object Data = null;

        public AdsPlatformInfo(string _platformAdsId, object _type){
            this.PlatformId = _platformAdsId;
            this.AdsType = _type;
        }
    }

    public class AdsInfo{
        public string AdsId;
        public object AdsType;
        public int TimeInterval;
        public long LastPlayedAdsTime;
        public object Data = null;

        public AdsInfo(string _adsId, object _type, int _playTimeInterval){
            this.AdsId = _adsId;
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
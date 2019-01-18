using System;

namespace BicUtil.AdsManager
{
    public interface IAdsManager
    {
        void SetPlatformAndroid(string _androidId);
        void SetPlatformIos(string _iosId);
        void SetAdsSettingAndroidOnly(string _adsId, object _type, int _playTimeInterval);
        void SetAdsSettingIOSOnly(string _adsId, object _type, int _playTimeInterval);
        void Initialize();

        void LoadInterstitial(object _adsType);
        bool IsReadyInterstitial(object _adsType);
        void ShowInterstitial(object _adsType, Action<AdsResult> _callback);

        void LoadRewardBased(object _adsType);
        bool IsReadyRewardBased(object _adsType);
        void ShowRewardBased(object _adsType, Action<AdsResult> _callback);
         
    }

    public enum AdsResult
    {
        Finished,
        Skipped,
        Cancel,
        Failed
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
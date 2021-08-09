using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;
using BicUtil.Tween;
using BicUtil.Ads;
using BicDB.Variable;


namespace BicUtil.ServerTime
{
    public class ServerTimeManager : MonoBehaviourHardBase<ServerTimeManager>
    {
        public EnumVariable<ServerTimeState> State = new EnumVariable<ServerTimeState>(ServerTimeState.Ready);
        private long timeDiff = 0;
        private int timeout = 3;
        private NtpClient ntpClient = new NtpClient();
        private Action onFiledSyncCallback;
        
        private void OnDestroy() {
            BicTween.Cancel(this.gameObject);    
        }
        
        public void Sync(){
            if(State.AsEnum == ServerTimeState.Fail){
                return;
            }

            sync();
        }

        private void sync()
        {
            if(State.AsEnum == ServerTimeState.Connecting){
                return;
            }


            Debug.Log("Sync ticks");

            State.AsEnum = ServerTimeState.Connecting;

            timeDiff = 0;

            #if UNITY_EDITOR
            BicTween.Delay(5f).SubscribeComplete(()=>{
                InternetTime.GetTime(setTimeByInternet, timeout);
            }).SetTargetObject(this.gameObject);
            #else
            InternetTime.GetTime(setTimeByInternet, timeout);
            #endif
        }

        private void setTimeByInternet(bool _isSuccess, DateTime _dateTime){
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                ntpClient.GetNetworkTime(NtpClient.NTPORG, timeout, setTimeByNtpOrg);
            }
        }

        private void setTimeByNtpOrg(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                ntpClient.GetNetworkTime(NtpClient.WINDOSCOM, timeout, setTimeByWindowCom);
            }
        }

        private void setTimeByWindowCom(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                var _nistClient = new NistClient();
                _nistClient.GetNetworkTime(timeout, setTimeByNist);
            }
        }

        private void setTimeByNist(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                Debug.Log("time sync failed retry " + timeout.ToString());
                State.AsEnum = ServerTimeState.Fail;
                BicTween.Delay(timeout).SubscribeComplete(sync).SetTargetObject(this.gameObject);
                timeout *= 2;
            }
        }

        private void setDiff(DateTime _serverTime){
            this.timeDiff = (_serverTime.ToLocalTime().Ticks - System.DateTime.Now.ToLocalTime().Ticks) / TimeSpan.TicksPerSecond;
            State.AsEnum = ServerTimeState.Available;
            timeout = 3;
            Debug.Log("Server time = " + _serverTime.ToLocalTime().ToString() + "/ Local time = " + DateTime.Now.ToString());
        }

        private void OnApplicationPause(bool pauseStatus) {
            // server time 다시 동기화 할것. (마지막 로컬 타임과 1시간 이상 차이 날 경우)
            if(pauseStatus == false && State.AsEnum == ServerTimeState.Available){
                if(AdsManager.Instance.IsShowingAds == false){
                    Sync();
                }
            }
        }

        public DateTime Now{
            get{
                return System.DateTime.Now.AddSeconds(this.timeDiff);
            }
        }

        public DateTime UtcNow{
            get{
                return System.DateTime.UtcNow;
            }
        }

        public long Timestamp{
            get{
                return this.Now.Ticks / TimeSpan.TicksPerSecond;
            }
        }

        public int TimestampUnix{
            get{
                return (int)Math.Truncate((this.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            }
        }
    }

    public enum ServerTimeState
    {
        Ready,
        Available,
        Connecting,
        Fail
    }
}
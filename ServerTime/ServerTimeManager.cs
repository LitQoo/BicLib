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
            });
            #else
            InternetTime.GetTime(setTimeByInternet, internetTimeout);
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
                State.AsEnum = ServerTimeState.Fail;
                BicTween.Delay(timeout).SubscribeComplete(sync);
                timeout += 2;
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

        public long Timestamp{
            get{
                return this.Now.Ticks / TimeSpan.TicksPerSecond;
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
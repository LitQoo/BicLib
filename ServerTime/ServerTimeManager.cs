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
        BoolVariable isAvailable = new BoolVariable(false);
        bool isConnecting = false;
        long timeDiff = 0;
        NtpClient ntpClient = new NtpClient();
        
        public void Sync(){
            if(isConnecting == true){
                return;
            }

            Debug.Log("Sync ticks");

            isAvailable.AsBool = false;
            isConnecting = true;
            timeDiff = 0;

            #if UNITY_EDITOR
            BicTween.Delay(5f).SubscribeComplete(()=>{
                setDiff(DateTime.Now);
            });
            #else
            InternetTime.GetTime(setTimeByInternet);
            #endif
        }

        private void setTimeByInternet(bool _isSuccess, DateTime _dateTime){
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                ntpClient.GetNetworkTime(NtpClient.NTPORG, setTimeByNtpOrg);
            }
        }

        private void setTimeByNtpOrg(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                ntpClient.GetNetworkTime(NtpClient.WINDOSCOM, setTimeByWindowCom);
            }
        }

        private void setTimeByWindowCom(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                var _nistClient = new NistClient();
                _nistClient.GetNetworkTime(setTimeByNist);
            }
        }

        private void setTimeByNist(bool _isSuccess, DateTime _dateTime)
        {
            if(_isSuccess == true){
                setDiff(_dateTime);
            }else{
                isConnecting = false;
                Invoke("Sync", 10f);
            }
        }

        private void setDiff(DateTime _serverTime){
            this.timeDiff = (_serverTime.ToLocalTime().Ticks - System.DateTime.Now.ToLocalTime().Ticks) / TimeSpan.TicksPerSecond;
            isAvailable.AsBool = true;
            isConnecting = false;
            Debug.Log("Server time = " + _serverTime.ToLocalTime().ToString() + "/ Local time = " + DateTime.Now.ToString());
        }

        private void OnApplicationPause(bool pauseStatus) {
            // server time 다시 동기화 할것. (마지막 로컬 타임과 1시간 이상 차이 날 경우)
            if(pauseStatus == false && isConnecting == false){
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

        public BoolVariable IsAvailable{
            get{
                return isAvailable;
            }
        }
    }
}
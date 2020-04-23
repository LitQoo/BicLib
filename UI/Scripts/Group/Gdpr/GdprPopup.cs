using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using BicDB.Container;
using BicDB.Core;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class GdprPopup : MonoBehaviour
    {
        #region static
        static private int GDPR_STATE_YES = 1; 
        static private int GDPR_STATE_NO = 2;
        static private int GDPR_STATE_NONE = 0;

        private const string EU_QUERY_URL = "http://adservice.google.com/getconfig/pubvendors";
        private static bool isGdprArea(){
            bool _result = false;

            #if UNITY_EDITOR
            return true;
            #endif

            try
            {
                using( WebClient webClient = new WebClient())
                {
                    string response = webClient.DownloadString( EU_QUERY_URL );
                    int index = response.IndexOf( "is_request_in_eea_or_unknown\":" );
                    if( index < 0 )
                        _result = true;
                    else
                    {
                        index += 30;
                        _result = index >= response.Length || !response.Substring( index ).TrimStart().StartsWith( "false" );
                    }
                }
            }
            catch
            {
                _result = true;
            }

            return _result;
        }
        #endregion
        #region DI
        [SerializeField]
        private GameObject startLayer;
        [SerializeField]
        private GameObject howLayer;
        [SerializeField]
        private GameObject optionLayer;
        [SerializeField]
        private GameObject warningLayer;
        [SerializeField]
        private BicUtil.TableView.TableView tableView;
        [SerializeField]
        private BicUtil.UI.CheckButtonWithChangeActive analyticsSettingButton;
        [SerializeField]
        private BicUtil.UI.CheckButtonWithChangeActive adsSettingButton;
        #endregion

        #region Instant
        private EnumVariable<Mode> mode = new EnumVariable<Mode>(Mode.Start);
        private IVariable isAgreedAnaltics;
        private IVariable isAgreedAds;
        private bool isOpend = false;
        #endregion

        #region Event
        private void Awake(){
            mode.Subscribe(setMode);
            tableView.AddClickCellEvent("OpenTerms", openTerms);
        }

        private void openTerms(IRecordContainer _data)
        {
            Application.OpenURL(_data["url"].AsVariable.AsString);
        }
        #endregion

        #region Logic
        public void Setup(){

            isAgreedAnaltics = TableService.GetProperty("iaan", new IntVariable(GDPR_STATE_NONE));
            isAgreedAds = TableService.GetProperty("iaad", new IntVariable(GDPR_STATE_NONE));

            this.adsSettingButton.IsSelect.AsBool = isAgreedAds.AsInt != GDPR_STATE_NO;
            this.analyticsSettingButton.IsSelect.AsBool = isAgreedAnaltics.AsInt != GDPR_STATE_NO;

            this.adsSettingButton.IsSelect.Subscribe(_value=>{
                this.isAgreedAds.AsInt = _value.AsBool == true ? GDPR_STATE_YES : GDPR_STATE_NO; 
            });

            this.analyticsSettingButton.IsSelect.Subscribe(_value=>{
                this.isAgreedAnaltics.AsInt = _value.AsBool == true ? GDPR_STATE_YES : GDPR_STATE_NO; 
            });

            var _urls = new List<RecordContainer>();

            _urls.Add(new RecordContainer(new Dictionary<string, BicDB.IDataBase>(){
                {"url", new StringVariable("https://bigjamgames.com/privacy_en.html")}
            }));
            
            var _analyticsTerms = BicUtil.Analytics.Analytics.Instance.GetTerms();
            for(int i = 0; i < _analyticsTerms.Count; i++){
                _urls.Add(new RecordContainer(new Dictionary<string, BicDB.IDataBase>(){
                    {"url", new StringVariable(_analyticsTerms[i])}
                }));
            }

            var _adsTerms = BicUtil.Ads.AdsManager.Instance.GetTerms();
            for(int i = 0; i < _adsTerms.Count; i++){
                _urls.Add(new RecordContainer(new Dictionary<string, BicDB.IDataBase>(){
                    {"url", new StringVariable(_adsTerms[i])}
                }));
            }

            tableView.SetDBSource(_urls);
        }

        public void Open(){
            isOpend = true;
            this.gameObject.SetActive(true);
            this.mode.AsEnum = Mode.Start;

            var _result = new Dictionary<string, object> ();
            _result["sessionCount"] = TableService.SessionCount;
            BicUtil.Analytics.Analytics.Event("GdprPopupOpen", _result);
        }

        private void close(){
            this.gameObject.SetActive(false);

            var _result = new Dictionary<string, object> ();
            _result["analytics"] = isAgreedAnaltics.AsInt;
            _result["ads"] = isAgreedAds.AsInt;
            BicUtil.Analytics.Analytics.Event("GdprPopupClose", _result);
        }

        public bool ShouldOpen(bool _shouldCheckCountry){
            if(isOpend == true){
                return false;
            }

            //미설정
            if(isAgreedAnaltics.AsInt == GDPR_STATE_NONE){
                //국가관련없이 무조건 띄울경우
                if(_shouldCheckCountry == false){
                    return true;

                //gdpr대상국가인지 체크하고 띄울경우
                }else{
                    if(isGdprArea() == true){
                        return true;
                    }else{
                        return false;
                    }
                }
            }

            //둘중하나라도 거절상태
            if(isAgreedAds.AsInt == GDPR_STATE_NO || isAgreedAnaltics.AsInt == GDPR_STATE_NO){
                return true;
            }

            return false;            
        }

        private void setMode(IEnumVariable<Mode> _mode)
        {
            disableAllLayer();
            switch(_mode.AsEnum){
                case Mode.Start:
                setStart();
                break;
                case Mode.How:
                setHow();
                break;
                case Mode.Option:
                setOption();
                break;
                case Mode.Warning:
                setWarning();
                break;
            }
        }

        private void disableAllLayer()
        {
            this.startLayer.SetActive(false);
            this.howLayer.SetActive(false);
            this.optionLayer.SetActive(false);
            this.warningLayer.SetActive(false);
        }

        private void setStart()
        {
            this.startLayer.SetActive(true);
        }

        private void setHow()
        {
            this.howLayer.SetActive(true);
        }

        private void setOption()
        {
            this.optionLayer.SetActive(true);
        }

        private void setWarning(){
            this.warningLayer.SetActive(true);
        }

        public void AgreeAll(){
            this.isAgreedAds.AsInt = GDPR_STATE_YES;
            this.isAgreedAnaltics.AsInt = GDPR_STATE_YES;
            TableService.Save();
            close();
        }

        public void ModeTo(Mode _type){
            this.mode.AsEnum = _type;
        }

        public void OpenHow(){
            this.mode.AsEnum = Mode.How;
        }

        public void OpenOption(){
            this.mode.AsEnum = Mode.Option;
        }

        public void OpenWarning(){
            this.mode.AsEnum = Mode.Warning;
        }

        public void CheckSetting(){
            if(this.adsSettingButton.IsSelect.AsBool == true && this.analyticsSettingButton.IsSelect.AsBool == true){
                ApplySetting();
            }else{
                OpenWarning();
            }
        }

        public void ApplySetting(){
            if(this.isAgreedAds.AsInt == GDPR_STATE_NONE){
                this.isAgreedAds.AsInt = GDPR_STATE_YES;
            }

            if(this.isAgreedAnaltics.AsInt == GDPR_STATE_NONE){
                this.isAgreedAnaltics.AsInt = GDPR_STATE_YES;
            }
            
            TableService.Save();
            close();
        }

        #endregion

        #region Enum
        public enum Mode{
            Start,
            How,
            Option,
            Warning
        } 
        #endregion
    }

}
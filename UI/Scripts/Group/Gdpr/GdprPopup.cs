using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicDB.Variable;
using BicUtil.Ads;
using BicUtil.ClassInitializer;
using BicUtil.UIFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace BicUtil.Disabled{
    public class GdprPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject
    {
        #region static
        static private int GDPR_STATE_YES = 1; 
        static private int GDPR_STATE_NO = 2;
        static private int GDPR_STATE_NONE = 0;

        private const string EU_QUERY_URL = "http://adservice.google.com/getconfig/pubvendors";
        private static bool isGDPRArea = false;
        private static bool IsGDPRArea{
            get{
                lock(locker){
                    return isGDPRArea;
                }
            }
        }
        private static bool isLoadGDPRArea = false;
        public static System.Object locker = new System.Object();
        public static void LoadGdprArea(){
            if(isLoadGDPRArea == true){
                return;
            }

            isLoadGDPRArea = true;

            if(TableService.IsLoaded == false){
                Debug.LogError("Not load tableservice");
            }else{
                var _state = TableService.GetProperty("iaan", new IntVariable(GDPR_STATE_NONE));
                if(_state.AsInt != GDPR_STATE_NONE){
                    return;
                }
            }

            try
            {
                using( WebClient webClient = new WebClient())
                {
                    var _uri = new Uri(EU_QUERY_URL);
                    webClient.DownloadStringCompleted += (_objet, _result)=>
                    {
                        lock(locker){
                            isGDPRArea = ParseIsGDPRArea(_result.Result);
                        }
                    };

                    webClient.DownloadStringAsync(_uri);
                    
                }
            }
            catch
            {
                lock(locker){
                    isGDPRArea = false;
                }
            }
        }

        public static void LoadGDPRArea(){
            LoadGDPRAreaAsync().Forget();
        }

        public static async UniTaskVoid LoadGDPRAreaAsync(){
            if(isLoadGDPRArea == true){
                return;
            }

            isLoadGDPRArea = true;
            
            if(TableService.IsLoaded == false){
                Debug.LogError("Not load tableservice");
                return;
            }else{
                var _state = TableService.GetProperty("iaan", new IntVariable(GDPR_STATE_NONE));
                if(_state.AsInt != GDPR_STATE_NONE){
                    return;
                }
            }

            var _request = UnityWebRequest.Get(EU_QUERY_URL);
            await _request.SendWebRequest().ToUniTask();
            var _isGDPRArea = ParseIsGDPRArea(_request.downloadHandler.text);
        
            if(string.IsNullOrEmpty(_request.error) == false){
                return;
            }

            lock(locker){
                isGDPRArea = _isGDPRArea;
            }
        }

        private static bool ParseIsGDPRArea(string _response)
        {
            bool _isGDPRArea = false;
            try
            {
                int index = _response.IndexOf("is_request_in_eea_or_unknown\":");

                if (index < 0)
                {
                    _isGDPRArea = true;
                }
                else
                {
                    index += 30;
                    _isGDPRArea = index >= _response.Length || !_response.Substring(index).TrimStart().StartsWith("false");
                }
            }
            catch
            {
                _isGDPRArea = false;
            }

            return _isGDPRArea;
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
        [SerializeField]
        private bool openAlwaysOnEditor = false;
        [SerializeField]
        private bool forcedGdprAreaOnEditor = false;
        #endregion

        #region Instant
        private EnumVariable<Mode> mode = new EnumVariable<Mode>(Mode.Start);
        private IVariable isAgreedAnaltics;
        private IVariable isAgreedAds;
        private bool isOpend = false;
        private bool isSetup = false;
        private Action closeCallback;
        #endregion

        #region ClassInitialiszer
        public void Deinitialize()
        {
            UIFlow.UIFlow.Instance.UnregisterUI(this);	
        }

        public void Initialize()
        {
            this.gameObject.SetActive(false);
            UIFlow.UIFlow.Instance.RegisterUI(this);
            Setup();
        }
        #endregion

        #region IUIFlowObject
        public OnCloseUIResult OnClosedUI(IUIFlowObject _fromUI, Action _finishCallback, object _parameter)
        {
            return OnCloseUIResult.DoNotWait;
        }

        public void OnOpenedUI(IUIFlowObject _fromUI, object _paramter)
        {
            Open();
        }
        #endregion

        #region Event
        private void Awake(){
            mode.Subscribe(setMode, true);
            tableView.AddClickCellEvent("OpenTerms", openTerms);
        }

        private void openTerms(IRecordContainer _data)
        {
            Application.OpenURL(_data["url"].AsVariable.AsString);
        }
        #endregion

        #region Logic
        public void Setup(){
            if(isSetup == true){
                return;
            }

            isSetup = true;

            // LoadGdprArea();
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

            isAgreedAds.Subscribe(_agreed=>{
                if(_agreed.AsInt == GDPR_STATE_NO){
                    AdsManager.Instance.SetUserConsent(false);
                }else{
                    AdsManager.Instance.SetUserConsent(true);
                }
            }, true);

            isAgreedAnaltics.Subscribe(_agreed=>{
                if(_agreed.AsInt == GDPR_STATE_NO){
                    Analytics.Analytics.Instance.SetUserConsent(false);
                }else{
                    Analytics.Analytics.Instance.SetUserConsent(true);
                }
            }, true);

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

        public void SubscribeOnClose(Action _callback){
            closeCallback += _callback;
        }

        private void close(){
            this.gameObject.SetActive(false);

            var _result = new Dictionary<string, object> ();
            _result["analytics"] = isAgreedAnaltics.AsInt;
            _result["ads"] = isAgreedAds.AsInt;
            _result["session"] = TableService.SessionCount;
            BicUtil.Analytics.Analytics.Event("GdprPopupClose", _result);

            if(closeCallback != null){
                closeCallback();
            }
        }

        public bool ShouldOpen(bool _shouldCheckCountry){
            #if UNITY_EDITOR
            if(this.openAlwaysOnEditor == true){
                return true;
            }
            #endif

            if(isOpend == true){
                return false;
            }

            //미설정
            if(isAgreedAnaltics.AsInt == GDPR_STATE_NONE || isAgreedAds.AsInt == GDPR_STATE_NONE){
                //국가관련없이 무조건 띄울경우
                if(_shouldCheckCountry == false){
                    return true;

                //gdpr대상국가인지 체크하고 띄울경우
                }else{
                    if(IsGDPRArea == true 
                        #if UNITY_EDITOR
                        || forcedGdprAreaOnEditor == true
                        #endif
                    ){
                        return true;
                    }else{
                        return false;
                    }
                }
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
            tableView.ReloadData();
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
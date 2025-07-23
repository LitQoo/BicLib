using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.ClassInitializer;
using BicUtil.Translate;
using BicUtil.Tween;
using BicUtil.UIFlow;
using UnityEngine;

namespace BicUtil.UI{
    public class ExitPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject
    {
        
        #region DI
        [SerializeField]
        private UnityEngine.UI.Text titleText;
        [SerializeField]
        private UnityEngine.UI.Text messageText;
        #endregion

        #region Instant
        private Func<string, string> translator;
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

            #if UNITY_EDITOR
            BicTween.Delay(0.5f).SubscribeComplete(()=>{
                var _check = TranslateManager.Instance.GetText("exit");
                if(string.IsNullOrEmpty(_check) == true){
                    Debug.LogError("Setup translate for exit");
                }
            });
            #endif
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

        #region Logic
        public void Open(){
            if(translator == null){
                translator = TranslateManager.Instance.GetText;
            }


            titleText.text = translator("exit");
            messageText.text = translator("exit_again");

            this.gameObject.SetActive(true);

            UIFlow.UIFlow.Instance.SetBackKeyAction(()=>{
                Exit();
            });
        }

        public void Close(){
            if(UIFlow.UIFlow.Instance.CurrentUI == this as UIFlow.IUIFlowObject){
                UIFlow.UIFlow.Instance.Back(CloseMode.Disable);
            }

            this.gameObject.SetActive(false);
        }

        public void Exit(){
            Application.Quit();
            #if UNITY_EDITOR
            Debug.Log("EXIT");
            #endif
        }
        #endregion
    }
}
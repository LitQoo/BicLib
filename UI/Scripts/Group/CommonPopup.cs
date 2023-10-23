using System.Globalization;
using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class CommonPopup : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private GameObject header;
        [SerializeField]
        private UnityEngine.UI.Text titleText;
        [SerializeField]
        private UnityEngine.UI.Text mainMessageText;
        [SerializeField]
        private UnityEngine.UI.Text subMessageText;
        [SerializeField]
        private UnityEngine.UI.InputField inputField;
        [SerializeField]
        private UnityEngine.UI.Text placeholderText;
        [SerializeField]
        private UnityEngine.UI.Button closeButton;
        [SerializeField]
        private UnityEngine.UI.Button leftButton;
        [SerializeField]
        private UnityEngine.UI.Button rightButton;
        [SerializeField]
        private UnityEngine.UI.Button bottomButton;
        [SerializeField]
        private UnityEngine.UI.Text leftButtonText;
        [SerializeField]
        private UnityEngine.UI.Text rightButtonText;
        [SerializeField]
        private UnityEngine.UI.Text bottomButtonText;
        [SerializeField]
        private UnityEngine.UI.Image leftButtonIcon;
        [SerializeField]
        private UnityEngine.UI.Image rightButtonIcon;
        [SerializeField]
        private GameObject popup;

        private RectTransform backTrasform = null;
        #endregion   

        #region Instant
        public string InputText{
            get{
                return inputField.text;
            }
        }

        public string Id = "";
        #endregion

        #region Event
        private Action onClickDimmed;
        private Action onClickClose;
        private Action onClickLeftButton;
        private Action onClickRightButton;
        private Action onClickBottomButton;
        #endregion

        #region Logic
        public void OnClickDimmed(){
            if(onClickDimmed != null){
                onClickDimmed();
            }
        }

        public void OnClickClose(){
            if(onClickClose != null){
                onClickClose();
            }
        }

        public void OnClickLeftButton(){
            if(onClickLeftButton != null){
                onClickLeftButton();
            }
        }

        public void OnClickRightButton(){
            if(onClickRightButton != null){
                onClickRightButton();
            }
        }

        public void OnClickBottomButton(){
            if(onClickBottomButton != null){
                onClickBottomButton();
            }
        }

        public void diableAllUI(){
            onClickClose = null;
            onClickDimmed = null;
            onClickLeftButton = null;
            onClickRightButton = null;
            onClickBottomButton = null;
            titleText.text = string.Empty;
            header.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            mainMessageText.gameObject.SetActive(false);
            subMessageText.gameObject.SetActive(false);
            inputField.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(false);
            leftButton.gameObject.SetActive(false);
            leftButtonText.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            bottomButton.gameObject.SetActive(false);
            rightButtonIcon.gameObject.SetActive(false);
            rightButtonText.gameObject.SetActive(false);
            leftButtonIcon.gameObject.SetActive(false);
            if(popup != null){
                popup.SetActive(true);
            }
        }

        public void Info(string _text, Action _onClickClose){
            diableAllUI();
            header.SetActive(true);
            mainMessageText.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            mainMessageText.text = _text;
            onClickClose = _onClickClose;
        }

        public void Confirm(string _text, string _buttonText, Action _onClickLeftButton){
            diableAllUI();
            mainMessageText.gameObject.SetActive(true);
            leftButton.gameObject.SetActive(true);
            leftButtonText.gameObject.SetActive(true);
            leftButtonText.text = _buttonText;
            mainMessageText.text = _text;
            onClickLeftButton = _onClickLeftButton;
        }

        public void NoButton(string _text){
            diableAllUI();
            mainMessageText.gameObject.SetActive(true);
            mainMessageText.text = _text;
        }

        public void Confirm(string _text, Sprite _buttonIcon, Action _onClickLeftButton){
            diableAllUI();
            mainMessageText.gameObject.SetActive(true);
            leftButton.gameObject.SetActive(true);
            leftButtonIcon.gameObject.SetActive(true);
            leftButtonIcon.sprite = _buttonIcon;
            mainMessageText.text = _text;
            onClickLeftButton = _onClickLeftButton;
        }

        public void Dimmed(string _text){
            diableAllUI();
            mainMessageText.gameObject.SetActive(true);
            mainMessageText.text = _text;
        }

        public void BlockAndOpen(){
            diableAllUI();
            if(popup != null){
                popup.SetActive(false);
            }
            this.Open();
        }

        public void Select(string _text, string _leftButtonText, string _rightButtonText, Action _onClickLeftButton, Action _onClickRightButton){
            diableAllUI();
            mainMessageText.gameObject.SetActive(true);
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            leftButtonText.gameObject.SetActive(true);
            rightButtonText.gameObject.SetActive(true);
            mainMessageText.text = _text;
            leftButtonText.text = _leftButtonText;
            rightButtonText.text = _rightButtonText;
            onClickLeftButton = _onClickLeftButton;
            onClickRightButton = _onClickRightButton;
        }

        public void Input(string _text, string _buttonText, string _inputText, string _placeholderText, Action _onClickLeftButton, Action _onClickClose){
            diableAllUI();
            header.gameObject.SetActive(true);
            mainMessageText.gameObject.SetActive(true);
            leftButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(_onClickClose != null);
            inputField.gameObject.SetActive(true);
            leftButtonText.gameObject.SetActive(true);
            mainMessageText.text = _text;
            leftButtonText.text = _buttonText;
            onClickLeftButton = _onClickLeftButton;
            onClickClose = _onClickClose;
            inputField.text = _inputText;
            placeholderText.text = _placeholderText;
        }

        public void SetTitle(string _text){
            this.titleText.text = _text;
            this.titleText.gameObject.SetActive(true);
            this.header.gameObject.SetActive(titleText);
        }

        public void SetSubMessage(string _text){
            this.subMessageText.gameObject.SetActive(true);
            this.subMessageText.text = _text;
        }

        public void SetBottomButton(string _text, Action _callback){
            this.bottomButton.gameObject.SetActive(true);
            this.bottomButtonText.text = _text;
            this.onClickBottomButton = _callback;
        }


        public void SetCloseButton(Action _callback){
            this.header.gameObject.SetActive(true);
            this.closeButton.gameObject.SetActive(true);
            this.onClickClose = _callback;
        }

        public void SetDimmed(Action _callback){
            this.onClickDimmed = _callback;
        }

        public void Open()
        {
            this.gameObject.SetActive(true);

            rebuild();
        }

        private void rebuild()
        {
            if (backTrasform == null)
            {
                backTrasform = header.transform.parent.gameObject.GetComponent<RectTransform>();
            }

            if (backTrasform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(backTrasform);
            }
        }

        public void Close(){
            this.gameObject.SetActive(false);
            if(popup != null){
                this.popup.SetActive(true);
            }
            Id = "";
        }
        #endregion
    }
}
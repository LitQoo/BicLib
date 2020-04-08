using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class RadioButtonGroup : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] checkButtons;

        private ICheckButton[] buttons;
        public IntVariable SelectedButtonIndex = new IntVariable(0);

        private void Awake(){
            buttons = new ICheckButton[checkButtons.Length];

            for(int i = 0; i < checkButtons.Length; i++){
                var _button = checkButtons[i].GetComponent<ICheckButton>();
                _button.IsSelect.AsBool = false;
                _button.IgnoreUnselectTouch = true;
                _button.IsSelect.Subscribe(setButtonGroup);
                buttons[i] = _button;
            }

            SelectedButtonIndex.Subscribe(selectButton, true);
        }

        private void selectButton(IVariableReadOnly _buttonIndex)
        {
            if(this.buttons[_buttonIndex.AsInt].IsSelect.AsBool != true){
                this.buttons[_buttonIndex.AsInt].IsSelect.AsBool = true;
            }
        }

        private void setButtonGroup(IVariableReadOnly _isSelect)
        {
            if(_isSelect.AsBool == false){
                return;
            }

            for(int i = 0; i < this.buttons.Length; i++){
                if(this.buttons[i].IsSelect != _isSelect){
                    this.buttons[i].IsSelect.AsBool = false;
                }else{
                    SelectedButtonIndex.AsInt = i;
                }
            }
        }
    }
}
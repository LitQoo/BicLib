using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BicUtil.UI{
    public class ToggleSwitch : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Button button;
        [SerializeField]
        private GameObject onHead;
        [SerializeField]
        private GameObject offHead;
        [SerializeField]
        private Color backgroundColorOn;
        [SerializeField]
        private Color backgroundColorOff;
        [SerializeField]
        private bool value = false;

        public UnityEvent<bool> OnChagnedValue;

        public bool Value{
            get=>value;
            set{
                this.value = value;

                if(OnChagnedValue != null){
                    OnChagnedValue.Invoke(this.value);
                }

                if(this.value == true){
                    on();
                }else{
                    off();
                }
            }
        }

        public void SetValueWithoutNotify(bool _value){
            this.value = _value;

            if(this.value == true){
                on();
            }else{
                off();
            }

        }

        private void Awake() {
            this.Value = this.value;     
        }

        public void ToggleValue(){
            this.Value = !this.Value;
        }

        private void off(){
            this.button.image.color = backgroundColorOff;
            this.onHead.SetActive(false);
            this.offHead.SetActive(true);
        }

        private void on(){
            this.button.image.color = backgroundColorOn;
            this.onHead.SetActive(true);
            this.offHead.SetActive(false);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class NumberChange : MonoBehaviour
    {
        #region Setting
        [SerializeField]
        private int minValue;
        [SerializeField]
        private int maxValue;
        [SerializeField]
        private int gapValue;
        [SerializeField]
        private string valueFormat = "{0}";
        [SerializeField]
        private UnityEngine.UI.Text valueText;
        #endregion

        public IntVariable Value{get;set;} = new IntVariable(0);

        private void Awake(){
            this.Value.Subscribe(setValueText, true);
        }

        public void Add(){
            this.Value.AsInt = Mathf.Clamp(this.Value.AsInt + gapValue, minValue, maxValue);
        }

        public void Sub(){
            this.Value.AsInt = Mathf.Clamp(this.Value.AsInt - gapValue, minValue, maxValue);
        }

        private void setValueText(IVariable _value)
        {
            this.valueText.text = string.Format(valueFormat, this.Value.AsInt);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class ActiveSelect : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private GameObject[] enableObjects;
        #endregion

        public IntVariable SelectedIndex{get;set;} = new IntVariable(0);

        private void Awake(){
            this.SelectedIndex.Subscribe(enableObject, true);
        }

        private void enableObject(IVariableReadOnly _selectedIndexk)
        {
            for(int i = 0; i < enableObjects.Length; i++){
                enableObjects[i].SetActive(i == _selectedIndexk.AsInt);
            }
        }

        public void IncreaseIndex(){
            var _nextValue = SelectedIndex.AsInt + 1;
            if(_nextValue >= enableObjects.Length){
                _nextValue = 0;
            }
            
            SelectedIndex.AsInt = Mathf.Clamp(_nextValue, 0, enableObjects.Length - 1);
        
        }
    }
}
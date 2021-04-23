using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BicUtil.UI{
    public class TabBarController : MonoBehaviour
    {
        #region LinkingObject
        [SerializeField]
        private GameObject viewLayer;
        [SerializeField]
        private GameObject buttonLayer;
        [SerializeField]
        private GameObject[] viewers;
        [SerializeField]
        private UnityEngine.UI.Button[] buttons;
        [SerializeField]
        private int defaultIndex = 0;
        #endregion

        private void Awake(){
            for(int i = 0; i < viewers.Length; i++){
                viewers[i].SetActive(defaultIndex == i);
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate() {
            if(viewLayer != null){
                var _viewer = new List<GameObject>();
                for(int i = 0; i < viewLayer.transform.childCount; i++){
                    _viewer.Add(viewLayer.transform.GetChild(i).gameObject);
                }
                this.viewers = _viewer.ToArray();
            }

            if(buttonLayer != null){
                var _buttons = new List<UnityEngine.UI.Button>();
                for(int i = 0; i < buttonLayer.transform.childCount; i++){
                    var _button = buttonLayer.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>();
                    _buttons.Add(_button);
                    link(_button, i);
                }

                this.buttons = _buttons.ToArray();
            }

            UnityEditor.EditorUtility.SetDirty(this.gameObject);
        }

        private void link(UnityEngine.UI.Button _button, int _index){
            if(_button == null){
                Debug.Log("not found button");
                return;
            }

            UnityAction<int> action = new UnityAction<int>(this.OpenTab);
            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(_button.onClick, action, _index);
        }
        #endif

        public void OpenTab(int _index){
            for(int i = 0; i < viewers.Length; i++){
                viewers[i].SetActive(_index == i);
            }
        }


    }
}

using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class CheckButtonWithChangeActive : MonoBehaviour, ICheckButton
    {
        [SerializeField]
        private UnityEngine.UI.Button targetButton;
        [SerializeField]
        private GameObject[] enableAtOnObjects;
        [SerializeField]
        private GameObject[] disableAtOnObjects;

        public BoolVariable IsSelect{get;set;} = new BoolVariable(false);
        public bool IgnoreUnselectTouch {get;set;} = false;

        private void Awake(){
            IsSelect.Subscribe(changeActive, true);
            targetButton.onClick.AddListener(changeSelectState);
        }

        private void changeSelectState()
        {
            if(IgnoreUnselectTouch == true && IsSelect.AsBool == true){
                return;
            }

            IsSelect.AsBool = !IsSelect.AsBool;
        }

        private void changeActive(IVariable _isSelect)
        {
            for(int i = 0; i < enableAtOnObjects.Length; i++){
                enableAtOnObjects[i].gameObject.SetActive(_isSelect.AsBool);
            }
            
            for(int i = 0; i < disableAtOnObjects.Length; i++){
                disableAtOnObjects[i].gameObject.SetActive(!_isSelect.AsBool);
            }
        }
    }
}
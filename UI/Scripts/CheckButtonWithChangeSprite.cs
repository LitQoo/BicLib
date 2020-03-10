using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class CheckButtonWithChangeSprite : MonoBehaviour, ICheckButton
    {
        [SerializeField]
        private UnityEngine.UI.Button targetButton;
        [SerializeField]
        private UnityEngine.UI.Image targetImage;
        [SerializeField]
        private Sprite onSprite;
        [SerializeField]
        private Sprite offSprite;

        public BoolVariable IsSelect{get;set;} = new BoolVariable(false);
        public bool IgnoreUnselectTouch {get;set;} = false;

        private void Awake(){
            IsSelect.Subscribe(changeSprite, true);
            targetButton.onClick.AddListener(changeSelectState);
        }

        private void changeSelectState()
        {
            if(IgnoreUnselectTouch == true && IsSelect.AsBool == true){
                return;
            }

            IsSelect.AsBool = !IsSelect.AsBool;
        }

        private void changeSprite(IVariable _isSelect)
        {
            if(_isSelect.AsBool == true){
                this.targetImage.sprite = onSprite;
            }else{
                this.targetImage.sprite = offSprite;
            }
        }
    }

    public interface ICheckButton{
        BoolVariable IsSelect{get;set;}
        bool IgnoreUnselectTouch {get;set;}
    }
}
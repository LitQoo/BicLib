using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.UI
{
    public class CheckInRewardIcon : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private Image rewardIcon;
        [SerializeField]
        private Image background;
        [SerializeField]
        private Text rewardText;
        [SerializeField]
        private Text dayText;
        [SerializeField]
        private GameObject check;
        #endregion

        #region logic
        public void SetRewardIcon(Sprite _sprite, Vector2 _size){
            this.rewardIcon.sprite = _sprite;
            this.rewardIcon.rectTransform.sizeDelta = _size;
        }

        public void SetRewardText(string _text){
            this.rewardText.text = _text;
        }

        public void EnableCheck(bool _isEnableCheck){
            check.gameObject.SetActive(_isEnableCheck);
        }

        public void SetColor(Color _color){
            background.color = _color;
        }

        public Image RewardIconImage{
            get{
                return rewardIcon;
            }
        }

        public string RewardString{
            get{
                return rewardText.text;
            }
        }
        #endregion
    }
}
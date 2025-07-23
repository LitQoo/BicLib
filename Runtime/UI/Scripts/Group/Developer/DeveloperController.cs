using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.Tween;

namespace Bigjam.Developer
{
    public class DeveloperController : MonoBehaviour
    {
        #region LinkingObject
        [SerializeField]
        private LitqooController litqoo;
        [SerializeField]
        private PonyController pony;
        #endregion

        #region InstantData
        TweenTracker _litqooDance = new TweenTracker();
        TweenTracker _ponyDance = new TweenTracker();
        #endregion
        
        #region Logic
        public void PlayHadsUpDance(){
            _litqooDance.SetTween(playHansUpDance(litqoo));
            _ponyDance.SetTween(playHansUpDance(pony));
        }

        public void StopDance(){
            _litqooDance.Cancel();
            _ponyDance.Cancel();
        }

        private Tween playHansUpDance(IDeveloper _developer){
            var _spawn = BicTween.Spawn();

            var _leftHandSeq = BicTween.Sequance();
            _leftHandSeq.AddChild(BicTween.RotateZ(_developer.LeftHand.gameObject, 160, 220, 0.5f));
            _leftHandSeq.AddChild(BicTween.RotateZ(_developer.LeftHand.gameObject, 220, 160, 0.5f));
            _spawn.AddChild(_leftHandSeq);

            var _rightHandSeq = BicTween.Sequance();
            _rightHandSeq.AddChild(BicTween.RotateZ(_developer.RightHand.gameObject, 160, 220, 0.5f));
            _rightHandSeq.AddChild(BicTween.RotateZ(_developer.RightHand.gameObject, 220, 160, 0.5f));
            _spawn.AddChild(_rightHandSeq);

            for(int i = 0; i < _developer.Head.Length; i++){
                var _headSeq = BicTween.Sequance();
                _headSeq.AddChild(BicTween.MoveLocalX(_developer.Head[i].gameObject, -10, 10, 0.5f));
                _headSeq.AddChild(BicTween.MoveLocalX(_developer.Head[i].gameObject, 10, -10, 0.5f));
                _spawn.AddChild(_headSeq);
            }

            var _jumpSeq = BicTween.Sequance();
            _jumpSeq.AddChild(BicTween.Delay(UnityEngine.Random.Range(0.0f, 0.3f)));
            _jumpSeq.AddChild(BicTween.MoveLocalY(_developer.All, 0, 50, 0.2f));
            _jumpSeq.AddChild(BicTween.MoveLocalY(_developer.All, 50, 0, 0.2f));
            _spawn.AddChild(_jumpSeq);

            return _spawn.SetRepeatForever().Play();
        }
        #endregion
    }
}
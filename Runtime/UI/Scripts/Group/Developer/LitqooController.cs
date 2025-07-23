using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BicUtil.Tween;

namespace Bigjam.Developer
{
    public class LitqooController : MonoBehaviour, IDeveloper
    {
        #region LinkingObject
        [SerializeField]
        private Image leftHand;
        [SerializeField]
        private Image rightHand;
        [SerializeField]
        private Image body;
        [SerializeField]
        private Image[] head;
        #endregion

        #region IDeveloper
        public Image LeftHand{get{return this.leftHand;}}
        public Image RightHand{get{return this.rightHand;}}
        public Image Body{get{return this.body;}}
        public Image[] Head{get{return this.head;}}
        public GameObject All{get{return this.gameObject;}}
        #endregion
    }
}
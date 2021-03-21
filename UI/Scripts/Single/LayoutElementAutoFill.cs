using System.Collections;
using System.Collections.Generic;
using BicUtil.CSharpExtensions;
using UnityEngine;

namespace BicUtil.UI{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class LayoutElementAutoFill : MonoBehaviour
    {
        private RectTransform parentRectTrasnform;
        private RectTransform rectTransform;
        private bool isSetup = false;
        private bool isVertical = true;

        private void autoFill()
        {
            if(isSetup == false){
                isSetup = true;

                if(this.transform.parent.gameObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>() != null){
                    isVertical = true;
                }else if(this.transform.parent.gameObject.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>() != null){
                    isVertical = false;
                }else{
                    Debug.LogWarning("Not Found layoutgroup");
                    isSetup = false;
                    return;
                }
            }

            if(parentRectTrasnform == null){
                parentRectTrasnform = this.transform.parent.transform as RectTransform;
            }

            if(this.rectTransform == null){
                rectTransform = (this.transform as RectTransform);
            }


            float _sizeSum = 0;
            var _childConers = new Vector3[4];
            for (int i = 0; i < parentRectTrasnform.childCount; i++)
            {
                var _child = parentRectTrasnform.GetChild(i);
                if (_child != this.transform)
                {
                    try{
                        var _childSize = (_child.transform as RectTransform).CalculateSize();

                        if(isVertical == true){
                            _sizeSum += _childSize.y;
                        }else{
                            _sizeSum += _childSize.x;
                        }
                    }catch{

                    }
                }
            }

            var _parentSize = parentRectTrasnform.CalculateSize();
            var _parentCorners = new Vector3[4];
            parentRectTrasnform.GetWorldCorners(_parentCorners);
            var _size = rectTransform.sizeDelta;
            if(isVertical == true){
                _size = new Vector2(rectTransform.sizeDelta.x, Mathf.Max(_parentSize.y - _sizeSum, 0));
            }else{
                _size = new Vector2(Mathf.Max(0, _parentSize.x - _sizeSum), rectTransform.sizeDelta.y);
            }

            rectTransform.sizeDelta = _size;
        }

        private void Update(){
            if(this.transform.hasChanged == true){
                autoFill();
            }
        }
    }

}
using System;
using UnityEngine;

namespace BicUtil.Tween
{
    [Serializable]
    public enum TweenType
    {
        None,
        Sequance,
        Spawn,
        Move,
        Active,
        Scale,
        Rotate,
        Delay,
        Value,
        Bezier,
        Virtual,
        Alpha,
        Size,
        Shake
    }

    public static class UpdateFuncs{

		public static Action<IUpdateData> GetFunc(TweenType _type){
			switch(_type){
				case TweenType.Move: return Move;
				case TweenType.Scale: return Scale;
				case TweenType.Rotate: return Rotate;
				case TweenType.Bezier: return Bezier;
				case TweenType.Active: return Active;
                case TweenType.Alpha: return Alpha;
                case TweenType.Size: return Size;
                case TweenType.Shake: return Shake;
			}

			return null;
		}

		public static void Move(IUpdateData _data){
			_data.TargetObject.transform.localPosition = _data.CurrentValue;
		}

		public static void Scale(IUpdateData _data){
			_data.TargetObject.transform.localScale = _data.CurrentValue;
		}

		public static void Rotate(IUpdateData _data){
			_data.TargetObject.transform.eulerAngles = _data.CurrentValue;
		}

		public static void Bezier(IUpdateData _data){
			if(_data.Data == null){
				_data.Data = new BezierPath(BicTween.ChildDataToBezier(_data.ChildDataList).ToArray());
			}

			var _curveData = (_data.Data as BezierPath);
			_data.TargetObject.transform.localPosition = _curveData.point(_data.CurrentValue.x);
		}

		public static void Active(IUpdateData _data){
            if(_data.Rate < 1f){
                if(_data.DiffValue.x >= 1f){
                    _data.TargetObject.SetActive(true);
                }else{
                    _data.TargetObject.SetActive(false);
                }
            }else if(_data.Rate >= 1f){
                if(_data.DiffValue.y >= 1f){
                    _data.TargetObject.SetActive(true);
                }else{
                    _data.TargetObject.SetActive(false);
                }
            }
		}

        public static void Alpha(IUpdateData _data){
            if(_data.Data == null){
                _data.Data = _data.TargetObject.GetComponent<UnityEngine.UI.Image>();
            }
            
            UnityEngine.UI.Image _image = (UnityEngine.UI.Image)_data.Data;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _data.CurrentValue.w);
        }

        public static void Size(IUpdateData _data){
            if(_data.Data == null){
                _data.Data = _data.TargetObject.GetComponent<RectTransform>();
            }

            RectTransform _rectTransform = (RectTransform)_data.Data;
            _rectTransform.sizeDelta = _data.CurrentValue;
        }

        public static void Shake(IUpdateData _data){
            if(_data.Data == null){
                _data.Data = _data.TargetObject.transform.localPosition;
            }

            Vector3 _shakeValue = new Vector3(UnityEngine.Random.Range(-_data.DiffValue.x, _data.DiffValue.x), UnityEngine.Random.Range(-_data.DiffValue.y, _data.DiffValue.y), UnityEngine.Random.Range(-_data.DiffValue.z, _data.DiffValue.z));
            _data.TargetObject.transform.localPosition = ((Vector3)_data.Data) + _shakeValue;

            if(_data.Rate >= 1f){
                _data.TargetObject.transform.localPosition = (Vector3)_data.Data;
            }
        }
	}

}

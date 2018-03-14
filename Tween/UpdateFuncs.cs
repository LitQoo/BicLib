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
        Shake,
        TypeWriting,
        Counter
    }

    public static class UpdateFuncs{
        public static void SetUpdateFunc(TweenModel _tween){
            _tween.UpdateFunc = None;

            switch(_tween.Type){
                case TweenType.Move: _tween.UpdateFunc = Move; break;
                case TweenType.Scale: _tween.UpdateFunc = Scale; break;
                case TweenType.Rotate: _tween.UpdateFunc = Rotate; break;
                case TweenType.Bezier: _tween.UpdateFunc = Bezier; break;
                case TweenType.Active: _tween.UpdateFunc = Active; break;
                case TweenType.Alpha: _tween.UpdateFunc = Alpha; break;
                case TweenType.Size: _tween.UpdateFunc = Size; break;
                case TweenType.Shake: _tween.UpdateFunc = Shake; break;
                case TweenType.TypeWriting: _tween.OnRepeatCallback = TypeWriting; break;
            }
        }

        public static void None(IUpdateData _data){

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
                _data.Data = _data.TargetObject.GetComponent<UnityEngine.UI.Graphic>();
            }
            
            UnityEngine.UI.Graphic _image = (UnityEngine.UI.Graphic)_data.Data;
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

        public static void TypeWriting(TweenModel _tween , int _number){
            if(_tween.Data == null){
                _tween.Data = _tween.TargetObject.GetComponent<UnityEngine.UI.Text>();
                _tween.RepeatCount = _tween.StringData.Length;
            }

            var _text = _tween.Data as UnityEngine.UI.Text;
            _text.text = _tween.StringData.Substring(0, _number);
        }
	}

}

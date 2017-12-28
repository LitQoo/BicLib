using System;
namespace BicUtil.Tween
{
    public static class UpdateFuncs{

		public static Action<IUpdateData> GetFunc(TweenType _type){
			switch(_type){
				case TweenType.Move: return Move;
				case TweenType.Scale: return Scale;
				case TweenType.Rotate: return Rotate;
				case TweenType.Bezier: return Bezier;
				case TweenType.Active: return Active;
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
	}

}

using System;
using UnityEngine;
namespace BicUtil.Tween
{

    public enum EaseType{
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InBounce,
        OutBounce,
        InOutBounce,
        InBack,
        OutBack,
        InOutBack,
        InElastic,
        OutElastic,
        InOutElastic,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InSine,
        OutSine,
        InOutSine,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        Spring
    }

    public static class EaseFuncs{
		public static Func<IEaseData, Vector4> GetFunc(EaseType _type){
			switch(_type){
				case EaseType.Linear: return Linear;
				case EaseType.InQuad: return InQuad;
				case EaseType.OutQuad: return OutQuad;
				case EaseType.InOutQuad: return InOutQuad;
				case EaseType.InBounce: return InBounce;
				case EaseType.OutBounce: return OutBounce;
				case EaseType.InOutBounce: return InOutBounce;
				case EaseType.InBack: return InBack;
				case EaseType.OutBack: return OutBack;
				case EaseType.InOutBack: return InOutBack;
                case EaseType.InElastic: return InElastic;
                case EaseType.OutElastic: return OutElastic;
                case EaseType.InOutElastic: return InOutElastic;
                case EaseType.InCubic : return InCubic;
                case EaseType.OutCubic : return OutCubic;
                case EaseType.InOutCubic : return InOutCubic;
                case EaseType.InQuart : return InQuart;
                case EaseType.OutQuart : return OutQuart;
                case EaseType.InOutQuart : return InOutQuart;
                case EaseType.InQuint : return InQuint;
                case EaseType.OutQuint : return OutQuint;
                case EaseType.InOutQuint : return InOutQuint;
                case EaseType.InSine : return InSine;
                case EaseType.OutSine : return OutSine;
                case EaseType.InOutSine : return InOutSine;
                case EaseType.InExpo : return InExpo;
                case EaseType.OutExpo : return OutExpo;
                case EaseType.InOutExpo : return InOutExpo;
                case EaseType.InCirc : return InCirc;
                case EaseType.OutCirc : return OutCirc;
                case EaseType.InOutCirc : return InOutCirc;
                case EaseType.Spring : return Spring;
			}

			return null;
		}

		public static Vector4 Linear(IEaseData _data){
			return _data.OriginValue + _data.DiffValue * _data.Rate;
		}

		public static Vector4 InQuad(IEaseData _data){
			return _data.OriginValue + _data.DiffValue * _data.Rate * _data.Rate;
		}

		public static Vector4 OutQuad(IEaseData _data){
			return -_data.DiffValue * _data.Rate * (_data.Rate - 2) + _data.OriginValue;
		}

        public static Vector4 InOutQuad(IEaseData _data){
            float _val = _data.Rate / 0.5f;
            if (_val < 1) return _data.DiffValue / 2 * _val * _val + _data.OriginValue;
            _val--;
            return -_data.DiffValue / 2 * (_val * (_val - 2) - 1) + _data.OriginValue;
        }

		public static Vector4 InBounce(IEaseData _data){
			return inBounce(_data.OriginValue, _data.DiffValue, _data.Rate);
		}

		public static Vector4 OutBounce(IEaseData _data){
			return outBounce(_data.OriginValue, _data.DiffValue, _data.Rate);
		}

		private static Vector4 outBounce(Vector4 _originValue, Vector4 _diffValue, float _rate){
			var _value = _rate;

			if (_value < (1 / 2.75f)){
				return _diffValue * (7.5625f * _value * _value) + _originValue;
			}else if (_value < (2 / 2.75f)){
				_value -= (1.5f / 2.75f);
				return _diffValue * (7.5625f * (_value) * _value + .75f) + _originValue;
			}else if (_value < (2.5 / 2.75)){
				_value -= (2.25f / 2.75f);
				return _diffValue * (7.5625f * (_value) * _value + .9375f) + _originValue;
			}else{
				_value -= (2.625f / 2.75f);
				return _diffValue * (7.5625f * (_value) * _value + .984375f) + _originValue;
			}
		}

		private static Vector4 inBounce(Vector4 _originValue, Vector4 _diffValue, float _rate){
			float d = 1f;
			return _diffValue - outBounce(Vector4.zero, _diffValue, d-_rate) + _originValue;
		}

		public static Vector4 InOutBounce(IEaseData _data){
			float d= 1f;
			if (_data.Rate < d/2) return inBounce(Vector4.zero, _data.DiffValue, _data.Rate*2) * 0.5f + _data.OriginValue;
			else return outBounce(Vector4.zero, _data.DiffValue, _data.Rate*2-d) * 0.5f + _data.DiffValue*0.5f + _data.OriginValue;
		}

		public static Vector4 InBack(IEaseData _data){
			float _overshoot = 1f;
			float s= 1.70158f * _overshoot;
			return _data.DiffValue * (_data.Rate) * _data.Rate * ((s + 1) * _data.Rate - s) + _data.OriginValue;
		}

		public static Vector4 OutBack(IEaseData _data){
			float _overshoot = 1f;
			float s = 1.70158f * _overshoot;
			var _val = (_data.Rate / 1) - 1;
			return _data.DiffValue * ((_val) * _val * ((s + 1) * _val + s) + 1) + _data.OriginValue;
		}

        public static Vector4 InOutBack(IEaseData _data){
            float _overshoot = 1f;
            float _s = 1.70158f * _overshoot;
            float _val = _data.Rate / .5f;
            if ((_val) < 1){
                _s *= (1.525f) * _overshoot;
                return _data.DiffValue / 2 * (_val * _val * (((_s) + 1) * _val - _s)) + _data.OriginValue;
            }
            _val -= 2;
            _s *= (1.525f) * _overshoot;
            return _data.DiffValue / 2 * ((_val) * _val * (((_s) + 1) * _val + _s) + 2) + _data.OriginValue;
        }

        public static Vector4 InElastic(IEaseData _data){
            return new Vector4(
                inElastic(_data.OriginValue.x, _data.DiffValue.x, _data.Rate, 1f, 0.3f),
                inElastic(_data.OriginValue.y, _data.DiffValue.y, _data.Rate, 1f, 0.3f),
                inElastic(_data.OriginValue.z, _data.DiffValue.z, _data.Rate, 1f, 0.3f),
                inElastic(_data.OriginValue.w, _data.DiffValue.w, _data.Rate, 1f, 0.3f)
                );
        }

        private static float inElastic(float _start, float _diff, float _rate, float _overshoot, float _period){
            float p = _period;
            float s = 0f;
            float a = 0f;

            if (_rate == 0f) return _start;

            if (_rate == 1f) return _start + _diff;

            if (a == 0f || a < Mathf.Abs(_diff)){
                a = _diff;
                s = p / 4f;
            }else{
                s = p / (2f * Mathf.PI) * Mathf.Asin(_diff / a);
            }

            if(_overshoot>1f && _rate>0.6f )
                _overshoot = 1f + ((1f-_rate) / 0.4f * (_overshoot-1f));

            _rate = _rate-1f;
            return _start-(a * Mathf.Pow(2f, 10f * _rate) * Mathf.Sin((_rate - s) * (2f * Mathf.PI) / p)) * _overshoot;
        }

        public static Vector4 OutElastic(IEaseData _data){
            return new Vector4(
                outElastic(_data.OriginValue.x, _data.DiffValue.x, _data.Rate, 1f, 0.3f),
                outElastic(_data.OriginValue.y, _data.DiffValue.y, _data.Rate, 1f, 0.3f),
                outElastic(_data.OriginValue.z, _data.DiffValue.z, _data.Rate, 1f, 0.3f),
                outElastic(_data.OriginValue.w, _data.DiffValue.w, _data.Rate, 1f, 0.3f)
                );
        }
        
        private static float outElastic(float _start, float _diff, float _rate, float _overshoot, float _period){
            float s = 0f;
            float a = 0f;

            if (_rate == 0f) return _start;

            if (_rate == 1f) return _start + _diff;

            if (a == 0f || a < Mathf.Abs(_diff)){
                a = _diff;
                s = _period / 4f;
            }else{
                s = _period / (2f * Mathf.PI) * Mathf.Asin(_diff / a);
            }
            if(_overshoot>1f && _rate<0.4f )
                _overshoot = 1f + (_rate / 0.4f * (_overshoot-1f));

            return _start + _diff + a * Mathf.Pow(2f, -10f * _rate) * Mathf.Sin((_rate - s) * (2f * Mathf.PI) / _period) * _overshoot;
        }
        
        public static Vector4 InOutElastic(IEaseData _data){
            return new Vector4(
            inOutElastic(_data.OriginValue.x, _data.DiffValue.x, _data.Rate, 1f, 0.3f),
            inOutElastic(_data.OriginValue.y, _data.DiffValue.y, _data.Rate, 1f, 0.3f),
            inOutElastic(_data.OriginValue.z, _data.DiffValue.z, _data.Rate, 1f, 0.3f),
            inOutElastic(_data.OriginValue.w, _data.DiffValue.w, _data.Rate, 1f, 0.3f)
            );
        }

        private static float inOutElastic(float _start, float _diff, float _rate, float _overshoot, float _period){
            
            float s = 0f;
            float a = 0f;

            if (_rate == 0f) return _start;

            _rate = _rate / (1f/2f);
            if (_rate == 2f) return _start + _diff;

            if (a == 0f || a < Mathf.Abs(_diff)){
                a = _diff;
                s = _period / 4f;
            }else{
                s = _period / (2f * Mathf.PI) * Mathf.Asin(_diff / a);
            }

            if(_overshoot>1f){
                if(_rate < 0.2f){
                    _overshoot = 1f + (_rate / 0.2f * (_overshoot-1f));
                }else if(_rate > 0.8f){
                    _overshoot = 1f + ((1f-_rate) / 0.2f * (_overshoot-1f));
                }
            }

            if (_rate < 1f){
                _rate = _rate - 1f;
                return _start - 0.5f * (a * Mathf.Pow(2f, 10f * _rate) * Mathf.Sin((_rate - s) * (2f * Mathf.PI) / _period)) * _overshoot;
            }

            _rate = _rate-1f;
            return _diff + _start + a * Mathf.Pow(2f, -10f * _rate) * Mathf.Sin((_rate - s) * (2f * Mathf.PI) / _period) * 0.5f * _overshoot;
        }

        public static Vector4 InCubic(IEaseData _data){
            return _data.DiffValue * _data.Rate * _data.Rate * _data.Rate + _data.OriginValue;
        }

        public static Vector4 OutCubic(IEaseData _data){
            var _rate = _data.Rate - 1;
            return _data.DiffValue * (_rate * _rate * _rate + 1) + _data.OriginValue;
        }

        public static Vector4 InOutCubic(IEaseData _data){
            var _rate =  _data.Rate / .5f;
            if (_rate < 1) return _data.DiffValue / 2f * _rate * _rate * _rate + _data.OriginValue;
            _rate -= 2;
            return _data.DiffValue / 2f * (_rate * _rate * _rate + 2) + _data.OriginValue;
        }

        public static Vector4 InQuart(IEaseData _data){
            return _data.DiffValue * _data.Rate * _data.Rate * _data.Rate * _data.Rate + _data.OriginValue;
        }

        public static Vector4 OutQuart(IEaseData _data){
            var _rate = _data.Rate - 1f;
            return - _data.DiffValue * (_rate * _rate * _rate * _rate - 1f) + _data.OriginValue;
        }

        public static Vector4 InOutQuart(IEaseData _data){
            var _rate = _data.Rate / 0.5f;
            if(_rate < 1){
                return _data.DiffValue / 2 * _rate * _rate * _rate * _rate + _data.OriginValue;
            }

            _rate -= 2;

            return - _data.DiffValue / 2 * (_rate * _rate * _rate * _rate - 2f) + _data.OriginValue;
        }

        public static Vector4 InQuint(IEaseData _data){
            return _data.DiffValue * _data.Rate * _data.Rate * _data.Rate * _data.Rate * _data.Rate + _data.OriginValue;
        }

        public static Vector4 OutQuint(IEaseData _data){
            var _rate = _data.Rate - 1f;
            return _data.DiffValue * (_rate * _rate * _rate * _rate * _rate + 1) + _data.OriginValue;
        }

        public static Vector4 InOutQuint(IEaseData _data){
            var _rate = _data.Rate / 0.5f;
            if(_rate < 1){
                return _data.DiffValue / 2f * _rate * _rate * _rate * _rate * _rate + _data.OriginValue;
            }

            _rate -= 2f;
            return _data.DiffValue / 2f * (_rate * _rate * _rate * _rate * _rate + 2f) + _data.OriginValue;
        }

        public static Vector4 InSine(IEaseData _data){
            return new Vector4(
                inSine(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inSine(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inSine(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inSine(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inSine(float _originValue, float _diffValue, float _rate){
            return -_diffValue * Mathf.Cos(_rate / 1 * (Mathf.PI / 2f)) + _diffValue + _originValue;
        }

        public static Vector4 OutSine(IEaseData _data){
            return new Vector4(
                outSine(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                outSine(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                outSine(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                outSine(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float outSine(float _originValue, float _diffValue, float _rate){
            return _diffValue * Mathf.Sin(_rate / 1 * (Mathf.PI / 2f)) + _originValue;
        }

        public static Vector4 InOutSine(IEaseData _data){
            return new Vector4(
                inOutSine(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inOutSine(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inOutSine(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inOutSine(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inOutSine(float _originValue, float _diffValue, float _rate){
            return -_diffValue / 2f * (Mathf.Cos(Mathf.PI * _rate / 1f) - 1) + _originValue;
        }

        public static Vector4 InExpo(IEaseData _data){
            return new Vector4(
                inExpo(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inExpo(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inExpo(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inExpo(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inExpo(float start, float end, float val){
            return end * Mathf.Pow(2, 10 * (val / 1f - 1)) + start;
        }

        public static Vector4 OutExpo(IEaseData _data){
            return new Vector4(
                outExpo(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                outExpo(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                outExpo(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                outExpo(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float outExpo(float start, float end, float val){
            return end * (-Mathf.Pow(2, -10 * val / 1f) + 1) + start;
        }

        public static Vector4 InOutExpo(IEaseData _data){
            return new Vector4(
                inOutExpo(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inOutExpo(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inOutExpo(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inOutExpo(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inOutExpo(float _originValue, float _diffValue, float _rate){
            _rate /= .5f;
            if (_rate < 1) return _diffValue / 2 * Mathf.Pow(2, 10 * (_rate - 1)) + _originValue;
            _rate--;
            return _diffValue / 2f * (-Mathf.Pow(2, -10 * _rate) + 2) + _originValue;
        }

        public static Vector4 InCirc(IEaseData _data){
            return new Vector4(
                inCirc(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inCirc(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inCirc(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inCirc(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inCirc(float _originValue, float _diffValue, float _rate){
            return -_diffValue * (Mathf.Sqrt(1 - _rate * _rate) - 1) + _originValue;
        }

        public static Vector4 OutCirc(IEaseData _data){
            return new Vector4(
                outCirc(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                outCirc(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                outCirc(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                outCirc(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float outCirc(float _originValue, float _diffValue, float _rate){
            _rate--;
            return _diffValue * Mathf.Sqrt(1 - _rate * _rate) + _originValue;
        }

        public static Vector4 InOutCirc(IEaseData _data){
            return new Vector4(
                inOutCirc(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                inOutCirc(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                inOutCirc(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                inOutCirc(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float inOutCirc(float _originValue, float _diffValue, float _rate){
            _rate /= .5f;
            if (_rate < 1) return -_diffValue / 2f * (Mathf.Sqrt(1 - _rate * _rate) - 1) + _originValue;
            _rate -= 2;
            return _diffValue / 2f * (Mathf.Sqrt(1 - _rate * _rate) + 1) + _originValue;
        }

        public static Vector4 Spring(IEaseData _data){
            return new Vector4(
                spring(_data.OriginValue.x, _data.DiffValue.x, _data.Rate),
                spring(_data.OriginValue.y, _data.DiffValue.y, _data.Rate),
                spring(_data.OriginValue.z, _data.DiffValue.z, _data.Rate),
                spring(_data.OriginValue.w, _data.DiffValue.w, _data.Rate)
            );
        }

        private static float spring(float _originValue, float _diffValue, float _rate){
            _rate = Mathf.Clamp01(_rate);
            _rate = (Mathf.Sin(_rate * Mathf.PI * (0.2f + 2.5f * _rate * _rate * _rate)) * Mathf.Pow(1f - _rate, 2.2f ) + _rate) * (1f + (1.2f * (1f - _rate) ));
            return _originValue + (_diffValue) * _rate;
        }
	}

}

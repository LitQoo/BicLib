using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Tween
{

	[System.Serializable]
	public class InitialInformations{
		public int TargetTweenId;
		public List<InitialInformation> List = new List<InitialInformation>();

		public void Apply(){
			for(int i = 0; i < List.Count; i++){
				List[i].Apply();
			}
		}

		public InitialInformations(int _targetTweenId){
			this.TargetTweenId = _targetTweenId;
		}

		public void Add(){
			List.Add(new InitialInformation());
		}
	}

	[System.Serializable]
	public class InitialInformation {
		public Vector4 Value;
		public GameObject TargetObject;
		public InitialType Type;
		public string StringValue;
		private object data;

		private T GetObject<T>() where T : class{
			if(data != null){
				return data as T;
			}

			T _object = TargetObject.GetComponent<T>();
			data = _object;
			return _object;
		}

		public void Apply(){
			switch(this.Type){
				case InitialType.Position:
					TargetObject.transform.position = this.Value;
				break;
				case InitialType.LocalPosition:
					TargetObject.transform.localPosition = this.Value;
				break;
				case InitialType.Size:
					this.GetObject<RectTransform>().sizeDelta = this.Value;
				break;
				case InitialType.Rotate:
					TargetObject.transform.eulerAngles = this.Value;
				break;
				case InitialType.Scale:
					TargetObject.transform.localScale = this.Value;
				break;
				case InitialType.Enable:
					TargetObject.SetActive(this.Value.x > 0);
				break;
				case InitialType.UIColor:
					this.GetObject<UnityEngine.UI.MaskableGraphic>().color = new Color(this.Value.x, this.Value.y, this.Value.z, this.Value.w);
				break;
				case InitialType.UIAlpha:
					var _object = this.GetObject<UnityEngine.UI.MaskableGraphic>();
					_object.color = new Color(_object.color.r, _object.color.g, _object.color.b, this.Value.x); 
				break;
				case InitialType.UIText:
					var _textObject = this.GetObject<UnityEngine.UI.Text>();
					_textObject.text = this.StringValue;
				break;
			}
		}
	}

	public enum InitialType{
		Position,
		LocalPosition,
		Size,
		Rotate,
		Scale,
		Enable,
		UIColor,
		UIAlpha,
		UIText
	}
}
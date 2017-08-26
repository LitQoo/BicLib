using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BicUtil.Components.SkinSystem
{
	[CreateAssetMenu(menuName = "Create SkinData")]
	public class SkinScriptableData : ScriptableObject {
		public List<SkinColor> Colors = new List<SkinColor>();

		public Color GetColor(string _partsName){
			var _result = Colors.FirstOrDefault (_row => _row.Name == _partsName);
			if (_result == null) {
				Debug.LogError ("not found color " + _partsName); 
				return Color.white;
			}

			return  Colors.FirstOrDefault (_row => _row.Name == _partsName).Color;
		}
	}

	[System.Serializable]
	public class SkinColor{
		public string Name;
		public Color Color;
	}
}

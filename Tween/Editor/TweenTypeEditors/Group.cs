using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		private void drawGroupHandleControl(TweenModel _tween){
			var _childs = _tween.GetChildList();
			for(int i = 0; i < _childs.Count; i++){
				DrawHandleControl(_childs[i]);
			}
		}

		private bool onClickedNodeGroup(TweenModel _tween, Event _event){
			if(onClickedNodeSingle(_tween, _event) == false){
				var _childList = _tween.GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					if(OnClicked(_childList[i], _event) == true){
						return true;
					}
				}

				return false;
			}else{
				return true;
			}
		}
	}
}

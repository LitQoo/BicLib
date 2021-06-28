using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		private void drawGroupHandleControl(Tween _tween){
			var _childs = _tween.GetChildList();
			DrawHandleControl(_childs);
		}

		private bool onClickedNodeGroup(Tween _tween, Event _event){
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

        private void openToAddTweenMenu(Tween _tween){
            GenericMenu genericMenu = new GenericMenu ();
            genericMenu.AddItem (new GUIContent ("Single"), false,delegate() {
				var _newTween = BicTween.MoveLocal(_tween.TargetObject, Vector3.zero, Vector3.zero, 1f, selectedTweenPool);

			    selectedTweenPool.AddTween(_tween, _newTween);
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });

            genericMenu.AddItem (new GUIContent ("Sequance"), false,delegate() {
                selectedTweenPool.AddTween(_tween, BicTween.Sequance(selectedTweenPool).SetTargetObject(_tween.TargetObject));
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });
            
			genericMenu.AddItem (new GUIContent ("Spawn"), false,delegate() {
                selectedTweenPool.AddTween(_tween, BicTween.Spawn(selectedTweenPool).SetTargetObject(_tween.TargetObject));
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });

			genericMenu.AddItem (new GUIContent ("Copy Childs"), false,delegate() {
				copiedTweens = _tween.GetChildList();
            });
            genericMenu.ShowAsContext ();
        }
	}
}

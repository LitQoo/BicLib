using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		[TweenEditorDrawNode(TweenType.Delay)]
		private Rect drawDelayNode(Tween _tween, Vector2 _startPosition, Timeline _timeline){
			return drawSingleNode(_tween, _startPosition, _timeline);
		}
		//[TweenEditorDrawNode(TweenType.?)]
		//private Rect draw?Node(Tween _tween, Vector2 _startPosition, Timeline _timeline)


		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(Tween _tween, Event _event)


		//[TweenEditorSettingNode(TweenType.?)]
		//void draw?Setting(Tween _tween)


		//[TweenEditorHandleController(TweenType.?)]
		//private void draw?Preview(Tween _tween)

	}
}

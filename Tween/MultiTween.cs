using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if BICUTIL_SPINE
using Spine.Unity;
#endif

namespace BicUtil.Tween
{
    public class MultiTween{
		TweenModel mother = null;
		List<TweenModel> groupTween = new List<TweenModel>();
		
		public TweenModel Current{
			get{
				if(groupTween.Count <= 0){
					throw new SystemException("MultiTween Current not found");
				}

				return groupTween[groupTween.Count - 1];
			}
		}

		private void addGroupTween(TweenModel _tween){
			if(mother == null){
				mother = _tween;
			}

			groupTween.Add(_tween);

			if(groupTween.Count > 1){
				groupTween[groupTween.Count - 2].AddChild(_tween);
			}
		}

		private void removeLastGroupTween(){
			groupTween.RemoveAt(groupTween.Count - 1);
		}
		
		public void StartSequance(
        TweenPool _pool = null,
        [CallerMemberName] string _memberName = "",
        [CallerFilePath] string _sourceFilePath = "",
        [CallerLineNumber] int _sourceLineNumber = 0){
			addGroupTween(BicTween.Sequance(_pool, _memberName, _sourceFilePath, _sourceLineNumber));
		}

		public void Sequance(Action _sequance,
        TweenPool _pool = null,
        [CallerMemberName] string _memberName = "",
        [CallerFilePath] string _sourceFilePath = "",
        [CallerLineNumber] int _sourceLineNumber = 0){
			this.StartSequance(_pool, _memberName, _sourceFilePath, _sourceLineNumber);
			_sequance();
			this.EndSequance();
		}

		public void Spawn(Action _spawn,
        TweenPool _pool = null,
        [CallerMemberName] string _memberName = "",
        [CallerFilePath] string _sourceFilePath = "",
        [CallerLineNumber] int _sourceLineNumber = 0){
			this.StartSpwan(_pool, _memberName, _sourceFilePath, _sourceLineNumber);
			_spawn();
			this.EndSpawn();
		}

		public void EndSequance(){
			if(Current.Type != TweenType.Sequance){
				throw new SystemException("Current Tween is not Sequance");
			}

			removeLastGroupTween();
		}

		public void StartSpwan(
            TweenPool _pool = null,
            [CallerMemberName] string _memberName = "",
            [CallerFilePath] string _sourceFilePath = "",
            [CallerLineNumber] int _sourceLineNumber = 0
        ){
			addGroupTween(BicTween.Spawn(_pool, _memberName, _sourceFilePath, _sourceLineNumber));
		}

		public void EndSpawn(){
			if(Current.Type != TweenType.Spawn){
				throw new SystemException("Current Tween is not Spawn");
			}

			removeLastGroupTween();
		}

		public void AddChild(TweenModel _tween){
			groupTween[groupTween.Count - 1].AddChild(_tween);
		}

		public TweenModel Make(){
			if(groupTween.Count != 0){
				throw new SystemException("MultiTween Count is " + groupTween.Count.ToString());
			}

			var _result = this.mother;
			this.mother = null;
			groupTween.Clear();
			return _result;
		}

		public TweenModel Play(){
			return Make().Play();
		}
	}
}


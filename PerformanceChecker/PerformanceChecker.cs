using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BicUtil.PerformanceChecker{
	public class PerformanceChecker : BicUtil.SingletonBase.SingletonBase<PerformanceChecker> {
		private System.Diagnostics.Stopwatch stopwatch;

		public PerformanceChecker(){
			#if UNITY_EDITOR
			Debug.LogWarning("[PerformanceChecker] INIT");
			#endif

			stopwatch = new System.Diagnostics.Stopwatch();
		}

		public long SpeedTest(Action _testFunc, int _loop = 10000000){
			stopwatch.Reset();
			stopwatch.Start();
			for(int i = 0; i < _loop; i++){
				_testFunc();
			}

			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}

		public void SpeedTest(string _testName, Action _testFunc, int _loop = 10000000){
			var _time = SpeedTest(_testFunc, _loop);
			Debug.LogWarning("[PerformanceChecker] Time:" + _time.ToString() + " / " + _testName);
		}
	}
}
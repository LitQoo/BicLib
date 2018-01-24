using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;

namespace BicUtil.UIFlow
{

	public interface IUIFlowObject
	{
		void OnOpenedUI(IUIFlowObject _fromUI, object _paramter);
		OnCloseUIResult OnClosedUI (IUIFlowObject _fromUI, Action _finishCallback, object parameter);
		
		GameObject gameObject { get; }
	}

	public enum OnCloseUIResult
	{
		WaitForFinishCallback,
		WaitForFinishCallbackAndFastDisplayNext,
		DoNotWait
	}

}

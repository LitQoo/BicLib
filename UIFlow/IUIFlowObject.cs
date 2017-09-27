using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;

namespace BicUtil.UIFlow
{

	public interface IUIFlowObject
	{
		void OnOpenedUI(object _paramter);
		OnCloseUIResult OnClosedUI (Action _finishCallback, object parameter);
		void Destroy();
		void Disable();
		void Enable();
	}

	public enum OnCloseUIResult
	{
		WaitForFinishCallback,
		DoNotWait
	}

}

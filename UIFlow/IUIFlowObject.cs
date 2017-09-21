using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;

namespace BicUtil.UIFlow
{

	public interface IUIFlowObject
	{
		void OnOpenedUI(object _paramter);
		void OnClosedUI (object parameter);
		void Destroy();
		void Disable();
		void Enable();
	}

}

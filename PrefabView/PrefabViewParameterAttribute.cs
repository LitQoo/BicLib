using System.Collections;
using System.Collections.Generic;
using BicUtil.PrefabView;
using UnityEngine;

namespace BicUtil.PrefabView
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
	public class PrefabViewParameterAttribute : System.Attribute
	{
		public PrefabViewParameterAttribute()
		{
		}
	}
}
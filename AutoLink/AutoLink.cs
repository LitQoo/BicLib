using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.AutoLink{
	[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
	public class AutoLinkAttribute : System.Attribute
	{
		readonly string objectName;

		public AutoLinkAttribute(string _objectName)
		{
			this.objectName = _objectName;
		}

		public string ObjectName
		{
			get { return objectName; }
		}
	}
}
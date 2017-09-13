using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB;

namespace BicUtil.MVCSystem
{

	public interface IModelObject<T>
	{
		T Controller{get;set;}
	}
}
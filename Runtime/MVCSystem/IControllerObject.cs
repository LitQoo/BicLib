using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB;
using BicDB.Container;


namespace BicUtil.MVCSystem
{
	public interface IControllerObject<T> where T : IRecordContainer
	{
		T Model { get;}

		void BindModel(T _model);
	}
}
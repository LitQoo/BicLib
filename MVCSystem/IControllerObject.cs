using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB;


namespace BicUtil.MVCSystem
{
	public interface IControllerObject<T> where T : IRecordContainer
	{
		T Model { get; set; }

		void SetModel(T _model);
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB.Container;

namespace BicDB.Storage {
	abstract public class ScriptableDataBase<T> : ScriptableObject where T : IRecordContainer, new() {
		public List<T> List = new List<T>();
	}

	abstract public class ScriptableRecordContainer : RecordContainer{
		abstract public void ApplyInspectorValue();
	}
}
using System;
using System.Collections.Generic;

namespace BicDB
{
	public interface IStorage
	{
		void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null)  where T : IModel, new();
		void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null)  where T : IModel, new();
	}
}


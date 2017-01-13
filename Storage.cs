using System;

namespace BicDB
{
	public interface IStorage
	{
		void Save<T>(ITable<T> _table, Action<bool> _callback)  where T : IModel, new();
		void Load<T>(ITable<T> _table, Action<bool> _callback)  where T : IModel, new();
	}

}


using System;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Threading.Tasks;

namespace BicDB
{

	public interface IStorageSuppoter{
		void Save(Action<Result> _callback = null, object _parameter = null);
		void Load(Action<Result> _callback = null, object _parameter = null);
		void Pull(Action<Result> _callback = null, object _parameter = null);
	}

	public interface ITableStorageSuppoter : IStorageSuppoter{
		void SetStorage(ITableStorage _storage);
	}

	public interface IDataStoreStorageSuppoter : IStorageSuppoter{
		void SetStorage(IDataStoreStorage _storage);
	}


	public interface IStringParser{
		void BuildTableContainer<T>(ITableContainer<T> _table, ref string _json, ref int _counter) where T : IRecordContainer, new();
		void BuildDataStoreContainer<T>(IDataStoreContainer<T> _table, ref string _json, ref int _counter) where T : IRecordContainer, new();
		void BuildListContainer<T>(IListContainer<T> _list, ref string _json, ref int _counter) where T : IDataBase, new();
		void BuildDictionaryContainer(IDictionaryContainer _dictionary, ref string _json, ref int _counter);
		void BuildModelContainer(IRecordContainer _model, ref string _json, ref int _counter);
		void BuildStringVariable(IVariable _variable, ref string _json, ref int _counter);
		void BuildNumberVariable(IVariable _variable, ref string _json, ref int _counter);	
		IDataBase BuildVariable(ref string _json, ref int _counter);

	}

	public interface IStringFormatter{
		string ToFormattedString<T>(ITableContainer<T> _table) where T : IRecordContainer, new();
		void BuildFormattedString<T>(ITableContainer<T> _table, ref string _json) where T : IRecordContainer, new();
		void BuildFormattedString<T>(IDataStoreContainer<T> _table, ref string _json) where T : IRecordContainer, new();
		void BuildFormattedString<T>(IListContainer<T> _list, ref string _json) where T : IDataBase, new();
		void BuildFormattedString(IDictionaryContainer _dictionary, ref string _json);
		void BuildFormattedString(IRecordContainer _model, ref string _json);
		void BuildFormattedString(IVariable _variable, ref string _json);
		void BuildFormattedString(IDataBase _variable, ref string _json);
	}

	public interface ITableStorage
	{
		void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new();
	}

	public interface IDataStoreStorage
	{
		void Save<T>(IDataStoreContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Load<T>(IDataStoreContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Pull<T>(IDataStoreContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new();
	}
}


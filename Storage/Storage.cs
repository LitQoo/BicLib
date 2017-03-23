using System;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;

namespace BicDB
{

	public interface IStorageSuppoter{
		void Save(Action<Result> _callback = null, object _parameter = null);
		void Load(Action<Result> _callback = null, object _parameter = null);
		void Pull(Action<Result> _callback = null, object _parameter = null);
		void SetStorage(IStorage _storage);
	}


	public interface IStringParser{
		void BuildTableContainer<T>(ITableContainer<T> _table, ref string _json, ref int _counter) where T : IRecordContainer, new();
		void BuildListContainer<T>(IListContainer<T> _list, ref string _json, ref int _counter) where T : IDataBase, new();
		void BuildDictionaryContainer<T>(IDictionaryContainer<T> _dictionary, ref string _json, ref int _counter) where T : IDataBase, new();
		void BuildModelContainer(IRecordContainer _model, ref string _json, ref int _counter);
		void BuildStringVariable(IVariable _variable, ref string _json, ref int _counter);
		void BuildNumberVariable(IVariable _variable, ref string _json, ref int _counter);
		IDataBase BuildVariable(ref string _json, ref int _counter);

	}

	public interface IStringFormatter{
		string ToFormattedString<T>(ITableContainer<T> _table) where T : IRecordContainer, new();
		void BuildFormattedString<T>(ITableContainer<T> _table, ref string _json) where T : IRecordContainer, new();
		void BuildFormattedString<T>(IListContainer<T> _list, ref string _json) where T : IDataBase, new();
		void BuildFormattedString<T>(IDictionaryContainer<T> _dictionary, ref string _json) where T : IDataBase, new();
		void BuildFormattedString(IRecordContainer _model, ref string _json);
		void BuildFormattedString(IVariable _variable, ref string _json);
		void BuildFormattedString(IDataBase _variable, ref string _json);
	}

	public interface IStorage
	{
		void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new();
	}
}


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BicDB.Container;
using BicDB.Variable;

namespace BicDB.Storage
{
	public interface IStorageSuppoter{
		void Save(Action<Result> _callback = null, object _parameter = null);
		void Load(Action<Result> _callback = null, object _parameter = null);
		void Pull(Action<Result> _callback = null, object _parameter = null);
		Task<Result> LoadAsync(object _parameter);
	}

	public interface ITableStorageSuppoter : IStorageSuppoter, IRecordContainerParent{
		void SetStorage(ITableStorage _storage);
	}

	public interface IDataStoreStorageSuppoter : IStorageSuppoter{
		void SetStorage(IDataStoreStorage _storage);
	}

	public interface IStringParser{
		void BuildTableContainer<T>(ITableContainer<T> _table, ref string _json, ref int _counter, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new();
		void BuildDataStoreContainer<T>(IDataStoreContainer<T> _table, ref string _json, ref int _counter, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new();
		void BuildMutableListContainer (IMutableListContainer _list, ref string _json, ref int _counter);
		void BuildListContainer<T> (IListContainer<T> _list, ref string _json, ref int _counter) where T : IDataBase, new();
		void BuildMutableDictionaryContainer(IMutableDictionaryContainer _dictionary, ref string _json, ref int _counter);
		void BuildDictionaryContainer<T>(IDictionaryContainer<T> _dictionary, ref string _json, ref int _counter) where T : IDataBase, new();
		void BuildModelContainer(IRecordContainer _model, ref string _json, ref int _counter);
		void BuildStringVariable(IVariable _variable, ref string _json, ref int _counter);
		void BuildNumberVariable(IVariable _variable, ref string _json, ref int _counter);	
		IDataBase BuildVariable(ref string _json, ref int _counter);

	}

	public interface IStringFormatter{
		string ToFormattedString<T>(ITableContainer<T> _table) where T : IRecordContainer, new();
		void BuildFormattedString<T>(ITableContainer<T> _table, StringBuilder _stringBuilder, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new();
		void BuildFormattedString<T>(IDataStoreContainer<T> _table, StringBuilder _stringBuilder, IMutableDictionaryContainer _option = null) where T : IRecordContainer, new();
		void BuildFormattedString(IMutableListContainer _list, StringBuilder _stringBuilder);
		void BuildFormattedString<T> (IListContainer<T> _list, StringBuilder _stringBuilder) where T : IDataBase, new();
		void BuildFormattedString(IMutableDictionaryContainer _dictionary, StringBuilder _stringBuilder);
		void BuildFormattedString<T> (IDictionaryContainer<T> _dictionary, StringBuilder _stringBuilder) where T : IDataBase, new();
		void BuildFormattedString(IRecordContainer _model, StringBuilder _stringBuilder);
		void BuildFormattedString(IVariable _variable, StringBuilder _stringBuilder);
		void BuildFormattedString(IDataBase _variable, StringBuilder _stringBuilder);
	}

	public interface IStorageParam{

	}
	
	public interface ITableStorage
	{
		string StorageType{get;}
		void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new();
		void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new();
		Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new();
	}

	public interface IDataStoreStorage
	{
		string StorageType{get;}
		void Save<T>(IDataStoreContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Load<T>(IDataStoreContainer<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IRecordContainer, new();
		void Pull<T>(IDataStoreContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new();
	}
}


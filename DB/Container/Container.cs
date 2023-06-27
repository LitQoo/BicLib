using System;
using BicDB.Variable;
using System.Collections.Generic;
using BicDB.Storage;

namespace BicDB.Container
{

	public interface IRecordContainerParent
	{
		#region get&set
		string Name{ get; set;}
		string PrimaryKey{ get; set; }
		#endregion

		#region Header&Property 
		MutableDictionaryContainer Header { get; }
		MutableDictionaryContainer Property { get; }
		#endregion

		IVariable GetRecordKey(IRecordContainer _record);
		void Commit(IRecordContainer _record);
	}

	public interface IDataStoreContainer<T> : IDataBase, IDictionary<string, T>, IRecordContainerParent, IDataStoreStorageSuppoter where T : IRecordContainer
	{
		#region event
		Action<string, T> OnAddedRowActions { get; set; }
		Action<string, T> OnRemovedRowActions { get; set;}
		#endregion
	}

	public interface IMutableDictionaryContainer : IDictionaryContainer<IDataBase>
	{

	}

	public interface IDictionaryContainer<T> : IDataBase, IDictionary<string, T> where T : IDataBase{
		Action<string, T> OnAddedRowActions { get; set; }
		Action<string, T> OnRemovedRowActions { get; set;}
	}

	public interface IMutableListContainer : IListContainer<IDataBase> {

	}

	public interface ITrackedListContainer{
		object GetObject(int _index);
		int ObjectCount{get;}
	}
	public interface IListContainer<T> : IDataBase, IList<T>, ITrackedListContainer where T : IDataBase{
		event Action<T> OnAddedValueActions;
		event Action OnClearedValueActions;
		OnChangedElementDelegator<int, T> OnChangedElementActions { get; set;}
		int IndexOf(Func<T,bool> _find);
	}

	public interface IRecordContainer :  IDictionary<string, IDataBase>, IDataBase, IBindRmover{
		IRecordContainerParent Parent{ get; set; }
		Action<IRecordContainer, string> OnChangedValueActions{ get; set;}

		void NotifyChanged(string _message = "");
		void AddManagedColumn(string _key, IDataBase _value);
		void MergeCopyBy(IRecordContainer _model);
		void Commit();
		bool ParseJson(string _json);
	}

	public interface ITableContainer<T> : IRecordContainerParent, ITableStorageSuppoter, IDataBase, ITrackedListContainer, IList<T> where T : IRecordContainer
	{
		#region event
		Action<T> OnAddedRowActions { get; set; }
		Action<T> OnRemovedRowActions { get; set;}
		Action OnChangeCountActions { get; set; }
		Action<Result> OnSave { get; set; }

		event Action OnSetup;
		event Action<string, string> OnMigration;
		event Action OnHashCodeError;
		#endregion

		#region Logic
		void AddWithoutDuplication(T _item);
		int IndexOf(Func<T,bool> _find);
		#endregion
	}

	public interface IQueryTable : IStorageSuppoter{
		IRecordContainer RecordAt(int _index);
		int Count{get;}

	}
}
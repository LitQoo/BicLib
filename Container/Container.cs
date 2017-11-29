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

	public interface IListContainer<T> : IDataBase, IList<T> where T : IDataBase{
		event Action<T> OnAddedValueActions;
		event Action OnClearedValueActions;
		OnChangedElementDelegator<int, T> OnChangedElementActions { get; set;}
	}

	public interface IRecordContainer :  IDictionary<string, IDataBase>, IDataBase, IBindRmover{
		IRecordContainerParent Parent{ get; set; }
		Action<IRecordContainer, string> OnChangedValueActions{ get; set;}

		void NotifyChanged(string _message = "");
		void AddManagedColumn(string _key, IDataBase _value);
		void CopyBy(IRecordContainer _model);
	}

	public interface ITableContainer<T> : IDataBase, IList<T>, IRecordContainerParent, ITableStorageSuppoter where T : IRecordContainer
	{
		#region event
		Action<T> OnAddedRowActions { get; set; }
		Action<T> OnRemovedRowActions { get; set;}

		event Action OnSetup;
		event Action<string, string> OnMigration;
		event Action OnHashCodeError;
		#endregion
	}
}
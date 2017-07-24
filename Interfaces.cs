using UnityEngine;
using System.Collections;
using BicDB.Container;
using System;
using System.Linq;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IDataBase
	{
		void BuildVariable(ref string _json, ref int _counter, IStringParser _parser);
		void BuildFormattedString(ref string _json, IStringFormatter _formatter);
		T As<T>() where T : class, IDataBase;

		DataType Type { get; }
		IVariable AsVariable{ get; }

	}

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

	public enum DataType
	{
		Int,
		Float,
		String,
		Bool,
		List,
		Dictionary,
		Record,
		Table,
		DataStore,
		Object,
		None
	}


	public class VariableBase{
		public event Action<IVariable, string> OnChangedValueActions = delegate{};

		public void NotifyChanged(string _message = ""){
			OnChangedValueActions (this as IVariable, _message);
		}

		public bool IsEqual(IVariable _variable){
			return VariableUtil.IsEqual(this as IVariable, _variable);
		}

		public void ClearOnChangedValueActions(){
			OnChangedValueActions = delegate{};
		}
	}

	public class Result{
		public int Code = 0;
		public string Message = "";

		public Result(int _code, string _message = ""){
			Code = _code;
			Message = _message;
		}
	}


	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
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

	public interface IRecordContainer :  IDictionary<string, IDataBase>, IDataBase{
		IRecordContainerParent Parent{ get; set; }
		Action<IRecordContainer, string> OnChangedValueActions{ get; set;}
		void NotifyChanged(string _message = "");

		void AddManagedColumn(string _key, IDataBase _value);
		void CopyBy(IRecordContainer _model);
		void ClearOnChangedValueActions();
	}

	public interface ITableContainer<T> : IDataBase, IList<T>, IRecordContainerParent, ITableStorageSuppoter where T : IRecordContainer
	{
		#region event
		Action<T> OnAddedRowActions { get; set; }
		Action<T> OnRemovedRowActions { get; set;}
		#endregion
	}

	public interface IEnumVariable<T> : IVariable where  T : struct
	{
		new event Action<IEnumVariable<T>> OnChangedValueActions;

		OnChangedValueToDelegator<T> OnSetValueActions{ get; set;}
		T AsEnum{ get; set; }
	}

	public interface IVariable : IDataBase{
		event Action<IVariable, string> OnChangedValueActions;

		void NotifyChanged(string _message = "");
		bool IsEqual(IVariable _variable);
		void ClearOnChangedValueActions ();

		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		bool AsBool{ get; set; }
	}

}

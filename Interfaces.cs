using UnityEngine;
using System.Collections;
using BicDB.Container;
using System;
using System.Linq;
using System.Collections.Generic;
using BicDB.Variable;
using BicDB.Storage;

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

	public enum DataType
	{
		Int,
		Float,
		String,
		Bool,
		Enum,
		List,
		Dictionary,
		Record,
		Table,
		DataStore,
		Object,
		None
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



	public interface IBindRmover{
		void ClearNotifyAndBinding ();
	}

}

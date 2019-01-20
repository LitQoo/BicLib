using UnityEngine;
using System.Collections;
using BicDB.Container;
using System;
using System.Linq;
using System.Collections.Generic;
using BicDB.Variable;
using BicDB.Storage;
using System.Text;

namespace BicDB
{
	public interface IDataBase
	{
		void BuildVariable(ref string _json, ref int _counter, IStringParser _parser);
		void BuildFormattedString(StringBuilder _stringBuilder, IStringFormatter _formatter);
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
		public int HashCode = 0;

		public Result(int _code, int _hashCode = 0, string _message = ""){
			Code = _code;
			Message = _message;
			HashCode = _hashCode;
		}
	}


	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}



	public interface IBindRmover{
		void ClearNotifyAndBinding ();
	}

}

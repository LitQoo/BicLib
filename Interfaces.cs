using UnityEngine;
using System.Collections;
using BicDB.Variable;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BicDB
{
	public interface IDataBase
	{
		void BuildVariable(ref string _json, ref int _counter, IStringParser _parser);
		void BuildFormattedString(ref string _json, IStringFormatter _formatter);

		DataType Type { get; }
	}


	public enum DataType
	{
		Int,
		Float,
		String,
		Bool,
		List,
		Dictionary,
		Model,
		Table
	}


	public class VariableBase{
		public event Action<IVariable, string> OnChangedValueActions = delegate{};

		public void NotifyChanged(string _message = ""){
			OnChangedValueActions (this as IVariable, _message);
		}

		public bool IsEqual(IVariable _variable){
			return VariableUtil.IsEqual(this as IVariable, _variable);
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

	public interface ILinqSupporter<T>
	{

		IEnumerable<T> Where(Func<T, bool> _func);
		T FirstOrDefault(Func<T, bool> _func);
		IEnumerable<U> Select<U>(Func<T, U> _func);
		IOrderedEnumerable<T> OrderBy<U>(Func<T, U> _func);
		IOrderedEnumerable<T> OrderByDescending<U>(Func<T, U> _func);
	}


}

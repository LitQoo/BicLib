using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IStringParser{
		void BuildTableVariable<T>(ITable<T> _table, ref string _json, ref int _counter) where T : IModelVariable, new();
		void BuildListVariable<T>(IListVariable<T> _list, ref string _json, ref int _counter) where T : IVariableBase, new();
		void BuildDictionaryVariable<T>(IDictionaryVariable<T> _dictionary, ref string _json, ref int _counter) where T : IVariableBase, new();
		void BuildModelVariable(IModelVariable _model, ref string _json, ref int _counter);
		void BuildStringVariable(IVariable _variable, ref string _json, ref int _counter);
		void BuildNumberVariable(IVariable _variable, ref string _json, ref int _counter);
		IVariableBase BuildVariable(ref string _json, ref int _counter);

	}

	public interface IStringFormatter{
		void BuildFormattedString<T>(ITable<T> _table, ref string _json) where T : IModelVariable, new();
		void BuildFormattedString<T>(IListVariable<T> _list, ref string _json) where T : IVariableBase, new();
		void BuildFormattedString<T>(IDictionaryVariable<T> _dictionary, ref string _json) where T : IVariableBase, new();
		void BuildFormattedString(IModelVariable _model, ref string _json);
		void BuildFormattedString(IVariable _variable, ref string _json);
		void BuildFormattedString(IVariableBase _variable, ref string _json);
	}

	public interface IStorage
	{
		void Save<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IModelVariable, new();
		void Load<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null)  where T : IModelVariable, new();
	}
}


using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;
using System;

namespace BicDB.Container
{
	public class ForeignRecord<T> where T : class, IRecordContainer{
		private ITableContainer<T> targetTable;
		private string targetFieldName;
		private IVariableReadOnly idVariable;
		private string lastIdValue = "_-_n_o-n_e_-_";
		private T cached = null;
		private Func<IVariableReadOnly,T> defaultFunc = null;

		public ForeignRecord(ITableContainer<T> _targetTable, string _findFieldName, IVariableReadOnly _idVariable, Func<IVariableReadOnly, T> _defaultFunc = null){
			targetTable = _targetTable;
			targetFieldName = _findFieldName;
			idVariable = _idVariable;
			defaultFunc = _defaultFunc;
		}

		public T Record{
			get{
				if(lastIdValue != idVariable.AsString){
					findRecord(this.idVariable);
				}

				return cached;
			}
		}

		private void findRecord(IVariableReadOnly _idVariable){
			lastIdValue = _idVariable.AsString;
			cached = targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsString == idVariable.AsString);

			if(cached == null && defaultFunc != null){
				cached = defaultFunc(_idVariable);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;

namespace BicDB.Container
{
	public class ForeignRecord<T> where T : IRecordContainer{
		private ITableContainer<T> targetTable;
		private string targetFieldName;
		private IVariable findVariable;
		public ForeignRecord(ITableContainer<T> _targetTable, string _findFieldName, IVariable _findVariable){
			targetTable = _targetTable;
			targetFieldName = _findFieldName;
			findVariable = _findVariable;
		}

		T Value{
			get{
				return targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsString == findVariable.AsString);
			}
		}

	}
}
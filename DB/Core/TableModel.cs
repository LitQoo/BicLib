using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;

namespace BicDB.Core
{
	public class TableModel : RecordContainer{
		#region Field
		public StringVariable Name = new StringVariable("");
		public StringVariable StorageType = new StringVariable("");
		public IntVariable SaveCount = new IntVariable(0);
		public IntVariable HashCode = new IntVariable(0);
		public IntVariable ErrorCount = new IntVariable(0);
		#endregion

		#region InstantData
		public IQueryTable Table;
		public bool IsFirstSetup = false;
		#endregion

		#region LifeCycle
		public TableModel(){
			AddManagedColumn ("name", Name);
			AddManagedColumn ("storage", StorageType);
			AddManagedColumn ("saveCount", SaveCount);
			AddManagedColumn ("hashCode", HashCode);
			AddManagedColumn ("errorCount", ErrorCount);
		}
		#endregion
	}
}
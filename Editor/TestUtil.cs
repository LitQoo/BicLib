using UnityEngine;
using System.Collections;
using BicDB.Variable;


namespace BicDB
{
	public class TestUtil{

	}

	public class TestStringModel : ModelVariable
	{
		static public string COLUMN_NAME = "data";

		public StringVariable Data = new StringVariable("");

		public TestStringModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}



	public class TestIntModel : ModelVariable
	{
		static public string COLUMN_NAME = "data";

		public IntVariable Data = new IntVariable(0);

		public TestIntModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}


	public class TestWebModel : BicDB.ModelVariable{
		public IVariableBase AndroidLink = new StringVariable("");
		public IVariableBase Assetbundle = new StringVariable("");
		public IVariableBase IosLink = new StringVariable("");
		public IVariableBase Title = new StringVariable("");
		public IVariableBase ViewWeight = new IntVariable(0);
		public IListVariable<StringVariable> Images = new ListVariable<StringVariable>();

		public TestWebModel(){
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IVariableBase);
		}
	}

	public class TestSyncModel : BicDB.ModelVariable{
		public IVariableBase Key = new IntVariable(0);
		public IVariableBase AndroidLink = new StringVariable("");
		public IVariableBase Assetbundle = new StringVariable("");
		public IVariableBase IosLink = new StringVariable("");
		public IVariableBase Title = new StringVariable("");
		public IVariableBase ViewWeight = new IntVariable(0);
		public IListVariable<StringVariable> Images = new ListVariable<StringVariable>();

		public TestSyncModel(){
			AddManagedColumn ("key", Key);
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IVariableBase);
		}
	}


}
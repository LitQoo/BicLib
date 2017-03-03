using UnityEngine;
using System.Collections;
using BicDB.Variable;


namespace BicDB
{
	public class TestUtil{

	}

	public class TestStringModel : Model
	{
		static public string COLUMN_NAME = "data";

		public StringVariable Data = new StringVariable("");

		public TestStringModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}



	public class TestIntModel : Model
	{
		static public string COLUMN_NAME = "data";

		public IntVariable Data = new IntVariable(0);

		public TestIntModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}


	public class TestWebModel : BicDB.Model{
		public IVariable AndroidLink = new StringVariable("");
		public IVariable Assetbundle = new StringVariable("");
		public IVariable IosLink = new StringVariable("");
		public IVariable Title = new StringVariable("");
		public IVariable ViewWeight = new IntVariable(0);
		public IListVariable<StringVariable> Images = new ListVariable<StringVariable>();

		public TestWebModel(){
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IVariable);
		}
	}

	public class TestSyncModel : BicDB.Model{
		public IVariable Key = new IntVariable(0);
		public IVariable AndroidLink = new StringVariable("");
		public IVariable Assetbundle = new StringVariable("");
		public IVariable IosLink = new StringVariable("");
		public IVariable Title = new StringVariable("");
		public IVariable ViewWeight = new IntVariable(0);
		public IListVariable<StringVariable> Images = new ListVariable<StringVariable>();

		public TestSyncModel(){
			AddManagedColumn ("key", Key);
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IVariable);
		}
	}


}
using UnityEngine;
using System.Collections;
using BicDB.Container;
using BicDB.Variable;


namespace BicDB
{
	public class TestUtil{

	}

	public class TestStringModel : ModelContainer
	{
		static public string COLUMN_NAME = "data";

		public StringVariable Data = new StringVariable("");

		public TestStringModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}



	public class TestIntModel : ModelContainer
	{
		static public string COLUMN_NAME = "data";

		public IntVariable Data = new IntVariable(0);

		public TestIntModel(){
			AddManagedColumn(COLUMN_NAME, Data);
		}
	}


	public class TestWebModel : ModelContainer{
		public StringVariable AndroidLink = new StringVariable("");
		public StringVariable Assetbundle = new StringVariable("");
		public StringVariable IosLink = new StringVariable("");
		public StringVariable Title = new StringVariable("");
		public IntVariable ViewWeight = new IntVariable(0);
		public ListContainer<StringVariable> Images = new ListContainer<StringVariable>();

		public TestWebModel(){
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IDataBase);
		}
	}

	public class TestSyncModel : ModelContainer{
		public IntVariable Key = new IntVariable(0);
		public StringVariable AndroidLink = new StringVariable("");
		public StringVariable Assetbundle = new StringVariable("");
		public StringVariable IosLink = new StringVariable("");
		public StringVariable Title = new StringVariable("");
		public IntVariable ViewWeight = new IntVariable(0);
		public ListContainer<StringVariable> Images = new ListContainer<StringVariable>();

		public TestSyncModel(){
			AddManagedColumn ("key", Key);
			AddManagedColumn ("androidLink", AndroidLink);
			AddManagedColumn ("assetbundle", Assetbundle);
			AddManagedColumn ("iosLink", IosLink);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images as IDataBase);
		}
	}


}
using UnityEngine;
using System.Collections;
using BicDB.Variable;


namespace BicDB
{
	public class TestUtil{

	}

	public class TestStringModel : Model
	{
		static public string PRIMARY_KEY_NAME = "data";

		public StringVariable Data = new StringVariable("");

		public TestStringModel(){
			AddManagedColumn("data", Data);
		}
	}

	public class TestIntModel : Model
	{
		static public string PRIMARY_KEY_NAME = "data";

		public IntVariable Data = new IntVariable(0);

		public TestIntModel(){
			AddManagedColumn("data", Data);
		}
	}
}
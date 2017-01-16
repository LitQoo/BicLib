using UnityEngine;
using System.Collections;
using BicDB.Variable;


namespace BicDB
{
	public class TestUtil{

	}

	public class TestModel : Model
	{
		static public string PRIMARY_KEY_NAME = "data";

		public StringVariable Data = new StringVariable("");

		public TestModel(){
			AddManagedColumn("data", Data);
		}
	}
}
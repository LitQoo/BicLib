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


}
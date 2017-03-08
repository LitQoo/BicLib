using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace BicDB
{
	public class ManagerTest {

		[SetUp]
		public void Setup(){
			Manager.ClearTables ();
		}

		[Test]
		public void AddTableTest(){
			ITable<ModelVariable> _table = new Table<ModelVariable> ("tablename");
			Manager.AddTable<ModelVariable> (_table);

			var _getTable = Manager.GetTable<ModelVariable> ();

			Assert.AreEqual (_table, _getTable);
		}


		[Test]
		public void AddAlreadyAddedTableTest(){
			ITable<ModelVariable> _table = new Table<ModelVariable> ("tablename");
			Manager.AddTable<ModelVariable> (_table);

			try {
				Manager.AddTable<ModelVariable> (_table);
			} catch (System.Exception) {
				Assert.Pass ();
			}

			Assert.Fail ();

		}

		[Test]
		public void GetTableTestWithoutTablename(){
			ITable<ModelVariable> _table = new Table<ModelVariable> ("tablename");
			Manager.AddTable<ModelVariable> (_table);

			var _getTable = Manager.GetTable<ModelVariable> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestWithTablename(){
			ITable<ModelVariable> _table = new Table<ModelVariable> ("tablename");
			Manager.AddTable<ModelVariable> (_table);

			var _getTable = Manager.GetTable<ModelVariable> ("tablename");

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestMissingTable(){
			
			var _getTable = Manager.GetTable<ModelVariable> ("???");

			Assert.IsNull (_getTable);
		}

		[Test]
		public void CreateTableTest(){
			ITable<ModelVariable> _table = Manager.CreateTable<ModelVariable>("tablename");

			var _getTable = Manager.GetTable<ModelVariable> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void ClearTableTest(){
			ITable<ModelVariable> _table = new Table<ModelVariable> ("tablename");
			Manager.AddTable<ModelVariable> (_table);
			Manager.ClearTables ();
			var _getTable = Manager.GetTable<ModelVariable> ();

			Assert.IsNull (_getTable);
		}
	}
}
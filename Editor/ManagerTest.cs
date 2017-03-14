using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BicDB.Variable;
using BicDB.Container;

namespace BicDB
{
	public class ManagerTest {

		[SetUp]
		public void Setup(){
			Manager.ClearTables ();
		}

		[Test]
		public void AddTableTest(){
			ITableContainer<ModelContainer> _table = new TableContainer<ModelContainer> ("tablename");
			Manager.AddTable<ModelContainer> (_table);

			var _getTable = Manager.GetTable<ModelContainer> ();

			Assert.AreEqual (_table, _getTable);
		}


		[Test]
		public void AddAlreadyAddedTableTest(){
			ITableContainer<ModelContainer> _table = new TableContainer<ModelContainer> ("tablename");
			Manager.AddTable<ModelContainer> (_table);

			try {
				Manager.AddTable<ModelContainer> (_table);
			} catch (System.Exception) {
				Assert.Pass ();
			}

			Assert.Fail ();

		}

		[Test]
		public void GetTableTestWithoutTablename(){
			ITableContainer<ModelContainer> _table = new TableContainer<ModelContainer> ("tablename");
			Manager.AddTable<ModelContainer> (_table);

			var _getTable = Manager.GetTable<ModelContainer> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestWithTablename(){
			ITableContainer<ModelContainer> _table = new TableContainer<ModelContainer> ("tablename");
			Manager.AddTable<ModelContainer> (_table);

			var _getTable = Manager.GetTable<ModelContainer> ("tablename");

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestMissingTable(){
			
			var _getTable = Manager.GetTable<ModelContainer> ("???");

			Assert.IsNull (_getTable);
		}

		[Test]
		public void CreateTableTest(){
			ITableContainer<ModelContainer> _table = Manager.CreateTable<ModelContainer>("tablename");

			var _getTable = Manager.GetTable<ModelContainer> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void ClearTableTest(){
			ITableContainer<ModelContainer> _table = new TableContainer<ModelContainer> ("tablename");
			Manager.AddTable<ModelContainer> (_table);
			Manager.ClearTables ();
			var _getTable = Manager.GetTable<ModelContainer> ();

			Assert.IsNull (_getTable);
		}
	}
}
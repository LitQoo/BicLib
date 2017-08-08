using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BicDB.Container;
using BicDB.Variable;

namespace BicDB
{
	public class ManagerTest {

		[SetUp]
		public void Setup(){
			Manager.ClearTables ();
		}

		[Test]
		public void AddTableTest(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer> ("tablename");
			Manager.AddTable<RecordContainer> (_table);

			var _getTable = Manager.GetTable<RecordContainer> ();

			Assert.AreEqual (_table, _getTable);
		}


		[Test]
		public void AddAlreadyAddedTableTest(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer> ("tablename");
			Manager.AddTable<RecordContainer> (_table);

			try {
				Manager.AddTable<RecordContainer> (_table);
			} catch (System.Exception) {
				Assert.Pass ();
			}

			Assert.Fail ();

		}

		[Test]
		public void GetTableTestWithoutTablename(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer> ("tablename");
			Manager.AddTable<RecordContainer> (_table);

			var _getTable = Manager.GetTable<RecordContainer> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestWithTablename(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer> ("tablename");
			Manager.AddTable<RecordContainer> (_table);

			var _getTable = Manager.GetTable<RecordContainer> ("tablename");

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestMissingTable(){
			
			var _getTable = Manager.GetTable<RecordContainer> ("???");

			Assert.IsNull (_getTable);
		}

		[Test]
		public void CreateTableTest(){
			ITableContainer<RecordContainer> _table = Manager.CreateTable<RecordContainer>("tablename");

			var _getTable = Manager.GetTable<RecordContainer> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void ClearTableTest(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer> ("tablename");
			Manager.AddTable<RecordContainer> (_table);
			Manager.ClearTables ();
			var _getTable = Manager.GetTable<RecordContainer> ();

			Assert.IsNull (_getTable);
		}
	}
}
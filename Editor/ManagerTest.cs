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
			ITable<Model> _table = new Table<Model> ("tablename");
			Manager.AddTable<Model> (_table);

			var _getTable = Manager.GetTable<Model> ();

			Assert.AreEqual (_table, _getTable);
		}


		[Test]
		public void AddAlreadyAddedTableTest(){
			ITable<Model> _table = new Table<Model> ("tablename");
			Manager.AddTable<Model> (_table);

			try {
				Manager.AddTable<Model> (_table);
			} catch (System.Exception) {
				Assert.Pass ();
			}

			Assert.Fail ();

		}

		[Test]
		public void GetTableTestWithoutTablename(){
			ITable<Model> _table = new Table<Model> ("tablename");
			Manager.AddTable<Model> (_table);

			var _getTable = Manager.GetTable<Model> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestWithTablename(){
			ITable<Model> _table = new Table<Model> ("tablename");
			Manager.AddTable<Model> (_table);

			var _getTable = Manager.GetTable<Model> ("tablename");

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void GetTableTestMissingTable(){
			
			var _getTable = Manager.GetTable<Model> ("???");

			Assert.IsNull (_getTable);
		}

		[Test]
		public void CreateTableTest(){
			ITable<Model> _table = Manager.CreateTable<Model>("tablename");

			var _getTable = Manager.GetTable<Model> ();

			Assert.AreEqual (_table, _getTable);
		}

		[Test]
		public void ClearTableTest(){
			ITable<Model> _table = new Table<Model> ("tablename");
			Manager.AddTable<Model> (_table);
			Manager.ClearTables ();
			var _getTable = Manager.GetTable<Model> ();

			Assert.IsNull (_getTable);
		}
	}
}
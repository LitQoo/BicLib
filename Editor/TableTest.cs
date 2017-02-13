using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace BicDB
{
	public class TableTest {
		[Test]
		public void GetRowSizeTest(){
			var _table = new Table<Model>("tablename");
			_table.AddRow(new Model());

			Assert.AreEqual(_table.GetSize(), 1);
		}

		[Test]
		public void GetRowTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);

			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void IndexerTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);



			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);

		}

		[Test]
		public void AddRowTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);

			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void InsertRow1(){
			var _table = new Table<Model>("tablename");
			var _model1 = new Model();
			_table.AddRow(_model1);
			var _model2 = new Model();
			_table.AddRow(_model2);
			var _model3 = new Model();
			_table.AddRow(_model3);

			var _model4 = new Model();
			_table.InsertRow(1, _model4);

			var _checkRow = _table[1];

			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model4, _checkRow);
		}


		[Test]
		public void OnAddedRowTest(){
			var _table = new Table<TestIntModel>("tablename");
			bool _isCalled = false;
			_table.OnAddedRow += (TestIntModel _row) => {
				Assert.AreEqual(_row.Data.AsInt, 123);
				_isCalled = true;
			};

			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 123;
			_table.AddRow(_model1);

			Assert.AreEqual (_isCalled, true);
		}

		[Test]
		public void OnRemovingRowTest(){
			var _table = new Table<TestIntModel>("tablename");
			bool _isCalled = false;
			_table.OnRemovingRow += (TestIntModel _row) => {
				Assert.AreEqual(_row.Data.AsInt, 123);
				_isCalled = true;
			};

			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 123;
			_table.AddRow(_model1);

			_table.RemoveRow (0);

			Assert.AreEqual (_isCalled, true);
		}


		[Test]
		public void ClearTest(){
			var _table = new Table<Model>("tablename");
			var _model1 = new Model();
			_table.AddRow(_model1);
			var _model2 = new Model();
			_table.AddRow(_model2);
			var _model3 = new Model();
			_table.AddRow(_model3);


			Assert.AreEqual (_table.GetSize (), 3);

			_table.Clear();

			Assert.AreEqual (_table.GetSize (), 0);
		}


		[Test]
		public void RemoveRowTest1(){
			var _table = new Table<Model>("tablename");
			var _model1 = new Model();
			_table.AddRow(_model1);
			var _model2 = new Model();
			_table.AddRow(_model2);
			var _model3 = new Model();
			_table.AddRow(_model3);

			_table.RemoveRow(1);

			var _checkRow = _table[1];


			Assert.AreEqual (_table.GetSize (), 2);
			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model3, _checkRow);
		}

		[Test]
		public void RemoveRowTest2(){
			var _table = new Table<TestIntModel>("tablename");
			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 1;
			_table.AddRow(_model1);
			var _model2 = new TestIntModel();
			_model2.Data.AsInt = 2;
			_table.AddRow(_model2);
			var _model3 = new TestIntModel();
			_model3.Data.AsInt = 3;
			_table.AddRow(_model3);

			_table.RemoveRow(_row=>_row.Data.AsInt == 2);

			var _checkRow = _table[1];

			Assert.AreEqual (_table.GetSize (), 2);
			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model3, _checkRow);

		}

		[Test]
		public void RemoveRowTest3(){
			var _table = new Table<TestIntModel>("tablename");
			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 1;
			_table.AddRow(_model1);
			var _model2 = new TestIntModel();
			_model2.Data.AsInt = 2;
			_table.AddRow(_model2);
			var _model3 = new TestIntModel();
			_model3.Data.AsInt = 3;
			_table.AddRow(_model3);

			_table.RemoveRow(_model2);

			var _checkRow = _table[1];

			Assert.AreEqual (_table.GetSize (), 2);
			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model3, _checkRow);

		}

		[Test]
		public void WhereTest(){
			var _testValue = 123;
			var _table = new Table<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.AddRow(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.AddRow(_model2);

			var _result = _table.Where(_row => _row.Data.AsInt == _testValue);

			foreach (var _item in _result) {
				Assert.AreEqual(_item, _model2);
			}
		}


		[Test]
		public void FirstOrDefaultTest(){
			var _testValue = 123;
			var _table = new Table<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.AddRow(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.AddRow(_model2);

			var _result = _table.FirstOrDefault(_row => _row.Data.AsInt == _testValue);

			Assert.AreEqual(_result, _model2);
		}

		[Test]
		public void SelectTest(){
			var _testValue = 123;
			var _table = new Table<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.AddRow(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.AddRow(_model2);

			var _result = _table.Select(_row => _row);

			List<TestStringModel> _list = new List<TestStringModel>(_result);

			Assert.AreEqual(_list[0], _model1);
			Assert.AreEqual(_list[1], _model2);
			Assert.AreEqual(_list.Count, 2);
		}

		[Test]
		public void SetStorageTest(){
			var _table = new Table<Model>("tablename");
			var _storage = Substitute.For<IStorage>();
			Action<bool> _callback = (bool _isSuccess) => {
			};


			_table.SetStorage(_storage);
			_table.Save(_callback);

			_storage.Received().Save<Model>(_table, _callback);
			Assert.Pass();
		}

		[Test]
		public void SaveTest(){
			var _table = new Table<Model>("tablename");
			var _storage = Substitute.For<IStorage>();
			Action<bool> _callback = (bool _isSuccess) => {
			};


			_table.SetStorage(_storage);
			_table.Save(_callback);

			_storage.Received().Save<Model>(_table, _callback);
			Assert.Pass();
		}

		[Test]
		public void LoadTest(){
			var _table = new Table<Model>("tablename");
			var _storage = Substitute.For<IStorage>();
			Action<bool> _callback = (bool _isSuccess) => {
			};


			_table.SetStorage(_storage);
			_table.Load(_callback);

			_storage.Received().Load<Model>(_table, _callback);
			Assert.Pass();
		}
	}
}
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using NSubstitute;
using System;

namespace BicDB
{
	public class TableTest {
		[Test]
		public void GetRowSize(){
			var _table = new Table<Model>("tablename");
			_table.AddRow(new Model());

			Assert.AreEqual(_table.GetRowSize(), 1);
		}

		[Test]
		public void GetRowTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);

			var _checkRow = _table.GetRow(0);

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void indexerTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);



			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);

		}

		[Test]
		public void SetPrimaryColumnTest(){
			var _testValue = 123;
			var _table = new Table<TestModel>("tablename", TestModel.PRIMARY_KEY_NAME);
			var _model = new TestModel();
			_model.Data.AsInt = _testValue;
			_table.AddRow(_model);

			var _checkRow = _table.FindRow(_testValue);

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void AddRowTest(){
			var _table = new Table<Model>("tablename");
			var _model = new Model();
			_table.AddRow(_model);

			var _checkRow = _table.GetRow(0);

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void FindRowTestWhenFoundWithInt(){
			var _testValue = 123;
			var _table = new Table<TestModel>("tablename", TestModel.PRIMARY_KEY_NAME);
			var _model = new TestModel();
			_model.Data.AsInt = _testValue;
			_table.AddRow(_model);

			var _checkRow = _table.FindRow(_testValue);

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void FindRowTestWhenNotFoundWithInt(){
			var _testValue = 123;
			var _table = new Table<TestModel>("tablename", TestModel.PRIMARY_KEY_NAME);
			var _model = new TestModel();
			_model.Data.AsInt = _testValue;
			_table.AddRow(_model);

			var _checkRow = _table.FindRow(_testValue + 1);

			Assert.IsNull(_checkRow);
		}

		[Test]
		public void FindRowTestWhenFoundWithFloat(){
			Assert.Fail();
		}

		[Test]
		public void FindRowTestWhenNotFoundWithFloat(){
			Assert.Fail();
		}

		[Test]
		public void FindRowTestWhenFoundWithString(){
			Assert.Fail();
		}

		[Test]
		public void FindRowTestWhenNotFoundWithString(){
			Assert.Fail();
		}

		[Test]
		public void WhereTest(){
			var _primaryKeyName = "data";
			var _testValue = 123;
			var _table = new Table<TestModel>("tablename", _primaryKeyName);

			var _model1 = new TestModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.AddRow(_model1);

			var _model2 = new TestModel();
			_model2.Data.AsInt = _testValue;
			_table.AddRow(_model2);

			var _result = _table.Where(_row => _row.Data.AsInt == _testValue);

			foreach (var _item in _result) {
				Assert.AreEqual(_item, _model2);
			}
		}


		[Test]
		public void FirstOrDefaultTest(){
			var _primaryKeyName = "data";
			var _testValue = 123;
			var _table = new Table<TestModel>("tablename", _primaryKeyName);

			var _model1 = new TestModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.AddRow(_model1);

			var _model2 = new TestModel();
			_model2.Data.AsInt = _testValue;
			_table.AddRow(_model2);

			var _result = _table.FirstOrDefault(_row => _row.Data.AsInt == _testValue);

			Assert.AreEqual(_result, _model2);
		}

		[Test]
		public void SelectTest(){
			Assert.Fail();
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
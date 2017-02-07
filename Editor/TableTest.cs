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
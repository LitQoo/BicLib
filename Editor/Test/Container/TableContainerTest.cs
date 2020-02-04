using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using NSubstitute;
using System;
using System.Collections.Generic;
using BicDB.Variable;
using System.Linq;
using BicDB.Storage;
using BicDB.Core;

namespace BicDB.Container
{
	[TestFixture]
	public class TableTest {
		public TableTest(){
			TableService.Load(_result=>{});
		}

		[Test]
		public void GetRowSizeTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			_table.Add(new RecordContainer());

			Assert.AreEqual(_table.Count, 1);
		}

		[Test]
		public void GetRowTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model = new RecordContainer();
			_table.Add(_model);

			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void IndexerTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model = new RecordContainer();
			_table.Add(_model);



			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);

		}

		[Test]
		public void AddRowTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model = new RecordContainer();
			_table.Add(_model);

			var _checkRow = _table[0];

			Assert.AreEqual(_model, _checkRow);
		}

		[Test]
		public void AddRowWithoutDuplicationTest(){
			var _table = new TableContainer<TestWebModel>("tablename");
			_table.PrimaryKey = "title";
			
			var _model1 = new TestWebModel();
			_model1.Title.AsString = "key";
			_table.AddWithoutDuplication(_model1);

			var _checkRow = _table[0];
			Assert.AreEqual(_model1, _checkRow);


			var _model2 = new TestWebModel();
			_model2.Title.AsString = "key";
			_table.AddWithoutDuplication(_model2);

			Assert.AreEqual(_table.Count, 1);


			var _model3 = new TestWebModel();
			_model3.Title.AsString = "key2";
			_table.AddWithoutDuplication(_model3);

			Assert.AreEqual(_table.Count, 2);
		}

		[Test]
		public void AddRowWithoutDuplicationTest2(){
			var _table = new TableContainer<TestWebModel>("tablename");
			
			Assert.Throws<Exception>(()=>{
				var _model1 = new TestWebModel();
				_model1.Title.AsString = "key";
				_table.AddWithoutDuplication(_model1);
			});

			_table.PrimaryKey = "title";

			Assert.DoesNotThrow(()=>{
				var _model1 = new TestWebModel();
				_model1.Title.AsString = "key";
				_table.AddWithoutDuplication(_model1);
			});
		}

		[Test]
		public void InsertRow1(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model1 = new RecordContainer();
			_model1.Add("key1", new StringVariable("test1"));
			_table.Add(_model1);
			var _model2 = new RecordContainer();
			_model2.Add("key1", new StringVariable("test2"));
			_table.Add(_model2);
			var _model3 = new RecordContainer();
			_model3.Add("key1", new StringVariable("test3"));
			_table.Add(_model3);

			var _model4 = new RecordContainer();
			_model4.Add("key1", new StringVariable("test4"));
			_table.Insert(1, _model4);

			var _checkRow = _table[1];

			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model4, _checkRow);
		}


		[Test]
		public void OnAddedRowTest(){
			var _table = new TableContainer<TestIntModel>("tablename");
			bool _isCalled = false;
			_table.OnAddedRowActions += (TestIntModel _row) => {
				Assert.AreEqual(_row.Data.AsInt, 123);
				_isCalled = true;
			};

			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 123;
			_table.Add(_model1);

			Assert.AreEqual (_isCalled, true);
		}

		[Test]
		public void OnRemovingRowTest(){
			var _table = new TableContainer<TestIntModel>("tablename");
			bool _isCalled = false;
			_table.OnAddedRowActions += (TestIntModel _row) => {
				Assert.AreEqual(_row.Data.AsInt, 123);
				_isCalled = true;
			};

			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 123;
			_table.Add(_model1);

			_table.RemoveAt (0);

			Assert.AreEqual (_isCalled, true);
		}


		[Test]
		public void ClearTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model1 = new RecordContainer();
			_table.Add(_model1);
			var _model2 = new RecordContainer();
			_table.Add(_model2);
			var _model3 = new RecordContainer();
			_table.Add(_model3);


			Assert.AreEqual (_table.Count, 3);

			_table.Clear();

			Assert.AreEqual (_table.Count, 0);
		}


		[Test]
		public void RemoveRowTest1(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _model1 = new RecordContainer();
			_model1["key1"] = new StringVariable("test");
			_table.Add(_model1);
			var _model2 = new RecordContainer();
			_model2["key1"] = new StringVariable("test");
			_table.Add(_model2);
			var _model3 = new RecordContainer();
			_model1["key1"] = new StringVariable("test");
			_table.Add(_model3);

			_table.RemoveAt(1);

			var _checkRow = _table[1];


			Assert.AreEqual (_table.Count, 2);
			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model3, _checkRow);
		}

		[Test]
		public void RemoveRowTest2(){
//			var _table = new TableContainer<TestIntModel>("tablename");
//			var _model1 = new TestIntModel();
//			_model1.Data.AsInt = 1;
//			_table.Add(_model1);
//			var _model2 = new TestIntModel();
//			_model2.Data.AsInt = 2;
//			_table.Add(_model2);
//			var _model3 = new TestIntModel();
//			_model3.Data.AsInt = 3;
//			_table.Add(_model3);
//
//			_table.Remove(_row=>_row.Data.AsInt == 2);
//
//			var _checkRow = _table[1];
//
//			Assert.AreEqual (_table.Count, 2);
//			Assert.AreNotEqual(_model2, _checkRow);
//			Assert.AreEqual(_model3, _checkRow);

		}

		[Test]
		public void RemoveRowTest3(){
			var _table = new TableContainer<TestIntModel>("tablename");
			var _model1 = new TestIntModel();
			_model1.Data.AsInt = 1;
			_table.Add(_model1);
			var _model2 = new TestIntModel();
			_model2.Data.AsInt = 2;
			_table.Add(_model2);
			var _model3 = new TestIntModel();
			_model3.Data.AsInt = 3;
			_table.Add(_model3);

			_table.Remove(_model2);

			var _checkRow = _table[1];

			Assert.AreEqual (_table.Count, 2);
			Assert.AreNotEqual(_model2, _checkRow);
			Assert.AreEqual(_model3, _checkRow);

		}

		[Test]
		public void WhereTest(){
			var _testValue = 123;
			var _table = new TableContainer<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.Add(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.Add(_model2);

			var _result = _table.Where(_row => _row.Data.AsInt == _testValue);

			foreach (var _item in _result) {
				Assert.AreEqual(_item, _model2);
			}
		}


		[Test]
		public void FirstOrDefaultTest(){
			var _testValue = 123;
			var _table = new TableContainer<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.Add(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.Add(_model2);

			var _result = _table.FirstOrDefault(_row => _row.Data.AsInt == _testValue);

			Assert.AreEqual(_result, _model2);
		}

		[Test]
		public void SelectTest(){
			var _testValue = 123;
			var _table = new TableContainer<TestStringModel>("tablename");

			var _model1 = new TestStringModel();
			_model1.Data.AsInt = _testValue + 100;
			_table.Add(_model1);

			var _model2 = new TestStringModel();
			_model2.Data.AsInt = _testValue;
			_table.Add(_model2);

			var _result = _table.Select(_row => _row);

			List<TestStringModel> _list = new List<TestStringModel>(_result);

			Assert.AreEqual(_list[0], _model1);
			Assert.AreEqual(_list[1], _model2);
			Assert.AreEqual(_list.Count, 2);
		}

		[Test]
		public void SetStorageTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _storage = Substitute.For<ITableStorage>();
			Action<Result> _callback = (Result _result) => {
			};


			_table.SetStorage(_storage);
			_table.Save(_callback);

			_storage.ReceivedWithAnyArgs().Save<RecordContainer>(_table, _callback);
			Assert.Pass();
		}

		[Test]
		public void SaveTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _storage = Substitute.For<ITableStorage>();
			Action<Result> _callback = (Result _result) => {
			};


			_table.SetStorage(_storage);
			_table.Save(_callback);

			_storage.ReceivedWithAnyArgs().Save<RecordContainer>(_table, _callback);
			Assert.Pass();
		}

		[Test]
		public void LoadTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			var _storage = Substitute.For<ITableStorage>();
			Action<Result> _callback = (Result _result) => {
			};


			_table.SetStorage(_storage);
			_table.Load(_callback);

			_storage.ReceivedWithAnyArgs().Load<RecordContainer>(_table, _callback);
			Assert.Pass();
		}

		[Test]
		public void AutoIncreaseTest(){
			var _table = new TableContainer<RecordContainer>("tablename");
			
			var _index1 = _table.AutoIncreaseNumber;

			Assert.AreEqual(_index1, 1);


			var _index2 = _table.AutoIncreaseNumber;

			Assert.AreEqual(_index2, 2);
		}

        class TestStorage : ITableStorage
        {
            public string StorageType{ get{ return "teststorage";}}

            public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new()
            {
				if(_callback != null){
					_callback(new Result(0, string.Empty, 0));
				} 
            }

            public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new()
            {
                throw new NotImplementedException();
            }

            public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
            {
                throw new NotImplementedException();
            }

            public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new()
            {
                if(_callback != null){
					_callback(new Result(0, string.Empty, 0));
				} 
            }
        }

		class TestRecord : RecordContainer{

		}

        [Test]
		public void QueryUpdateTest1(){
			var _storage = new TestStorage();
			var _table = new TableContainer<TestRecord>("QueryTest1");
			_table.SetStorage(_storage);
			_table.Load();
			
			var _row1 = new TestRecord();
			_row1.Add("no", new IntVariable(1));
			_row1.Add("name", new StringVariable("jone"));
			_row1.Add("money", new IntVariable(1000));
			_table.Add(_row1);

			var _row2 = new TestRecord();
			_row2.Add("no", new IntVariable(2));
			_row2.Add("name", new StringVariable("smith"));
			_row2.Add("money", new IntVariable(2000));
			_table.Add(_row2);

            TableService.Query("SETUP QueryId=ABC, TargetUser="+TableService.UserId+", Expired=99121223, Repeat=99999;UPDATE QueryTest1 SET money+=2100, name=\"change\" WHERE no=1");

			Assert.AreEqual(_table[0]["no"].AsVariable.AsInt, 1);
			Assert.AreEqual(_table[0]["name"].AsVariable.AsString, "change");
			Assert.AreEqual(_table[0]["money"].AsVariable.AsInt, 3100);

			Assert.AreEqual(_table[1]["no"].AsVariable.AsInt, 2);
			Assert.AreEqual(_table[1]["name"].AsVariable.AsString, "smith");
			Assert.AreEqual(_table[1]["money"].AsVariable.AsInt, 2000);

		}
		
		[Test]
		public void QueryUpdateTest2(){
			var _storage = new TestStorage();
			var _table = new TableContainer<TestRecord>("QueryTest2");
			_table.SetStorage(_storage);
			_table.Load();

			var _row1 = new TestRecord();
			_row1.Add("no", new IntVariable(1));
			_row1.Add("name", new StringVariable("jone"));
			_row1.Add("money", new IntVariable(1000));
			_table.Add(_row1);

			var _row2 = new TestRecord();
			_row2.Add("no", new IntVariable(2));
			_row2.Add("name", new StringVariable("smith"));
			_row2.Add("money", new IntVariable(2000));
			_table.Add(_row2);

			
			TableService.Query("SETUP QueryId=DEF, TargetUser="+TableService.UserId+", Expired=99121223, Repeat=99999;UPDATE QueryTest2 SET money*=2, name=\"change1\" WHERE no=2");
			

			Assert.AreEqual(_table[0]["no"].AsVariable.AsInt, 1);
			Assert.AreEqual(_table[0]["name"].AsVariable.AsString, "jone");
			Assert.AreEqual(_table[0]["money"].AsVariable.AsInt, 1000);

			Assert.AreEqual(_table[1]["no"].AsVariable.AsInt, 2);
			Assert.AreEqual(_table[1]["name"].AsVariable.AsString, "change1");
			Assert.AreEqual(_table[1]["money"].AsVariable.AsInt, 4000);

		}

		[Test]
		public void QueryExpiredCheck1(){
			bool isException = false;
			try{
				TableService.Query("SETUP QueryId=EIAKFH, TargetUser="+TableService.UserId+", Expired=18121223, Repeat=99999;");
			}catch(QueryException _e){
				Assert.AreEqual(_e.ExceptionType, QueryExceptionType.Expired);
				isException = true;
			}

			Assert.AreEqual(isException, true);

		}

		[Test]
		public void QueryExpiredCheck2(){
			bool isException = false;
			try{
				TableService.Query("SETUP QueryId=EIAKFH, TargetUser="+TableService.UserId+", Expired=99121223, Repeat=99999;");
			}catch{
				isException = true;
			}

			Assert.AreEqual(isException, false);

		}

		[Test]
		public void QueryTargetUserCheck1(){
			bool isException = false;
			try{
				TableService.Query("SETUP QueryId=EIAKFH, TargetUser=1234, Expired=99121223, Repeat=99999;");
			}catch(QueryException _e){
				Assert.AreEqual(_e.ExceptionType, QueryExceptionType.NotTargetUser);
				isException = true;
			}

			Assert.AreEqual(isException, true);

		}
	}
}
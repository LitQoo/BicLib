using System;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;
using NUnit.Framework;
using NSubstitute;

namespace BicDB.Storage
{
	public class FileStorageTest {
		[Test]
		public void GetInstanceTest(){
			var _storage = FileStorage.GetInstance();

			Assert.IsNotNull(_storage);
		}

		[Test]
		public void SaveTest1(){
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = new Table<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			var _model = new TestStringModel();
			_model.Data.AsString = "value";
			_table.AddRow(_model);

			_storage.Save<TestStringModel>(_table, (bool _isSuccess)=>{
				
			});

			_fileController.Received().Write("{\"version\":0,\"data\":[{\"data\":\"value\"}]}", FileStorage.FILE_NAME_PREFIX + _tableName);
		}


		[Test]
		public void SaveTest2(){
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = new Table<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			var _model = new TestStringModel();
			_model.Data.AsString = "va\"lu\te";
			_table.AddRow(_model);

			_storage.Save<TestStringModel>(_table, (bool _isSuccess)=>{

			});

			_fileController.Received().Write("{\"version\":0,\"data\":[{\"data\":\"va\\\"lu\te\"}]}", FileStorage.FILE_NAME_PREFIX + _tableName);
		}

		[Test]
		public void LoadTest1(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();
			_fileController.Read(Arg.Any<string>()).Returns("{\"version\":0,\"data\":[{\"data\":\"value\"}]}");

			//do
			_table.Load();

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "value");

		}

		[Test]
		public void LoadTest2(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();
			_fileController.Read(Arg.Any<string>()).Returns("{\"version\":0,\"data\":[{\"data\":\"va\\\"lu \\\t\te\"}]}");

			//do
			_table.Load();

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "va\"lu \t\te");

		}

	}


}

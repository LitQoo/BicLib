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
		public void SaveTest(){
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = new Table<TestModel>(_tableName);
			_table.SetStorage(_storage);
			var _model = new TestModel();
			_model.Data.AsString = "value";
			_table.AddRow(_model);

			_storage.Save<TestModel>(_table, (bool _isSuccess)=>{
				
			});

			_fileController.Received().Write("{\"version\":0,\"data\":[{\"data\":\"value\"}]}", FileStorage.FILE_NAME_PREFIX + _tableName);
		}

		[Test]
		public void LoadTest(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();
			var _fileController = Substitute.For<IFileController>();
			_storage.SetFileController(_fileController);

			var _table = Manager.CreateTable<TestModel>(_tableName, "");
			Action<bool> _callback = (bool _isSuccess)=>{

			};
			_table.SetStorage(_storage);
			_fileController.Read(Arg.Any<string>()).Returns("{\"version\":0,\"data\":[{\"data\":\"value\"}]}");

			//do
			_table.Load(_callback);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "value");

		}


	}


}

using System;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;
using NUnit.Framework;
using NSubstitute;

namespace BicDB.Storage
{
	public class JsonConvertorTest {
		[Test]
		public void ConvertTableToJsonString1(){
			var _tableName = "tablename";
			var _table = new Table<TestStringModel>(_tableName);
			var _model = new TestStringModel();
			_model.Data.AsString = "value";
			_table.SetHeader ("version", 0);
			_table.AddRow(_model);

			string _result = JsonConvertor.ConvertTableToJsonString (_table);

			Assert.AreEqual (_result, "{\"version\":0,\"data\":[{\"data\":\"value\"}]}");
		}

		[Test]
		public void ConvertTableToJsonString2(){
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = new Table<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			var _model = new TestStringModel();
			_model.Data.AsString = "va\"lu\te";
			_table.SetHeader ("version", 0);
			_table.AddRow(_model);

			string _result = JsonConvertor.ConvertTableToJsonString (_table);

			Assert.AreEqual (_result, "{\"version\":0,\"data\":[{\"data\":\"va\\\"lu\te\"}]}");
		}

		[Test]
		public void ConvertJsonDictionaryToTable1(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();
		
			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":123,\"data\":[{\"data\":\"value\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "value");
			Assert.AreEqual(_table.GetHeader("version").AsString, "123");
		}

		[Test]
		public void ConvertJsonDictionaryToTable2(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":0,\"data\":[{\"data\":\"va\\\"lu \\\t\te\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "va\"lu \t\te");

		}

		[Test]
		public void ConvertJsonDictionaryToTable3(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":0,\"data\":[{\"data\":{\"key\":\"value\"}}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "{\"key\":\"value\"}");

		}

		[Test]
		public void ConvertJsonDictionaryToTable4(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":0,\"data\":[{\"data\":[1,2,3]}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "[1,2,3]");

		}

		[Test]
		public void ConvertJsonListToTable1(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestWebModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			string _jsonList = "[{\"androidLink\":\"https://play.google.com/store/apps/details?id=com.kaimangames.basketball.nba.kim.jordan\",\"assetbundle\":\"http://aksdjf?dkf\",\"iosLink\":\"none\",\"title\":\"농구왕 김득점\",\"viewWeight\":100},{\"title\":\"ab cde\tfg\"}]";
			JsonConvertor.ConvertJsonListToTable (ref _jsonList, ref _table);

			//check
			Assert.AreEqual(_table[0].Title.AsString, "ab cde\tfg");
		}

	}
}
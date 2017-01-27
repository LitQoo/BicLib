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
			_table.AddRow(_model);

			string _result = JsonConvertor.ConvertTableToJsonString (_table);

			Assert.AreEqual (_result, "{\"version\":0,\"data\":[{\"data\":\"va\\\"lu\te\"}]}");
		}

		[Test]
		public void ConvertJsonStringToTable1(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();
		
			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonStringToTable ("{\"version\":0,\"data\":[{\"data\":\"value\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "value");
		}

		[Test]
		public void ConvertJsonStringToTable2(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestStringModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			JsonConvertor.ConvertJsonStringToTable ("{\"version\":0,\"data\":[{\"data\":\"va\\\"lu \\\t\te\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["data"].AsString, "va\"lu \t\te");

		}

	}
}
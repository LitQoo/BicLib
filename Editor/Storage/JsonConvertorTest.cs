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
			Assert.AreEqual(_table[0]["data"].AsString, "va\\\"lu \\\t\te");
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

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":0,\"data\":[{\"data\":1} , \n\t{\"data\":2}]}", _table);

			//check
			Assert.AreEqual(_table.GetSize(), 2);
			Assert.AreEqual(_table[0]["data"].AsInt, 1);
			Assert.AreEqual(_table[1]["data"].AsInt, 2);

		}

		[Test]
		public void ConvertJsonDictionaryToTable5(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestWebModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			string _jsonList = "{\"data\":[{\"images\":[\"url1\",\"url2\",\"url3\",\"url4\"],\"title\":\"농구왕 김득점\",\"viewWeight\":100},{\"title\":\"중년기사 김봉식\",\"viewWeight\":200}],\"version\":1}";
			JsonConvertor.ConvertJsonDictionaryToTable (_jsonList, _table);

			string _result = JsonConvertor.ConvertTableToJsonString(_table);

			//check
			Assert.AreEqual(_result, "{\"version\":\"1\",\"data\":[{\"androidLink\":\"\",\"assetbundle\":\"\",\"iosLink\":\"\",\"title\":\"농구왕 김득점\",\"viewWeight\":100,\"images\":[\"url1\", \"url2\", \"url3\", \"url4\"]},{\"androidLink\":\"\",\"assetbundle\":\"\",\"iosLink\":\"\",\"title\":\"중년기사 김봉식\",\"viewWeight\":200,\"images\":[]}]}");

		}

		[Test]
		public void ConvertJsonDictionaryToTableThenUpdate1(){
			//set
			var _tableName = "synctesttable";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestSyncModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();
			_table.PrimaryKey = "key";

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":123,\"data\":[{\"key\":1,\"title\":\"value1\"}, {\"key\":2,\"title\":\"value2\"}]}", _table);

			Assert.AreEqual(_table[0][_table.PrimaryKey].AsString, "1");

			JsonConvertor.ConvertJsonDictionaryToTableThenUpdate ("{\"version\":123,\"data\":[{\"key\":2,\"title\":\"mod\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["title"].AsString, "value1");
			Assert.AreEqual(_table[1]["title"].AsString, "mod");
			Assert.AreEqual(_table.GetSize(), 2);
		}

		[Test]
		public void ConvertJsonDictionaryToTableThenUpdate2(){
			//set
			var _tableName = "synctesttable";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestSyncModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();
			_table.PrimaryKey = "key";

			JsonConvertor.ConvertJsonDictionaryToTable ("{\"version\":123,\"data\":[{\"key\":1,\"title\":\"value1\"}, {\"key\":2,\"title\":\"value2\"}]}", _table);

			//check
			Assert.AreEqual(_table[0][_table.PrimaryKey].AsInt, 1);
			Assert.AreEqual(_table[1][_table.PrimaryKey].AsInt, 2);

			JsonConvertor.ConvertJsonDictionaryToTableThenUpdate ("{\"version\":123,\"data\":[{\"key\":3,\"title\":\"mod\"}]}", _table);

			//check
			Assert.AreEqual(_table[0]["title"].AsString, "value1");
			Assert.AreEqual(_table[1]["title"].AsString, "value2");
			Assert.AreEqual(_table[2]["title"].AsString, "mod");
			Assert.AreEqual(_table.GetSize(), 3);
		}

		[Test]
		public void ConvertJsonListToTable1(){
			//set
			var _tableName = "tablename";
			var _storage = new FileStorage();

			var _table = Manager.GetOrCreateTable<TestWebModel>(_tableName);
			_table.SetStorage(_storage);
			_table.Clear ();

			string _jsonList = "{\"data\":[{\"images\":[\"url1\",\"url2\",\"url3\",\"url4\"],\"title\":\"농구왕 김득점\",\"viewWeight\":100},{\"title\":\"중년기사 김봉식\",\"viewWeight\":200}],\"version\":1}";
			JsonConvertor.ConvertJsonDictionaryToTable (_jsonList, _table);

			//check
			Assert.AreEqual(_table.GetSize(), 2);
			Assert.AreEqual(_table[0].Title.AsString, "농구왕 김득점");
			Assert.AreEqual(_table[1].Title.AsString, "중년기사 김봉식");
			Assert.AreEqual(_table[0].Images[0].AsString, "url1");
			Assert.AreEqual(_table[0].Images[1].AsString, "url2");
			Assert.AreEqual(_table[0].Images.GetSize(), 4);
		}

		[Test]
		public void ConvertJsonListToList1(){
			
			List<IVariable> _list = JsonConvertor.ConvertJsonListToList<StringVariable>("[\"abc\",\"d\\\"\t\",\"e\",\"f\"]");

			Assert.AreEqual(_list[0].AsString, "abc");
			Assert.AreEqual(_list[1].AsString, "d\\\"\t");
			Assert.AreEqual(_list[2].AsString, "e");
			Assert.AreEqual(_list[3].AsString, "f");
		}

		[Test]
		public void ConvertJsonListToList2(){
			
			List<IVariable> _list = JsonConvertor.ConvertJsonListToList<StringVariable>("[\n\t\t\t\t{ \"test\" : \n\"value\" }, \n\t\t\t\t{ \"test\" : \"value\" }, \n\t\t\t\t{ \"test\" : \"value\" }\n\t\t\t]");

			Assert.AreEqual(_list[0].AsString, "{ \"test\" : \n\"value\" }");
		}

		[Test]
		public void ConvertJsonListToList3(){

			List<IVariable> _list = JsonConvertor.ConvertJsonListToList<IntVariable>("[\n123, \n456, \n789, 0,1]");

			Assert.AreEqual(_list[0].AsInt, 123);
			Assert.AreEqual(_list[1].AsInt, 456);
			Assert.AreEqual(_list[2].AsInt, 789);
			Assert.AreEqual(_list[3].AsInt, 0);
			Assert.AreEqual(_list[4].AsInt, 1);
		}

		[Test]
		public void ConvertJsonListToList4(){

			List<IVariable> _list = JsonConvertor.ConvertJsonListToList<EncryptedIntVariable>("[\n123, \n456, \n789, 0,1]");

			Assert.AreEqual(_list[0].AsInt, 123);
			Assert.AreEqual(_list[1].AsInt, 456);
			Assert.AreEqual(_list[2].AsInt, 789);
			Assert.AreEqual(_list[3].AsInt, 0);
			Assert.AreEqual(_list[4].AsInt, 1);
		}

		[Test]
		public void ConvertJsonListToList5(){

			List<IVariable> _list = JsonConvertor.ConvertJsonListToList<EncryptedIntVariable>("[]");

			Assert.AreEqual (_list.Count, 0);
		}

		[Test]
		public void ConvertListToJsonString1(){
			
			List<IVariable> _list = new List<IVariable>();

			_list.Add(new IntVariable(123));
			_list.Add(new IntVariable(456));
			_list.Add(new IntVariable(789));

			string _result = JsonConvertor.ConvertListToJsonString(_list);

			Assert.AreEqual(_result, "[123, 456, 789]");
		}

		[Test]
		public void ConvertListToJsonString2(){

			List<IVariable> _list = new List<IVariable>();

			string _result = JsonConvertor.ConvertListToJsonString(_list);

			Assert.AreEqual(_result, "[]");
		}
	}
}
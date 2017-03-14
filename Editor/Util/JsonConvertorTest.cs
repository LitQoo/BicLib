using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB.Container;
using BicDB;
using System;
using System.IO;

namespace BicDB.Utility
{
	public class JsonConvertorTest
	{
		[Test]
		public void JsonToString1(){
			StringVariable _number = new StringVariable();
			string _json = "\"bb\\\"\t\nteset\"";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildStringVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsString, "bb\"\t\nteset");
		}

		[Test]
		public void JsonToNumber1(){
			IntVariable _number = new IntVariable();
			string _json = "23948";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsInt, 23948);
		}

		[Test]
		public void JsonToNumber2(){
			IntVariable _number = new IntVariable();
			string _json = "-23948";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsInt, -23948);
		}

		[Test]
		public void JsonToNumber3(){
			FloatVariable _number = new FloatVariable();
			string _json = "23948.3323";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsFloat, 23948.3323f);
		}

		[Test]
		public void JsonToNumber4(){
			FloatVariable _number = new FloatVariable();
			string _json = "-23948.3323";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsFloat, -23948.3323f);
		}

		[Test]
		public void JsonToNumber5(){
			BoolVariable _number = new BoolVariable();
			string _json = "true";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsBool, true);
		}

		[Test]
		public void JsonToNumber6(){
			BoolVariable _number = new BoolVariable();
			string _json = "false";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsBool, false);
		}
		[Test]
		public void JsonToNumber7(){
			BoolVariable _number = new BoolVariable();
			string _json = "TrUe";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsBool, true);
		}

		[Test]
		public void JsonToNumber8(){
			BoolVariable _number = new BoolVariable();
			string _json = "fALSE";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsBool, false);
		}

		[Test]
		public void JsonToNumber9(){
			EnumVariable<TestEnum> _number = new EnumVariable<TestEnum>();
			string _json = "two";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsEnum, TestEnum.two);
		}

		[Test]
		public void JsonToNumber10(){
			EnumVariable<TestEnum> _number = new EnumVariable<TestEnum>();
			string _json = "two";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildNumberVariable(_number, ref _json, ref _counter);

			Assert.AreEqual(_number.AsEnum, TestEnum.two);
		}

		[Test]
		public void JsonToList1(){
			IListContainer<IntVariable> _list = new ListContainer<IntVariable>();
			string _json = "[1,2,3,4]";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildListVariable(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
		}

		[Test]
		public void JsonToList2(){
			IListContainer<IntVariable> _list = new ListContainer<IntVariable>();
			string _json = "[ 1333.1, 22242 ,\t3,\n\n -4 \n\t]";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildListVariable(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
			Assert.AreEqual(_list[0].Type, DataType.Int);
		}

		[Test]
		public void JsonToList3(){
			IListContainer<StringVariable> _list = new ListContainer<StringVariable>();
			string _json = "  [\n\t\t    \" sdkf\"\t\n ,  \n\t\" \t\\\" \",\"3\",\"4\"]";
			int _counter = 0;


			Console.WriteLine("test");
			JsonConvertor.GetInstance().BuildListVariable(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
			Assert.AreEqual(_list[0].Type, DataType.String);
			Assert.AreEqual(_list[0].AsString, " sdkf");
			Assert.AreEqual(_list[1].AsString, " \t\" ");
			Assert.AreEqual(_list[2].AsString, "3");
			Assert.AreEqual(_list[3].AsString, "4");
		}

		[Test]
		public void JsonToDictionary1(){
			IDictionaryContainer<FloatVariable> _dictionary = new DictionaryContainer<FloatVariable>();
			string _json = "{\t  \"key\"  : -122.3\t,\"key2\":1111}\t";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildDictionaryVariable(_dictionary, ref _json, ref _counter);

			Assert.AreEqual(_dictionary["key"].AsFloat, -122.3f);
			Assert.AreEqual(_dictionary["key2"].AsFloat, 1111f);
		}

		[Test]
		public void JsonToModel1(){
			TestClass _model = new TestClass();
			string _json = "{\t\"key1\":123,\"key2\":\"aaa\",\"key4\":-23.2,\"key3\":{\"key1\":12}}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildModelVariable(_model, ref _json, ref _counter);

			Assert.AreEqual(_model.member1.AsInt, 123);
			Assert.AreEqual(_model.member2.AsString, "aaa");
			Assert.AreEqual((_model["key4"] as IVariable).AsString, "-23.2");
			Assert.AreEqual(_model.member3.member1.AsInt, 12);
		}

		[Test]
		public void JsonToTable1(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test");
			string _json = "{ \"data\" : [{\"key1\":123, \"key2\" :\"string\", \"key3\":{\"key1\":119, \"key2\":\"test\"}} ,{\"key1\":2, \"key2\" :\"string\", \"key3\":{\"key1\":222}}], \"name\" : \"test\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildTableVariable(_table, ref _json, ref _counter);

			Assert.AreEqual(_table[0].member1.AsInt, 123);
			Assert.AreEqual(_table[1].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "test");
			Assert.AreEqual(_table[0].member3.member1.AsInt, 119);
			Assert.AreEqual((_table[0].member3["key2"] as IVariable).AsString, "test");
		}	

		[Test]
		public void TableToJson1(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test");

			{
				var _row = new TestClass();
				_row.member1.AsInt = 1;
				_row.member2.AsString = "two";
				_row.member3.member1.AsInt = 2;

				_table.Add(_row);
			}

			{
				var _row = new TestClass();
				_row.member1.AsInt = 12;
				_row.member2.AsString = "two2";
				_row.member3.member1.AsInt = 22;

				_table.Add(_row);
			}

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_table, ref _json);

			Assert.AreEqual("{\"data\":[{\"key1\":1,\"key2\":\"two\",\"key3\":{\"key1\":2}},{\"key1\":12,\"key2\":\"two2\",\"key3\":{\"key1\":22}}]}", _json);
		}

		[Test]
		public void DictionaryToJson(){
			DictionaryContainer<StringVariable> _dict = new DictionaryContainer<StringVariable>();
			_dict.Add("test", new StringVariable("test"));
			_dict.Add("key1", new StringVariable("test1"));

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_dict, ref _json);

			Assert.AreEqual("{\"test\":\"test\",\"key1\":\"test1\"}", _json);
		}

		[Test]
		public void ListToJson(){
			ListContainer<StringVariable> _dict = new ListContainer<StringVariable>();
			_dict.Add(new StringVariable("test0"));
			_dict.Add(new StringVariable("test1"));

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_dict, ref _json);

			Assert.AreEqual("[\"test0\",\"test1\"]", _json);
		}

		[Test]
		public void ModelToJson(){
			TestClass _model = new TestClass();
			_model.member1.AsInt = 999;
			_model.member2.AsString = "test";
			_model.member3.member1.AsInt = 888;

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_model, ref _json);

			Assert.AreEqual("{\"key1\":999,\"key2\":\"test\",\"key3\":{\"key1\":888}}", _json);
		}


		[Test]
		public void StringToJson(){
			StringVariable _value = new StringVariable();
			_value.AsString = "good";

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_value, ref _json);

			Assert.AreEqual("\"good\"", _json);
		}

		[Test]
		public void NumberToJson(){
			EnumVariable<TestEnum> _value = new EnumVariable<TestEnum>();
			_value.AsEnum = TestEnum.three;

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_value, ref _json);

			Assert.AreEqual("three", _json);
		}



		class TestClass : ModelContainer{
			public IntVariable member1 = new IntVariable(0);
			public StringVariable member2 = new StringVariable("");
			public TestClass2 member3 = new TestClass2();

			public TestClass(){
				AddManagedColumn("key1", member1);
				AddManagedColumn("key2", member2);
				AddManagedColumn("key3", member3);
			}
		}

		class TestClass2 : ModelContainer{
			public IntVariable member1 = new IntVariable(0);

			public TestClass2(){
				AddManagedColumn("key1", member1);
			}
		}

		enum TestEnum 
		{
			one,
			two,
			three,
			four
		}
	}
}
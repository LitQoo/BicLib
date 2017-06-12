using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB.Variable;
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
			IListContainer<IVariable> _list = new ListContainer<IVariable>();
			string _json = "[1,2,3,4]";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildListContainer(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
		}

		[Test]
		public void JsonToList2(){
			IListContainer<IVariable> _list = new ListContainer<IVariable>();
			string _json = "[ 1333.1, 22242 ,\t3,\n\n -4 \n\t]";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildListContainer(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
			Assert.AreEqual(_list[0].Type, DataType.Float);
		}

		[Test]
		public void JsonToList3(){
			IListContainer<IVariable> _list = new ListContainer<IVariable>();
			string _json = "  [\n\t\t    \" sdkf\"\t\n ,  \n\t\" \t\\\" \",\"3\",\"4\"]";
			int _counter = 0;


			Console.WriteLine("test");
			JsonConvertor.GetInstance().BuildListContainer(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
			Assert.AreEqual(_list[0].Type, DataType.String);
			Assert.AreEqual(_list[0].AsVariable.AsString, " sdkf");
			Assert.AreEqual(_list[1].AsVariable.AsString, " \t\" ");
			Assert.AreEqual(_list[2].AsVariable.AsString, "3");
			Assert.AreEqual(_list[3].AsVariable.AsString, "4");
		}

		[Test]
		public void JsonToDictionary1(){
			IDictionaryContainer _dictionary = new DictionaryContainer();
			string _json = "{\t  \"key\"  : -122.3\t,\"key2\":1111}\t";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildDictionaryContainer(_dictionary, ref _json, ref _counter);

			Assert.AreEqual((_dictionary["key"] as IVariable).AsFloat, -122.3f);
			Assert.AreEqual((_dictionary["key2"] as IVariable).AsFloat, 1111f);
		}

		[Test]
		public void JsonToModel1(){
			TestClass _model = new TestClass();
			string _json = "{\t\"key1\":123,\"key2\":\"aaa\",\"key4\":-23.2,\"key3\":{\"key1\":12}}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json, ref _counter);

			Assert.AreEqual(_model.member1.AsInt, 123);
			Assert.AreEqual(_model.member2.AsString, "aaa");
			Assert.AreEqual((_model["key4"] as IVariable).AsString, "-23.2");
			Assert.AreEqual(_model.member3.member1.AsInt, 12);
		}


		[Test]
		public void JsonToModel2(){
			TestClass _model = new TestClass();
			string _json = "{\t\"key3\":{\"key1\":12, \"key2\":\"vv\"},\"key1\":123,\"key2\":\"aaa\",\"key4\":-23.2}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json, ref _counter);

			Assert.AreEqual(_model.member1.AsInt, 123);
			Assert.AreEqual(_model.member2.AsString, "aaa");
			Assert.AreEqual((_model["key4"] as IVariable).AsString, "-23.2");
			Assert.AreEqual(_model.member3.member1.AsInt, 12);
		}



		[Test]
		public void JsonToModel3(){
			RecordContainer _model = new RecordContainer();
			string _json1 = "{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":10,\"name\":\"mike\"}";
			int _counter1 = 0;
			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json1, ref _counter1);


			string _json2 = "{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"}";
			int _counter2 = 0;
			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json2, ref _counter2);

			Assert.AreEqual(_model.GetValue<IVariable>("age").AsInt, 13);
		}

		[Test]
		public void JsonToTable1(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test");
			string _json = "{\"data\":[{\"key1\":123,\"key2\":\"string\",\"key3\":{\"key1\":119,\"key2\":\"test\"}},{\"key1\":2,\"key2\":\"string\",\"key3\":{\"key1\":222}}],\"name\":\"test\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);

			Assert.AreEqual(_table[0].member1.AsInt, 123);
			Assert.AreEqual(_table[1].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "test");
			Assert.AreEqual(_table[0].member3.member1.AsInt, 119);
			Assert.AreEqual((_table[0].member3["key2"] as IVariable).AsString, "test");
		}	

		[Test]
		public void JsonToTable2(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test");
			_table.PrimaryKey = "key1";
			string _json1 = "{ \"data\" : [{\"key1\":123, \"key2\" :\"string\", \"key3\":{\"key1\":119, \"key2\":\"test\"}} ,{\"key1\":2, \"key2\" :\"string\", \"key3\":{\"key1\":222}}], \"name\" : \"test\"}";
			int _counter1 = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json1, ref _counter1);

			Assert.AreEqual(_table[0].member2.AsString, "string");

			string _json2 = "{ \"data\" : [{\"key1\":123, \"key2\" :\"modify\", \"key3\":{\"key1\":119, \"key2\":\"test\"}} ,{\"key1\":444, \"key2\" :\"add~\", \"key3\":{\"key1\":222}}], \"name\" : \"test\"}";
			int _counter2 = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json2, ref _counter2);

			Assert.AreEqual(_table[0].member1.AsInt, 123);
			Assert.AreEqual(_table[1].member1.AsInt, 2);

			Assert.AreEqual(_table[0].member2.AsString, "modify");
			Assert.AreEqual(_table[1].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "test");
			Assert.AreEqual(_table[0].member3.member1.AsInt, 119);
			Assert.AreEqual((_table[0].member3["key2"] as IVariable).AsString, "test");
			Assert.AreEqual(_table[2].member1.AsInt, 444);
		}	

		[Test]
		public void JsonToTable3(){
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer>("test");
			string _json = "{\"data\":[{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"},{\"adress\":{\"city\":\"seoul\",\"country\":\"korea\"},\"age\":14,\"name\":\"js\"}], \"name\" : \"test\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);

			Assert.AreEqual(_table.Count, 2);
			Assert.AreEqual(_table[0].GetValue<IVariable>("age").AsInt, 13);
			Assert.AreEqual(_table[1].GetValue<IVariable>("age").AsInt, 14);
			Assert.AreEqual(_table[0].GetValue<IVariable>("name").AsString, "mike");
			Assert.AreEqual(_table[1].GetValue<IVariable>("name").AsString, "js");
			Assert.AreEqual(_table[1]["adress"].As<IDictionaryContainer>()["city"].AsVariable.AsString, "seoul");
			Assert.AreEqual(_table[0]["adress"].As<IDictionaryContainer>()["city"].AsVariable.AsString, "gogo");
			Assert.AreEqual(_table.Property["name"].AsVariable.AsString, "test");
		}	

		[Test]
		public void JsonToTable4(){
			ITableContainer<TestClass3> _table = new TableContainer<TestClass3>("test4");
			_table.PrimaryKey = "key1";
			string _json1 = "{ \"data\" : [{\"key4\":[4,3,2,1],\"key1\":123, \"key2\" :\"string\", \"key3\":{\"key1\":119, \"key2\":\"test\"}} ,{\"key4\":[5,6,7,8],\"key1\":2, \"key2\" :\"string\", \"key3\":{\"key1\":222}}], \"name\" : \"test\"}";
			int _counter1 = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json1, ref _counter1);

			Assert.AreEqual(_table[0].member1.AsInt, 123);
			Assert.AreEqual(_table[1].member1.AsInt, 2);

			Assert.AreEqual(_table[0].member2.AsString, "string");
			Assert.AreEqual(_table[1].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "test");
			Assert.AreEqual(_table[0].member3.member1.AsInt, 119);
			Assert.AreEqual((_table[0].member3["key2"] as IVariable).AsString, "test");

			Assert.AreEqual (_table [0].member4.Count, 4);
			Assert.AreEqual (_table [0].member4[0].AsFloat, 4);
			Assert.AreEqual (_table [0].member4[1].AsFloat, 3);
			Assert.AreEqual (_table [0].member4[2].AsFloat, 2);
			Assert.AreEqual (_table [0].member4[3].AsFloat, 1);
		}	

		[Test]
		public void BuildVariableTest(){
			string _json = "{\"head\":\"value\",\"list\":[100,200,300], \"data\":[{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"},{\"adress\":{\"city\":\"seoul\",\"country\":\"korea\"},\"age\":14,\"name\":\"js\"}], \"name\" : \"test\"}";
			int _counter = 0;

			var _value = JsonConvertor.GetInstance().BuildVariable(ref _json, ref _counter) as DictionaryContainer;

			Assert.AreEqual(_value.Type, DataType.Dictionary);
			Assert.IsTrue(_value.ContainsKey("head"));
			Assert.AreEqual(_value["head"].AsVariable.AsString, "value");
			Assert.IsTrue(_value.ContainsKey("list"));
			Assert.AreEqual(_value["list"].Type, DataType.List);
			Assert.AreEqual(_value["list"].As<IListContainer<IDataBase>>()[1].AsVariable.AsString, "200");
			Assert.IsTrue(_value.ContainsKey("data"));
			Assert.AreEqual(_value["name"].AsVariable.AsString, "test");

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

			_table.Property["test"] = new StringVariable("testvalue");

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_table, ref _json, null);

			Assert.AreEqual("{\"test\":\"testvalue\",\"data\":[{\"key1\":1,\"key2\":\"two\",\"key3\":{\"key1\":2}},{\"key1\":12,\"key2\":\"two2\",\"key3\":{\"key1\":22}}]}", _json);
		}

		[Test]
		public void JsonToDataStore1(){
			IDataStoreContainer<TestClass> _table = new DataStoreContainer<TestClass>("test");
			string _json = "{\"data\":{\"sklfslkdfsdf\":{\"key1\":123,\"key2\":\"string\",\"key3\":{\"key1\":119,\"key2\":\"test\"}},\"aaaasedf\":{\"key1\":2,\"key2\":\"string\",\"key3\":{\"key1\":222}}},\"name\":\"test\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildDataStoreContainer(_table, ref _json, ref _counter);

			Assert.AreEqual(_table["sklfslkdfsdf"].member1.AsInt, 123);
			Assert.AreEqual(_table["aaaasedf"].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "test");
			Assert.AreEqual(_table["sklfslkdfsdf"].member3.member1.AsInt, 119);
			Assert.AreEqual((_table["sklfslkdfsdf"].member3["key2"] as IVariable).AsString, "test");
		}	

		[Test]
		public void JsonToDataStore2(){
			IDataStoreContainer<TestClass> _table = new DataStoreContainer<TestClass>("test");
			string _json1 = "{\"data\":{\"2\":{\"age\":1,\"name\":\"bb\"},\"sakdjfefdf\":{\"age\":99,\"name\":\"jacka\"}}}";
			int _counter1 = 0;

			JsonConvertor.GetInstance().BuildDataStoreContainer(_table, ref _json1, ref _counter1);

			string _json2 = "{\"data\":{\"2\":{\"age\":100,\"name\":\"js\"},\"sakdjfefdf\":{\"age\":77,\"name\":\"jack\"}}}";
			int _counter2 = 0;

			JsonConvertor.GetInstance().BuildDataStoreContainer(_table, ref _json2, ref _counter2);

			Assert.AreEqual(_table["2"].GetValue<IVariable>("age").AsInt, 100);
			Assert.AreEqual(_table["2"].GetValue<IVariable>("name").AsString, "js");

			Assert.AreEqual(_table["sakdjfefdf"].GetValue<IVariable>("age").AsInt, 77);
			Assert.AreEqual(_table["sakdjfefdf"].GetValue<IVariable>("name").AsString, "jack");
		}	


		[Test]
		public void DictionaryToJson(){
			DictionaryContainer _dict = new DictionaryContainer();
			_dict.Add("test", new StringVariable("test"));
			_dict.Add("key1", new StringVariable("test1"));

			string _json = string.Empty;
			JsonConvertor.GetInstance().BuildFormattedString(_dict, ref _json);

			Assert.AreEqual("{\"test\":\"test\",\"key1\":\"test1\"}", _json);
		}

		[Test]
		public void ListToJson(){
			ListContainer<IVariable> _dict = new ListContainer<IVariable>();
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



		class TestClass : RecordContainer{
			public IntVariable member1 = new IntVariable(0);
			public StringVariable member2 = new StringVariable("");
			public TestClass2 member3 = new TestClass2();

			public TestClass(){
				AddManagedColumn("key1", member1);
				AddManagedColumn("key2", member2);
				AddManagedColumn("key3", member3);
			}
		}

		class TestClass2 : RecordContainer{
			public IntVariable member1 = new IntVariable(0);

			public TestClass2(){
				AddManagedColumn("key1", member1);
			}
		}

		class TestClass3 : RecordContainer{
			public IntVariable member1 = new IntVariable(0);
			public StringVariable member2 = new StringVariable("");
			public TestClass2 member3 = new TestClass2();
			public ListContainer<IntVariable> member4 = new ListContainer<IntVariable>();

			public TestClass3(){
				AddManagedColumn("key1", member1);
				AddManagedColumn("key2", member2);
				AddManagedColumn("key3", member3);
				AddManagedColumn("key4", member4);
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
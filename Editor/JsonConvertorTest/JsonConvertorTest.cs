using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB.Variable;
using BicDB;
using System;
using System.IO;
using BicUtil.Json;

namespace BicUtil.Json
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

			JsonConvertor.GetInstance().BuildListContainer(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
		}

		[Test]
		public void JsonToList2(){
			IListContainer<FloatVariable> _list = new ListContainer<FloatVariable>();
			string _json = "[ 1333.1, 22242 ,\t3,\n\n -4 \n\t]";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildListContainer(_list, ref _json, ref _counter);

			Assert.AreEqual(_list.Count, 4);
			Assert.AreEqual(_list[0].Type, DataType.Float);
		}

		[Test]
		public void JsonToList3(){
			IListContainer<StringVariable> _list = new ListContainer<StringVariable>();
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
		public void JsonToMutableDictionary1(){
			IMutableDictionaryContainer _dictionary = new MutableDictionaryContainer();
			string _json = "{\t  \"key\"  : -122.3\t,\"key2\":1111}\t";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildMutableDictionaryContainer(_dictionary, ref _json, ref _counter);

			Assert.AreEqual((_dictionary["key"] as IVariable).AsFloat, -122.3f);
			Assert.AreEqual((_dictionary["key2"] as IVariable).AsFloat, 1111f);
		}

		[Test]
		public void JsonToModel1(){
			TestClass _model = new TestClass();
			string _json = "{\t\"key1\":123,\"key2\":\"aa\na\",\"key4\":-23.2,\"key3\":{\"key1\":12}}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json, ref _counter);

			Assert.AreEqual(_model.member1.AsInt, 123);
			Assert.AreEqual(_model.member2.AsString, "aa\na");
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
		public void JsonToModel4(){
			TestClass5 _model = new TestClass5();
			string _json1 = "{\"v\":\"12,123.13\"}";
			int _counter1 = 0;
			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json1, ref _counter1);

			Assert.AreEqual(_model.ToString(), "{\"v\":\"12,123.13\"}");
		}


		[Test]
		public void JsonToModel5(){
			TestClass5 _model = new TestClass5();
			string _json1 = "{\"v\":\"12,123.13\", \"qest\":[], \"t\":\"sts\"}";
			int _counter1 = 0;
			JsonConvertor.GetInstance().BuildModelContainer(_model, ref _json1, ref _counter1);

			Assert.AreEqual(_model.ToString(), "{\"v\":\"12,123.13\",\"qest\":[],\"t\":\"sts\"}");
		}

		[Test]
		public void JsonToTable1(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test");
			string _json = "{\"data\":[{\"key1\":123,\"key2\":\"string\",\"key3\":{\"key1\":119,\"key2\":\"test\"}},{\"key1\":2,\"key2\":\"string\",\"key3\":{\"key1\":222}}],\"name\":\"te\nst\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);

			Assert.AreEqual(_table[0].member1.AsInt, 123);
			Assert.AreEqual(_table[1].member1.AsInt, 2);
			Assert.AreEqual((_table.Property["name"] as IVariable).AsString, "te\nst");
			Assert.AreEqual(_table[0].member3.member1.AsInt, 119);
			Assert.AreEqual((_table[0].member3["key2"] as IVariable).AsString, "test");
		}	

		[Test]
		public void JsonToTable2(){
			ITableContainer<TestClass> _table = new TableContainer<TestClass>("test2");
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
			ITableContainer<RecordContainer> _table = new TableContainer<RecordContainer>("test3");
			string _json = "{\"data\":[{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"},{\"adress\":{\"city\":\"seoul\",\"country\":\"korea\"},\"age\":14,\"name\":\"js\"}], \"name\" : \"test\"}";
			int _counter = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);

			Assert.AreEqual(_table.Count, 2);
			Assert.AreEqual(_table[0].GetValue<IVariable>("age").AsInt, 13);
			Assert.AreEqual(_table[1].GetValue<IVariable>("age").AsInt, 14);
			Assert.AreEqual(_table[0].GetValue<IVariable>("name").AsString, "mike");
			Assert.AreEqual(_table[1].GetValue<IVariable>("name").AsString, "js");
			Assert.AreEqual(_table[1]["adress"].As<IMutableDictionaryContainer>()["city"].AsVariable.AsString, "seoul");
			Assert.AreEqual(_table[0]["adress"].As<IMutableDictionaryContainer>()["city"].AsVariable.AsString, "gogo");
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
		public void JsonToTable5(){
			ITableContainer<TestClass3> _table = new TableContainer<TestClass3>("test5");
			_table.PrimaryKey = "key1";
			string _json1 = "{\"data\":	[ [\"key4\", \"key1\", \"key2\", \"key3\"],[ [4,3,2,1], 123, 	\"string\",{\"key1\":119, \"key2\":\"test\"}]	, [[5,6,7,8], 2, \"string\", {\"key1\":222}]],\"name\":\"test\"}";
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
		public void JsonToTable6(){
			ITableContainer<TestClass4> _table = new TableContainer<TestClass4>("test6");
			_table.PrimaryKey = "key1";
			string _json1 = "{\"ir\":true,\"data\":[[\"MN\",\"CG\",\"IP\",\"SV\",\"TC\"],[1,1,false,false,0],[2,1,false,false,0],[3,1,false,false,0],[4,1,false,false,0],[5,1,true,false,0],[92,1,true,false,0],[7,2,true,false,0],[8,3,true,false,29],[9,3,true,false,98],[10,1,true,false,0],[11,3,true,false,48],[12,1,true,false,0],[13,0,false,true,0],[191,0,false,false,0],[193,0,false,false,0],[192,0,false,false,0],[196,0,false,false,0],[15,1,false,false,0],[14,1,true,false,0],[17,1,true,false,0]]}";
			int _counter1 = 0;

			JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json1, ref _counter1);

			Assert.AreEqual(_table[0].MapNo.AsInt, 1);
			Assert.AreEqual(_table[1].MapNo.AsInt, 2);

			Assert.AreEqual(_table[0].ClearGrade.AsInt, 1);
			Assert.AreEqual(_table[1].ClearGrade.AsInt, 1);


			Assert.AreEqual(_table[0].IsPerfect.AsBool, false);
			Assert.AreEqual(_table[1].IsPerfect.AsBool, false);
		}	

		[Test]
		public void BuildVariableTest(){
			string _json = "{\"head\":\"value\",\"list\":[100,200,300], \"data\":[{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"},{\"adress\":{\"city\":\"seoul\",\"country\":\"korea\"},\"age\":14,\"name\":\"js\"}], \"name\" : \"test\"}";
			int _counter = 0;

			var _value = JsonConvertor.GetInstance().BuildVariable(ref _json, ref _counter) as MutableDictionaryContainer;

			Assert.AreEqual(_value.Type, DataType.Dictionary);
			Assert.IsTrue(_value.ContainsKey("head"));
			Assert.AreEqual(_value["head"].AsVariable.AsString, "value");
			Assert.IsTrue(_value.ContainsKey("list"));
			Assert.AreEqual(_value["list"].Type, DataType.List);
			Assert.AreEqual(_value["list"].As<MutableListContainer>()[1].AsVariable.AsString, "200");
			Assert.IsTrue(_value.ContainsKey("data"));
			Assert.AreEqual(_value["name"].AsVariable.AsString, "test");

		}	

		[Test]
		public void BuildVariableTest2(){
			string _json = "{\"head\":\"value\",\"dict\":{\"k1\":100,\"k2\":200,\"k3\":300}, \"data\":[{\"adress\":{\"city\":\"gogo\",\"country\":\"korea\"},\"age\":13,\"name\":\"mike\"},{\"adress\":{\"city\":\"seoul\",\"country\":\"korea\"},\"age\":14,\"name\":\"js\"}], \"name\" : \"test\"}";
			int _counter = 0;

			var _value = JsonConvertor.GetInstance().BuildVariable(ref _json, ref _counter) as MutableDictionaryContainer;

			Assert.AreEqual(_value.Type, DataType.Dictionary);
			Assert.IsTrue(_value.ContainsKey("head"));
			Assert.AreEqual(_value["head"].AsVariable.AsString, "value");
			Assert.IsTrue(_value.ContainsKey("dict"));
			Assert.AreEqual(_value["dict"].Type, DataType.Dictionary);
			Assert.AreEqual(_value["dict"].As<IMutableDictionaryContainer>()["k1"].AsVariable.AsInt, 100);
			Assert.AreEqual(_value["dict"].As<IMutableDictionaryContainer>()["k2"].AsVariable.AsInt, 200);
			Assert.AreEqual(_value["dict"].As<IMutableDictionaryContainer>()["k3"].AsVariable.AsInt, 300);
			Assert.IsTrue(_value.ContainsKey("data"));
			Assert.AreEqual(_value["name"].AsVariable.AsString, "test");

		}	

		[Test]
		public void BuildListContainerTest(){
			string _json = "[\"test\" , \"test2\",\t\n \"test3\"]";
			int _counter = 0;
			MutableListContainer _value = new MutableListContainer();
			JsonConvertor.GetInstance ().BuildMutableListContainer(_value, ref _json, ref _counter);

			Assert.AreEqual (_value.As<MutableListContainer> () .Count, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].AsVariable.AsString, "test");
			Assert.AreEqual (_value.As<MutableListContainer> () [1].AsVariable.AsString, "test2");
			Assert.AreEqual (_value.As<MutableListContainer> () [2].AsVariable.AsString, "test3");
		}

		[Test]
		public void BuildListContainerTest2(){
			string _json = "[222 , 333,\t\n 444]";
			int _counter = 0;

			MutableListContainer _value = new MutableListContainer();
			JsonConvertor.GetInstance ().BuildMutableListContainer(_value, ref _json, ref _counter);

			Assert.AreEqual (_value.As<MutableListContainer> () .Count, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].AsVariable.AsInt, 222);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].AsVariable.AsInt, 333);
			Assert.AreEqual (_value.As<MutableListContainer> () [2].AsVariable.AsInt, 444);
		}

		[Test]
		public void BuildListContainerTest3(){
			string _json = "[222.1 , 333.0,\t\n 444.5]";
			int _counter = 0;

			MutableListContainer _value = new MutableListContainer();
			JsonConvertor.GetInstance ().BuildMutableListContainer(_value, ref _json, ref _counter);

			Assert.AreEqual (_value.As<MutableListContainer> () .Count, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].AsVariable.AsFloat, 222.1f);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].AsVariable.AsFloat, 333.0f);
			Assert.AreEqual (_value.As<MutableListContainer> () [2].AsVariable.AsFloat, 444.5f);
		}

		[Test]
		public void IsIntTest(){
			string _json = "111.2";

			Assert.AreEqual (JsonConvertor.GetInstance ().IsInt (ref _json, 0), false);


			_json = "111";

			Assert.AreEqual (JsonConvertor.GetInstance ().IsInt (ref _json, 0), true);
		}


		[Test]
		public void BuildListContainerTest4(){
			string _json = "[]";
			int _counter = 0;

			MutableListContainer _value = new MutableListContainer();
			JsonConvertor.GetInstance ().BuildMutableListContainer(_value, ref _json, ref _counter);

			Assert.AreEqual (_value.As<MutableListContainer> ().Count, 0);
		}


		[Test]
		public void BuildListContainerTest5(){
			string _json = "[true , false,\t\n false]";
			int _counter = 0;

			MutableListContainer _value = new MutableListContainer();
			JsonConvertor.GetInstance ().BuildMutableListContainer(_value, ref _json, ref _counter);

			Assert.AreEqual (_value.As<MutableListContainer> () .Count, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].AsVariable.AsBool, true);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].AsVariable.AsBool, false);
			Assert.AreEqual (_value.As<MutableListContainer> () [2].AsVariable.AsBool, false);
		}

		[Test]
		public void BuildListContainerTest6(){
			string _json = "[[1,2,3] , [4, 5, 6],\t\n [7, 8, 9]]";
			int _counter = 0;

			var _value = JsonConvertor.GetInstance ().BuildVariable (ref _json, ref _counter);
		
			Assert.AreEqual (_value.As<MutableListContainer> () .Count, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].As<MutableListContainer>()[0].Type, DataType.Int);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].As<MutableListContainer>()[0].AsVariable.AsInt, 1);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].As<MutableListContainer>()[1].AsVariable.AsInt, 2);
			Assert.AreEqual (_value.As<MutableListContainer> () [0].As<MutableListContainer>()[2].AsVariable.AsInt, 3);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].As<MutableListContainer>()[0].AsVariable.AsInt, 4);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].As<MutableListContainer>()[1].AsVariable.AsInt, 5);
			Assert.AreEqual (_value.As<MutableListContainer> () [1].As<MutableListContainer>()[2].AsVariable.AsInt, 6);

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

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_table, _stringBuilder, null);

			Assert.AreEqual("{\"test\":\"testvalue\",\"data\":[{\"key1\":1,\"key2\":\"two\",\"key3\":{\"key1\":2}},{\"key1\":12,\"key2\":\"two2\",\"key3\":{\"key1\":22}}]}", _stringBuilder.ToString());
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
			MutableDictionaryContainer _dict = new MutableDictionaryContainer();
			_dict.Add("test", new StringVariable("test"));
			_dict.Add("key1", new StringVariable("test1"));

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_dict, _stringBuilder);

			Assert.AreEqual("{\"test\":\"test\",\"key1\":\"test1\"}", _stringBuilder.ToString());
		}

		[Test]
		public void ListToJson(){
			ListContainer<StringVariable> _dict = new ListContainer<StringVariable>();
			_dict.Add(new StringVariable("test0"));
			_dict.Add(new StringVariable("test1"));

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_dict, _stringBuilder);

			Assert.AreEqual("[\"test0\",\"test1\"]", _stringBuilder.ToString());
		}

		[Test]
		public void ModelToJson(){
			TestClass _model = new TestClass();
			_model.member1.AsInt = 999;
			_model.member2.AsString = "test";
			_model.member3.member1.AsInt = 888;

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_model, _stringBuilder);

			Assert.AreEqual("{\"key1\":999,\"key2\":\"test\",\"key3\":{\"key1\":888}}", _stringBuilder.ToString());
		}


		[Test]
		public void StringToJson(){
			StringVariable _value = new StringVariable();
			_value.AsString = "good";

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_value, _stringBuilder);

			Assert.AreEqual("\"good\"", _stringBuilder.ToString());
		}

		[Test]
		public void EnumToJson(){
			EnumVariable<TestEnum> _value = new EnumVariable<TestEnum>();
			_value.AsEnum = TestEnum.three;

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			JsonConvertor.GetInstance().BuildFormattedString(_value, _stringBuilder);

			Assert.AreEqual("\"three\"", _stringBuilder.ToString());
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

		 public class TestClass4 : RecordContainer{
			#region FieldName
			private const string MAP_NO = "MN";
			private const string CLEAR_GRADE = "CG";
			private const string IS_PERFECT = "IP";
			private const string HAS_SAVING = "SV";
			private const string IS_REWARDED = "IR";
			private const string TIME_RECORD = "TC";
			#endregion

			#region Field
			public IntVariable MapNo = new IntVariable(0);
			public IntVariable ClearGrade = new IntVariable(0);
			public BoolVariable IsPerfect = new BoolVariable();
			public BoolVariable HasSaving = new BoolVariable(false);
			public IntVariable TimeRecord = new IntVariable(0);
			#endregion

			#region LifeCycle
			public TestClass4(){
				AddManagedColumn(MAP_NO, this.MapNo);
				AddManagedColumn(CLEAR_GRADE, this.ClearGrade);
				AddManagedColumn(IS_PERFECT, this.IsPerfect);
				AddManagedColumn(HAS_SAVING, this.HasSaving);
				AddManagedColumn(TIME_RECORD, this.TimeRecord);
			}
			#endregion
		}


		public class TestClass5 : RecordContainer{
			public Vector2Variable vector = new Vector2Variable(0, 0);

			public TestClass5(){
				AddManagedColumn("v", this.vector); 
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
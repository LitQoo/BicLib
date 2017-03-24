using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BicDB.Utility;
using BicDB.Container;
using BicDB.Variable;

namespace BicDB.Container
{
	public class DictionaryTest {


		[Test]
		public void CreateStringTest1()
		{

			DictionaryContainer _var = new DictionaryContainer ();
			string _json = "{\"test1\" : \"test\", \"test2\":\"123a\", \"test3\":\t\"456\"}";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var.GetValue<StringVariable>("test1").AsString, "test");
			Assert.AreEqual(_var.GetValue<StringVariable>("test2").AsString, "123a");
			Assert.AreEqual(_var.GetValue<StringVariable>("test3").AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			DictionaryContainer _var = new DictionaryContainer ();
			string _json = "{\"test1\" : 123, \"test2\":	456, \"test3\":789, \"test4\":null, \"test5\":true, \"test6\":12.34, \"test7\":false, \"test8\":true  }";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var.GetValue<IVariable>("test1").AsInt, 123);
			Assert.AreEqual(_var.GetValue<IVariable>("test2").AsInt, 456);
			Assert.AreEqual(_var.GetValue<IVariable>("test3").AsInt, 789);
			Assert.AreEqual(_var.GetValue<IVariable>("test3").AsInt, 789);
			Assert.AreEqual(_var["test4"], null);
			Assert.AreEqual(_var.ContainsKey("test4"), true);
			Assert.AreEqual(_var.GetValue<IVariable>("test5").AsBool, true);
			Assert.AreEqual(_var.GetValue<IVariable>("test6").AsFloat, 12.34f);
			Assert.AreEqual(_var.GetValue<IVariable>("test7").AsBool, false);
			Assert.AreEqual(_var.GetValue<IVariable>("test8").AsBool, true);
		}

		[Test]
		public void GetSizeTest()
		{
			DictionaryContainer _var = new DictionaryContainer ();
			_var.Add("t1", new StringVariable("1"));
			_var.Add("t2", new StringVariable("3"));
			_var.Add("t3", new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);
		}

		[Test]
		public void AddTest()
		{
			DictionaryContainer _var = new DictionaryContainer ();
			_var.Add("test", new StringVariable("1"));

			Assert.AreEqual(_var.GetValue<StringVariable>("test").AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			DictionaryContainer _var = new DictionaryContainer ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);

			_var.Remove("key1");
			Assert.AreEqual(_var.Count, 2);
			Assert.AreEqual(_var.GetValue<StringVariable>("key2").AsString, "2");
		}

	

		[Test]
		public void ClearTest1()
		{
			DictionaryContainer _var = new DictionaryContainer ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			_var.Clear ();

			Assert.AreEqual(_var.ContainsKey("key1"), false);
			Assert.AreEqual(_var.ContainsKey("key6"), false);
			Assert.AreEqual(_var.Count, 0);
		}


	}
}
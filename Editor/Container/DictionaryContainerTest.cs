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

			Assert.AreEqual(_var["test1"].AsVariable.AsString, "test");
			Assert.AreEqual(_var["test2"].AsVariable.AsString, "123a");
			Assert.AreEqual(_var["test3"].AsVariable.AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			DictionaryContainer _var = new DictionaryContainer ();
			string _json = "{\"test1\" : 123, \"test2\":	456, \"test3\":789, \"test4\":null, \"test5\":true, \"test6\":12.34, \"test7\":false, \"test8\":true  }";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var["test1"].AsVariable.AsInt, 123);
			Assert.AreEqual(_var["test2"].AsVariable.AsInt, 456);
			Assert.AreEqual(_var["test3"].AsVariable.AsInt, 789);
			Assert.AreEqual(_var["test4"], null);
			Assert.AreEqual(_var.ContainsKey("test4"), true);
			Assert.AreEqual(_var["test5"].AsVariable.AsBool, true);
			Assert.AreEqual(_var["test6"].AsVariable.AsFloat, 12.34f);
			Assert.AreEqual(_var["test7"].AsVariable.AsBool, false);
			Assert.AreEqual(_var["test8"].AsVariable.AsBool, true);
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

			Assert.AreEqual(_var["test"].AsVariable.AsString, "1");
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
			Assert.AreEqual(_var["key2"].AsVariable.AsString, "2");
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
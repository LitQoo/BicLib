using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BicDB.Utility;
using BicDB.Container;
using BicDB.Container;

namespace BicDB.Container
{
	public class DictionaryTest {


		[Test]
		public void CreateStringTest1()
		{

			DictionaryContainer<StringVariable> _var = new DictionaryContainer<StringVariable> ();
			string _json = "{\"test1\" : \"test\", \"test2\":\"123a\", \"test3\":\t\"456\"}";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var["test1"].AsString, "test");
			Assert.AreEqual(_var["test2"].AsString, "123a");
			Assert.AreEqual(_var["test3"].AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			DictionaryContainer<IntVariable> _var = new DictionaryContainer<IntVariable> ();
			string _json = "{\"test1\" : 123, \"test2\":	456, \"test3\":789}";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var["test1"].AsInt, 123);
			Assert.AreEqual(_var["test2"].AsInt, 456);
			Assert.AreEqual(_var["test3"].AsInt, 789);
		}

		[Test]
		public void GetSizeTest()
		{
			DictionaryContainer<StringVariable> _var = new DictionaryContainer<StringVariable> ();
			_var.Add("t1", new StringVariable("1"));
			_var.Add("t2", new StringVariable("3"));
			_var.Add("t3", new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);
		}

		[Test]
		public void AddTest()
		{
			DictionaryContainer<StringVariable> _var = new DictionaryContainer<StringVariable> ();
			_var.Add("test", new StringVariable("1"));

			Assert.AreEqual(_var["test"].AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			DictionaryContainer<StringVariable> _var = new DictionaryContainer<StringVariable> ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);

			_var.Remove("key1");
			Assert.AreEqual(_var.Count, 2);
			Assert.AreEqual(_var["key2"].AsString, "2");
		}

	

		[Test]
		public void ClearTest1()
		{
			DictionaryContainer<StringVariable> _var = new DictionaryContainer<StringVariable> ();
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
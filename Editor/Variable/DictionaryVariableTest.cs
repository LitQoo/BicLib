using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class DictionaryVariableTest {


		[Test]
		public void CreateStringTest1()
		{

			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			string _json = "{\"test1\" : \"test\", \"test2\":\"123a\", \"test3\":\t\"456\"}";
			int _counter = 0;
			_var.LoadFormatString(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var["test1"].AsString, "test");
			Assert.AreEqual(_var["test2"].AsString, "123a");
			Assert.AreEqual(_var["test3"].AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			DictionaryVariable<IntVariable> _var = new DictionaryVariable<IntVariable> ();
			string _json = "{\"test1\" : 123, \"test2\":	456, \"test3\":789}";
			int _counter = 0;
			_var.LoadFormatString(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var["test1"].AsInt, 123);
			Assert.AreEqual(_var["test2"].AsInt, 456);
			Assert.AreEqual(_var["test3"].AsInt, 789);
		}

		[Test]
		public void GetSizeTest()
		{
			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			_var.Add("t1", new StringVariable("1"));
			_var.Add("t2", new StringVariable("3"));
			_var.Add("t3", new StringVariable("3"));

			Assert.AreEqual(_var.GetSize(), 3);
		}

		[Test]
		public void AddTest()
		{
			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			_var.Add("test", new StringVariable("1"));

			Assert.AreEqual(_var["test"].AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			Assert.AreEqual(_var.GetSize(), 3);

			_var.RemoveAt("key1");
			Assert.AreEqual(_var.GetSize(), 2);
			Assert.AreEqual(_var["key2"].AsString, "2");
		}

		[Test]
		public void ContainsTest1()
		{
			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			Assert.AreEqual(_var.Contains(new StringVariable("1")), true);
			Assert.AreEqual(_var.Contains(new StringVariable("6")), false);
			Assert.AreEqual(_var.Contains("key1"), true);
			Assert.AreEqual(_var.Contains("key6"), false);
		}

		[Test]
		public void ClearTest1()
		{
			DictionaryVariable<StringVariable> _var = new DictionaryVariable<StringVariable> ();
			_var.Add("key1", new StringVariable("1"));
			_var.Add("key2", new StringVariable("2"));
			_var.Add("key3", new StringVariable("3"));

			_var.Clear ();

			Assert.AreEqual(_var.Contains(new StringVariable("1")), false);
			Assert.AreEqual(_var.Contains(new StringVariable("6")), false);
			Assert.AreEqual(_var.Contains("key1"), false);
			Assert.AreEqual(_var.Contains("key6"), false);
			Assert.AreEqual(_var.GetSize(), 0);
		}

		[Test]
		public void IsEqualTest1(){
			DictionaryVariable<StringVariable> _var1 = new DictionaryVariable<StringVariable> ();
			DictionaryVariable<StringVariable> _var2 = new DictionaryVariable<StringVariable> ();

			_var1.Add("key", new StringVariable("1"));
			_var2.Add("key", new StringVariable("2"));

			Assert.AreEqual(_var1.IsEqual(_var2), false);
			Assert.AreEqual(_var1.IsEqual(_var2), false);

			_var2["key"].AsString = "1";

			Assert.AreEqual(_var1.IsEqual(_var2), false);
			Assert.AreEqual(_var1.IsEqual(_var2), false);
		}
	}
}
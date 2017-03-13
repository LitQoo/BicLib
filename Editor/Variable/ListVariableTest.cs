using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class ListVariableTest
	{
		[Test]
		public void CreateStringTest1()
		{

			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();

			string _json = "[\"test\", \"123\", \"456\"]";
			int _counter = 0;
			_var.LoadFormatString(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsString, "test");
			Assert.AreEqual(_var[1].AsString, "123");
			Assert.AreEqual(_var[2].AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			ListVariable<IntVariable> _var = new ListVariable<IntVariable> ();
			string _json = "[123,456,789]";
			int _counter = 0;
			_var.LoadFormatString(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsInt, 123);
			Assert.AreEqual(_var[1].AsInt, 456);
			Assert.AreEqual(_var[2].AsInt, 789);
		}

		[Test]
		public void GetSizeTest()
		{
			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("3"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.GetSize(), 3);
		}

		[Test]
		public void AddTest()
		{
			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();
			_var.Add(new StringVariable("1"));

			Assert.AreEqual(_var[0].AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("2"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.GetSize(), 3);

			_var.RemoveAt(0);
			Assert.AreEqual(_var.GetSize(), 2);
			Assert.AreEqual(_var[0].AsString, "2");
		}

		[Test]
		public void ContainsTest()
		{
			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("2"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.Contains(new StringVariable("1")), true);
			Assert.AreEqual(_var.Contains(new StringVariable("6")), false);
		}

		[Test]
		public void IsEqualTest1(){
			ListVariable<StringVariable> _var1 = new ListVariable<StringVariable> ();
			ListVariable<StringVariable> _var2 = new ListVariable<StringVariable> ();

			_var1.Add(new StringVariable("1"));
			_var2.Add(new StringVariable("2"));

			Assert.AreEqual(_var1.IsEqual(_var2), false);

			_var2[0].AsString = "1";

			Assert.AreEqual(_var1.IsEqual(_var2), false);
		}

		[Test]
		public void OnChangedElementNotifyTest(){
			ListVariable<StringVariable> _var1 = new ListVariable<StringVariable> ();
			int _count = 0;
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("1"));

			_var1.OnChangedElementActions[0] += (int _index, StringVariable _variable) => {
				if(_variable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[0] += (int _index, StringVariable _variable) => {
				if(_variable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[1] += (int _index, StringVariable _variable) => {
				_count = -1;
			};

			_var1[0] = new StringVariable("test");

			Assert.AreEqual(_count, 2);
		}
	}
}


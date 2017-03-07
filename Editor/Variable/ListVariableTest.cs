using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;

namespace BicDB.Variable
{
	public class ListVariableTest
	{
		[Test]
		public void CreateStringTest1()
		{

			ListVariable<StringVariable> _var = new ListVariable<StringVariable> ();
			_var.LoadValue("[\"test\", \"123\", \"456\"]");
			Assert.AreEqual(_var[0].AsString, "test");
			Assert.AreEqual(_var[1].AsString, "123");
			Assert.AreEqual(_var[2].AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			ListVariable<IntVariable> _var = new ListVariable<IntVariable> ();
			_var.LoadValue("[123,456,789]");
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

			Assert.AreEqual(_var1.IsEqualExactly(_var2), false);
			Assert.AreEqual(_var1.IsEqualGenerally(_var2), false);

			_var2[0].AsString = "1";

			Assert.AreEqual(_var1.IsEqualExactly(_var2), true);
			Assert.AreEqual(_var1.IsEqualGenerally(_var2), true);
		}
	}
}


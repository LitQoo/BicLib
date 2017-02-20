using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariableTest {

		[Test]
		public void CreateTest()
		{

			StringVariable _var = new StringVariable ("test");
			Assert.AreEqual(_var.AsString, "test");
		}

		[Test]
		public void AsIntOutTest()
		{

			StringVariable _var = new StringVariable ("123");
			Assert.AreEqual(_var.AsInt, 123);
		}

		[Test]
		public void AsFloatOutTest()
		{

			StringVariable _var = new StringVariable ("123.132");
			Assert.AreEqual (_var.AsFloat, 123.132f);;
		}

		[Test]
		public void AsStringOutTest()
		{

			StringVariable _var = new StringVariable ("abc");
			Assert.AreEqual(_var.AsString, "abc");
		}

		[Test]
		public void AsIntInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsInt = 1234;
			Assert.AreEqual(_var.AsString, "1234");
		}

		[Test]
		public void AsFloatInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsFloat = 123.123f;
			Assert.AreEqual(_var.AsString, "123.123");
		}

		[Test]
		public void AsStringInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsString = "abc";
			Assert.AreEqual(_var.AsString, "abc");
		}

		[Test]
		public void AsBoolInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsBool = true;
			Assert.AreEqual(_var.AsString, "true");
		}

		[Test]
		public void AsBoolOutTest()
		{

			StringVariable _var = new StringVariable ("true");
			Assert.AreEqual(_var.AsBool, true);
		}


		[Test]
		public void TypeTest()
		{

			StringVariable _var = new StringVariable ("abcd");
			Assert.AreEqual(_var.Type, VariableType.String);
		}

		[Test]
		public void OnChangedValueTest(){
			StringVariable _var = new StringVariable ("def");
			string _changeValue = "change";

			_var.OnChangedValueActions += (IVariable _changer) => {
				if(_changer.AsString != _changeValue){
					Assert.Fail();
				}
			};

			_var.AsString = _changeValue;

			Assert.Pass();
		}

		[Test]
		public void VirtualStringTest(){
			Assert.Ignore ();
			// need int, float, bool ... virtual test
		}
	}
}
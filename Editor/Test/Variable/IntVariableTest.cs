using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class IntVariableTest {


		[Test]
		public void CreateTest()
		{

			IntVariable _var = new IntVariable (145);
			Assert.AreEqual(_var.AsInt, 145);
		}

		[Test]
		public void AsIntOutTest()
		{

			IntVariable _var = new IntVariable (145);
			Assert.AreEqual(_var.AsInt, 145);
		}

		[Test]
		public void AsFloatOutTest()
		{

			IntVariable _var = new IntVariable (215);
			Assert.AreEqual(_var.AsFloat, 215.0f);
		}

		[Test]
		public void AsStringOutTest()
		{

			IntVariable _var = new IntVariable (425);
			Assert.AreEqual(_var.AsString, "425");
		}

		[Test]
		public void AsIntInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsInt = 425;
			Assert.AreEqual(_var.AsInt, 425);
		}

		[Test]
		public void AsFloatInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsFloat = 143.3f;
			Assert.AreEqual(_var.AsInt, 143);
		}

		[Test]
		public void AsStringInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsString = "183";
			Assert.AreEqual(_var.AsInt, 183);
		}

		[Test]
		public void TypeTest()
		{

			IntVariable _var = new IntVariable (124);
			Assert.AreEqual(_var.Type, DataType.Int);
		}

		[Test]
		public void OnChangedValueTest(){
			IntVariable _var = new IntVariable (124);
			int _changeValue = 222;

			_var.Subscribe((IVariable _changer) => {
				if(_changer.AsFloat != _changeValue){
					Assert.Fail();
				}
			});

			_var.AsInt = _changeValue;

			Assert.Pass();
		}

		[Test]
		public void IsEqualTest1(){
			IntVariable _var1 = new IntVariable(123);
			IntVariable _var2 = new IntVariable(44444);

			Assert.AreEqual(_var1.IsEqual(_var2), false);
			Assert.AreEqual(_var1.IsEqual(_var2), false);

			_var2.AsInt = 123;

			Assert.AreEqual(_var1.IsEqual(_var2), true);
			Assert.AreEqual(_var1.IsEqual(_var2), true);
		}

		[Test]
		public void IsEqualTest2(){
			IntVariable _var1 = new IntVariable(123);
			StringVariable _var2 = new StringVariable("4444");

			Assert.AreEqual(_var1.IsEqual(_var2), false);

			_var2.AsString = "123";

			Assert.AreEqual(_var1.IsEqual(_var2), false);
		}
	}
}
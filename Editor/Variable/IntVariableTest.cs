using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
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
			Assert.AreEqual(_var.Type, VariableType.Int);
		}

		[Test]
		public void OnChangedValueTest(){
			IntVariable _var = new IntVariable (124);
			int _changeValue = 222;

			_var.OnChangedValueActions += (IVariable _changer) => {
				if(_changer.AsFloat != _changeValue){
					Assert.Fail();
				}
			};

			_var.AsInt = _changeValue;

			Assert.Pass();
		}
	}
}
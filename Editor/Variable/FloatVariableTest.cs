using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class FloatVariableTest {

		[Test]
		public void CreateTest()
		{
			FloatVariable _var = new FloatVariable (3845.23f);
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void AsIntOutTest()
		{

			FloatVariable _var = new FloatVariable (3845.23f);
			Assert.AreEqual(_var.AsInt, 3845);
		}

		[Test]
		public void AsFloatOutTest()
		{

			FloatVariable _var = new FloatVariable (3845.23f);
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void AsStringOutTest()
		{

			FloatVariable _var = new FloatVariable (3845.23f);
			Assert.AreEqual(_var.AsString, "3845.23");
		}

		[Test]
		public void AsIntInTest()
		{

			FloatVariable _var = new FloatVariable (0);
			_var.AsInt = 3823;
			Assert.AreEqual(_var.AsFloat, 3823.0f);
		}

		[Test]
		public void AsFloatInTest()
		{

			FloatVariable _var = new FloatVariable (0);
			_var.AsFloat = 3845.23f;
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void AsStringInTest()
		{

			FloatVariable _var = new FloatVariable (0);
			_var.AsString = "3845.23";
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void TypeTest()
		{

			FloatVariable _var = new FloatVariable (3845.2f);
			Assert.AreEqual(_var.Type, VariableType.Float);
		}

		[Test]
		public void OnChangedValueTest(){
			FloatVariable _var = new FloatVariable (3845.2f);
			float _changeValue = 222;

			_var.OnChangedValueActions += (IVariable _changer) => {
				if(_changer.AsFloat != _changeValue){
					Assert.Fail();
				}
			};

			_var.AsFloat = _changeValue;

			Assert.Pass();
		}
	}
}

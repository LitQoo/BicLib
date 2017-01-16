using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class IntVariableTest {
		private const int _testValue = 823;

		[Test]
		public void CreateTest()
		{

			IntVariable _var = new IntVariable (_testValue);
			Assert.AreEqual(_var.AsInt, _testValue);
		}

		[Test]
		public void AsIntOutTest()
		{

			IntVariable _var = new IntVariable (_testValue);
			Assert.AreEqual(_var.AsInt, _testValue);
		}

		[Test]
		public void AsFloatOutTest()
		{

			IntVariable _var = new IntVariable (_testValue);
			Assert.AreEqual(_var.AsFloat, (float)_testValue);
		}

		[Test]
		public void AsStringOutTest()
		{

			IntVariable _var = new IntVariable (_testValue);
			Assert.AreEqual(_var.AsString, _testValue.ToString());
		}

		[Test]
		public void AsIntInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsInt = _testValue;
			Assert.AreEqual(_var.AsInt, _testValue);
		}

		[Test]
		public void AsFloatInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsFloat = (float)_testValue;
			Assert.AreEqual(_var.AsFloat, (float)_testValue);
		}

		[Test]
		public void AsStringInTest()
		{

			IntVariable _var = new IntVariable (0);
			_var.AsString = _testValue.ToString ();
			Assert.AreEqual(_var.AsString, _testValue.ToString());
		}

		[Test]
		public void TypeTest()
		{

			IntVariable _var = new IntVariable (_testValue);
			Assert.AreEqual(_var.Type, VariableType.Int);
		}

		[Test]
		public void OnChangedValueTest(){
			IntVariable _var = new IntVariable (_testValue);
			int _changeValue = 222;
			bool _checkResult = false;

			_var.OnChangedValueActions += (IVariable _changer) => {
				if(_changer.AsInt == _changeValue){
					_checkResult = true;
				}
			};

			_var.AsInt = _changeValue;

			Assert.IsTrue (_checkResult);
		}

		[Test]
		public void EqualsTestWithInt(){
			Assert.Fail();
		}

		[Test]
		public void EqualsTestWithString(){
			Assert.Fail();
		}

		[Test]
		public void EqualsTestWithFloat(){
			Assert.Fail();
		}
	}
}
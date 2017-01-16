using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariableTest {
		private const string _testValue = "123";

		[Test]
		public void CreateTest()
		{

			StringVariable _var = new StringVariable (_testValue);
			Assert.AreEqual(_var.AsString, _testValue);
		}

		[Test]
		public void AsIntOutTest()
		{

			StringVariable _var = new StringVariable (_testValue);
			Assert.AreEqual(_var.AsInt, int.Parse(_testValue));
		}

		[Test]
		public void AsFloatOutTest()
		{

			StringVariable _var = new StringVariable (_testValue);
			Assert.AreEqual (_var.AsFloat, float.Parse (_testValue));;
		}

		[Test]
		public void AsStringOutTest()
		{

			StringVariable _var = new StringVariable (_testValue);
			Assert.AreEqual(_var.AsString, _testValue.ToString());
		}

		[Test]
		public void AsIntInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsInt = int.Parse(_testValue);
			Assert.AreEqual(_var.AsInt, int.Parse(_testValue));
		}

		[Test]
		public void AsFloatInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsFloat = float.Parse(_testValue);
			Assert.AreEqual(_var.AsFloat, float.Parse(_testValue));
		}

		[Test]
		public void AsStringInTest()
		{

			StringVariable _var = new StringVariable ("");
			_var.AsString = _testValue.ToString ();
			Assert.AreEqual(_var.AsString, _testValue.ToString());
		}

		[Test]
		public void TypeTest()
		{

			StringVariable _var = new StringVariable (_testValue);
			Assert.AreEqual(_var.Type, VariableType.String);
		}

		[Test]
		public void OnChangedValueTest(){
			StringVariable _var = new StringVariable (_testValue);
			string _changeValue = "change";
			bool _checkResult = false;

			_var.OnChangedValueActions += (IVariable _changer) => {
				if(_changer.AsString == _changeValue){
					_checkResult = true;
				}
			};

			_var.AsString = _changeValue;

			Assert.IsTrue (_checkResult);
		}
	}
}
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class BoolVariableTest
	{

		[Test]
		public void CreateTest()
		{

			BoolVariable _var = new BoolVariable (true);
			Assert.AreEqual(_var.AsBool, true);
		}

		[Test]
		public void AsIntOutTest()
		{

			BoolVariable _var = new BoolVariable (true);
			Assert.AreEqual(_var.AsInt, 1);

			BoolVariable _var2 = new BoolVariable (false);
			Assert.AreEqual(_var2.AsInt, 0);
		}

		[Test]
		public void AsFloatOutTest()
		{

			BoolVariable _var1 = new BoolVariable (true);
			Assert.AreEqual(_var1.AsFloat, 1);

			BoolVariable _var2 = new BoolVariable (false);
			Assert.AreEqual(_var2.AsFloat, 0);
		}

		[Test]
		public void AsStringOutTest()
		{

			BoolVariable _var = new BoolVariable (true);
			Assert.AreEqual(_var.AsString, "true");

			BoolVariable _var2 = new BoolVariable (false);
			Assert.AreEqual(_var2.AsString, "false");
		}

		[Test]
		public void AsIntInTest()
		{
			BoolVariable _var = new BoolVariable (false);
			_var.AsInt = 0;
			Assert.AreEqual(_var.AsBool, false);

			BoolVariable _var2 = new BoolVariable (false);
			_var2.AsInt = 1;
			Assert.AreEqual(_var2.AsBool, true);
		}

		[Test]
		public void AsFloatInTest()
		{

			BoolVariable _var = new BoolVariable (false);
			_var.AsFloat = 1.0f;
			Assert.AreEqual(_var.AsBool, true);

			BoolVariable _var2 = new BoolVariable (true);
			_var2.AsFloat = 0.0f;
			Assert.AreEqual(_var2.AsBool, false);
		}

		[Test]
		public void AsStringInTest()
		{

			BoolVariable _var = new BoolVariable (false);
			_var.AsString = "true";
			Assert.AreEqual(_var.AsBool, true);

			BoolVariable _var2 = new BoolVariable (false);
			_var2.AsString = "false";
			Assert.AreEqual(_var2.AsBool, false);

			BoolVariable _var3 = new BoolVariable (false);
			_var3.AsString = "True";
			Assert.AreEqual(_var3.AsBool, true);

			BoolVariable _var4 = new BoolVariable (false);
			_var4.AsString = "TRUE";
			Assert.AreEqual(_var4.AsBool, true);

			BoolVariable _var5 = new BoolVariable (true);
			_var5.AsString = "False";
			Assert.AreEqual(_var5.AsBool, false);
		}

		[Test]
		public void AsBoolInTest()
		{

			BoolVariable _var = new BoolVariable (false);
			_var.AsBool = true;
			Assert.AreEqual(_var.AsBool, true);

			BoolVariable _var2 = new BoolVariable (false);
			_var2.AsBool = false;
			Assert.AreEqual(_var2.AsBool, false);
		}

		[Test]
		public void AsBoolOutTest()
		{
			BoolVariable _var = new BoolVariable (false);
			_var.AsBool = true;
			Assert.AreEqual(_var.AsBool, true);

			BoolVariable _var2 = new BoolVariable (false);
			_var2.AsBool = false;
			Assert.AreEqual(_var2.AsBool, false);
		}

		[Test]
		public void TypeTest()
		{

			BoolVariable _var = new BoolVariable (true);
			Assert.AreEqual(_var.Type, DataType.Bool);
		}

		[Test]
		public void OnChangedValueTest(){
			BoolVariable _var = new BoolVariable (false);
			bool _changeValue = true;

			_var.Subscribe((IVariableReadOnly _changer) => {
				if(_changer.AsBool != _changeValue){
					Assert.Fail();
				}
			});

			_var.AsBool = _changeValue;

			Assert.Pass();
		}

		[Test]
		public void RemoveAllOnChangedValueTest(){
			BoolVariable _var = new BoolVariable (false);

			_var.Subscribe((IVariableReadOnly _changer) => {
				Assert.Fail();
			});

			_var.UnsubscribeAll ();

			_var.AsBool = true;

			Assert.Pass();
		}
	}
}
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class EnumVariableTest {

		enum TestEnum
		{
			one = 1,
			two = 2,
			three = 3
		}

		[Test]
		public void CreateTest()
		{

			IVariable _var = new EnumVariable<TestEnum> (TestEnum.one);
			Assert.AreEqual(_var.AsInt, 1);
			Assert.AreEqual((TestEnum)_var.AsInt, TestEnum.one);
		}

		[Test]
		public void AsIntOutTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (TestEnum.two);
			Assert.AreEqual(_var.AsInt, 2);
		}

		[Test]
		public void AsFloatOutTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (TestEnum.two);
			Assert.AreEqual(_var.AsFloat, 2.0f);
		}

		[Test]
		public void AsEnumOutTest()
		{

			EnumVariable<TestEnum> _var = new  EnumVariable<TestEnum> (TestEnum.two);

			Assert.AreEqual(_var.AsEnum, TestEnum.two);
		}

		[Test]
		public void AsStringOutTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (TestEnum.three);
			Assert.AreEqual(_var.AsString, "three");
		}


		[Test]
		public void AsEnumInTest()
		{

			IEnumVariable<TestEnum> _var = new  EnumVariable<TestEnum> (TestEnum.two);

			_var.AsEnum = TestEnum.three;

			Assert.AreEqual(_var.AsEnum, TestEnum.three);
		}

		[Test]
		public void AsIntInTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (0);
			_var.AsInt = 2;
			Assert.AreEqual(_var.AsInt, 2);
		}

		[Test]
		public void AsFloatInTest()
		{

			IVariable _var = new  EnumVariable<TestEnum>(0);
			_var.AsFloat = 2f;
			Assert.AreEqual(_var.AsInt, 2);
		}

		[Test]
		public void AsStringInTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (0);
			_var.AsString = "3";
			Assert.AreEqual(_var.AsInt, 3);
			_var.AsString = "three";
			Assert.AreEqual(_var.AsInt, 3);
			_var.AsString = "three";
			Assert.AreEqual((TestEnum)_var.AsInt, TestEnum.three);
		}

		[Test]
		public void TypeTest()
		{

			IVariable _var = new  EnumVariable<TestEnum> (TestEnum.three);
			Assert.AreEqual(_var.Type, DataType.Int);
		}

		[Test]
		public void OnChangedValueTest(){
			IVariable _var = new  EnumVariable<TestEnum> (TestEnum.three);
			int _changeValue = 1;

			_var.OnChangedValueActions += (IVariable _changer, string _message) => {
				if(_changer.AsFloat != _changeValue){
					Assert.Fail();
				}
			};

			_var.AsInt = _changeValue;

			Assert.Pass();
		}
			
		[Test]
		public void IsEqualTest1(){
			EnumVariable<TestEnum> _var1 = new EnumVariable<TestEnum>(TestEnum.one);
			EnumVariable<TestEnum> _var2 = new EnumVariable<TestEnum>(TestEnum.two);

			Assert.AreEqual(_var1.IsEqual(_var2), false);
			Assert.AreEqual(_var1.IsEqual(_var2), false);

			_var2.AsEnum = TestEnum.one;

			Assert.AreEqual(_var1.IsEqual(_var2), true);
			Assert.AreEqual(_var1.IsEqual(_var2), true);
		}
	}
}
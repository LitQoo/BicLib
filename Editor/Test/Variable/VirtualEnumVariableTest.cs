using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class VirtualEnumVariableTest {

		enum TestEnum
		{
			one = 1,
			two = 2,
			three = 3
		}

		[Test]
		public void CreateTest()
		{

			IVariable _var = new VirtualEnumVariable<TestEnum> (()=>TestEnum.one);
			Assert.AreEqual(_var.AsInt, 1);
			Assert.AreEqual((TestEnum)_var.AsInt, TestEnum.one);
		}

		[Test]
		public void AsIntOutTest()
		{

			IVariable _var = new  VirtualEnumVariable<TestEnum> (()=>TestEnum.two);
			Assert.AreEqual(_var.AsInt, 2);
		}

		[Test]
		public void AsFloatOutTest()
		{

			IVariable _var = new  VirtualEnumVariable<TestEnum> (()=>TestEnum.two);
			Assert.AreEqual(_var.AsFloat, 2.0f);
		}

		[Test]
		public void AsStringOutTest()
		{

			IVariable _var = new  VirtualEnumVariable<TestEnum> (()=>TestEnum.three);
			Assert.AreEqual(_var.AsString, "three");
		}

		[Test]
		public void AsIntInTest()
		{

			IVariable _var = new  VirtualEnumVariable<TestEnum> (()=>0);
			try {
				_var.AsInt = 2;
			} catch (Exception) {
				Assert.Pass();
			}

			Assert.Fail();
		}

		[Test]
		public void TypeTest()
		{

			IVariable _var = new  VirtualEnumVariable<TestEnum> (()=>TestEnum.three);
			Assert.AreEqual(_var.Type, DataType.Enum);
		}
	}
}
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class VirtualIntVariableTest {


		[Test]
		public void CreateTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 145;});
			Assert.AreEqual(_var.AsInt, 145);
		}

		[Test]
		public void AsIntOutTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 145;});
			Assert.AreEqual(_var.AsInt, 145);
		}

		[Test]
		public void AsFloatOutTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 215;});
			Assert.AreEqual(_var.AsFloat, 215.0f);
		}

		[Test]
		public void AsStringOutTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 425;});
			Assert.AreEqual(_var.AsString, "425");
		}

		[Test]
		public void AsIntInTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 0;});
			try {
				_var.AsInt = 425;
			} catch (Exception) {
				Assert.Pass();
			}

			Assert.Fail();
		}

		[Test]
		public void TypeTest()
		{

			VirtualIntVariable _var = new VirtualIntVariable (()=>{return 124;});
			Assert.AreEqual(_var.Type, DataType.Int);
		}
	}
}
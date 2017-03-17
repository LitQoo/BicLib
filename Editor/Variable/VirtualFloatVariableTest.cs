using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;

namespace BicDB.Container
{
	public class VirtualFloatVariableTest {

		[Test]
		public void CreateTest()
		{
			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 3845.23f;});
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void AsIntOutTest()
		{

			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 3845.23f;});
			Assert.AreEqual(_var.AsInt, 3845);
		}

		[Test]
		public void AsFloatOutTest()
		{

			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 3845.23f;});
			Assert.AreEqual(_var.AsFloat, 3845.23f);
		}

		[Test]
		public void AsStringOutTest()
		{

			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 3845.23f;});
			Assert.AreEqual(_var.AsString, "3845.23");
		}

		[Test]
		public void AsFloatInTest()
		{
			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 0;});

			try {
				_var.AsFloat = 3845.23f;
			} catch (Exception) {
				Assert.Pass();
			}

			Assert.Fail();
		}

		[Test]
		public void TypeTest()
		{

			VirtualFloatVariable _var = new VirtualFloatVariable (()=>{return 3845.23f;});
			Assert.AreEqual(_var.Type, DataType.Float);
		}
	}
}

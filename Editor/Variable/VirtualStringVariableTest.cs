using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualStringVariableTest {

		[Test]
		public void CreateTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"test");
			Assert.AreEqual(_var.AsString, "test");
		}

		[Test]
		public void AsIntOutTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"123");
			Assert.AreEqual(_var.AsInt, 123);
		}

		[Test]
		public void AsFloatOutTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"123.132");
			Assert.AreEqual (_var.AsFloat, 123.132f);;
		}

		[Test]
		public void AsStringOutTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"abc");
			Assert.AreEqual(_var.AsString, "abc");
		}

		[Test]
		public void AsStringInTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"");
			try {
				_var.AsString = "abc";
				
			} catch (System.Exception) {
				Assert.Pass();
			}

			Assert.Fail();
		}


		[Test]
		public void AsBoolOutTest1()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"true");
			Assert.AreEqual(_var.AsBool, true);
		}


		[Test]
		public void AsBoolOutTest2()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"True");
			Assert.AreEqual (_var.AsBool, true);
		}



		[Test]
		public void AsBoolOutTest3()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"TrUe");
			Assert.AreEqual(_var.AsBool, true);
		}


		[Test]
		public void TypeTest()
		{

			VirtualStringVariable _var = new VirtualStringVariable (()=>"abcd");
			Assert.AreEqual(_var.Type, DataType.String);
		}
	}
}
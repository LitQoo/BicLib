using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace BicDB.Variable
{
	public class VirtualBoolVariableTest {

		[Test]
		public void CreateTest()
		{

			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var.AsBool, true);
		}

		[Test]
		public void AsIntOutTest()
		{

			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var.AsInt, 1);

			VirtualBoolVariable _var2 = new VirtualBoolVariable (()=>{return false;});
			Assert.AreEqual(_var2.AsInt, 0);
		}

		[Test]
		public void AsFloatOutTest()
		{

			VirtualBoolVariable _var1 = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var1.AsFloat, 1);

			VirtualBoolVariable _var2 = new VirtualBoolVariable (()=>{return false;});
			Assert.AreEqual(_var2.AsFloat, 0);
		}

		[Test]
		public void AsStringOutTest()
		{

			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var.AsString, "true");

			VirtualBoolVariable _var2 = new VirtualBoolVariable (()=>{return false;});
			Assert.AreEqual(_var2.AsString, "false");
		}

		[Test]
		public void AsBoolInTest()
		{

			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return false;});

			try {
				_var.AsBool = true;
			} catch (System.Exception) {
				Assert.Pass();
			}

			Assert.Fail();
		}

		[Test]
		public void AsBoolOutTest()
		{
			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return false;});
			Assert.AreEqual(_var.AsBool, false);

			VirtualBoolVariable _var2 = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var2.AsBool, true);
		}

		[Test]
		public void TypeTest()
		{

			VirtualBoolVariable _var = new VirtualBoolVariable (()=>{return true;});
			Assert.AreEqual(_var.Type, VariableType.Bool);
		}
	}
}
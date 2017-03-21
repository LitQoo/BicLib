using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class EncryptedIntVariableTest {

		[Test]
		public void CreateTest(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			Assert.AreEqual(_var.AsInt, 10);
		}

		[Test]
		public void AsIntTest(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			_var.AsInt = 20;

			Assert.AreEqual(_var.AsInt, 20);
		}

		[Test]
		public void AsStringTest(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			_var.AsString = "123";

			Assert.AreEqual(_var.AsInt, 123);
		}

		[Test]
		public void AsStringTest2(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			_var.AsString = "123";

			Assert.AreEqual(_var.AsString, "123");
		}

		[Test]
		public void LoadValue(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			string _json = "123";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var.AsString, "123");
		}
	}
}
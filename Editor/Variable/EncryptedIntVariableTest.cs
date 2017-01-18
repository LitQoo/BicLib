using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Variable;
using BicDB;

namespace BicDB.Variable
{
	public class EncryptedIntVariableTest {

		[Test]
		public void CreateTest(){
			EncryptedIntVariable _var = new EncryptedIntVariable(10);

			Assert.AreEqual(_var.AsInt, 10);
		}
	}
}
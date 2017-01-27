using System;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;
using NUnit.Framework;
using NSubstitute;

namespace BicDB.Storage
{
	public class ResourceStorageTest {
		[Test]
		public void GetInstanceTest(){
			var _storage = ResourceStorage.GetInstance();

			Assert.IsNotNull(_storage);
		}

		[Test]
		public void SaveTest(){
			Assert.Ignore ();
		}

		[Test]
		public void LoadTest(){
			Assert.Ignore ();
		}

		[Test]
		public void ReadTest(){
			Assert.Ignore ();
		}
	}


}

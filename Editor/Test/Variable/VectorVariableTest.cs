using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;
using BicUtil.Json;

namespace BicDB.Variable
{
	public class VectorVariableTest 
	{
		[Test]
		public void CreateTest(){
			VectorVariable _var = new VectorVariable (0, 0);
			Assert.AreEqual (_var.X.AsFloat, 0);
			Assert.AreEqual (_var.Y.AsFloat, 0);
		}

		[Test]
		public void AsVactorOutTest(){
			VectorVariable _var = new VectorVariable (99, -77.2f);
			Assert.AreEqual (_var.AsVector.x, 99);
			Assert.AreEqual (_var.AsVector.y, -77.2f);
		}

		[Test]
		public void createByStringTest(){
			VectorVariable _var = new VectorVariable (99, -100);
			string _json = "{\"x\" : 1, \"y\":-10}";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual (_var.X.AsFloat, 1);
			Assert.AreEqual (_var.Y.AsFloat, -10);
		}

		[Test]
		public void buildStringTest(){
			VectorVariable _var = new VectorVariable (99, -100);
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			_var.BuildFormattedString(_stringBuilder, JsonConvertor.GetInstance());

			Assert.AreEqual (_stringBuilder.ToString(), "{\"x\":99,\"y\":-100}");
		}
	}
}
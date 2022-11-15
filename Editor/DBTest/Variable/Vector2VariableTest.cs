using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using System;
using BicUtil.Json;

namespace BicDB.Variable
{
	public class Vector2VariableTest 
	{
		[Test]
		public void CreateTest(){
			Vector2Variable _var = new Vector2Variable (0, 0);
			Assert.AreEqual (_var.X, 0);
			Assert.AreEqual (_var.Y, 0);
		}

		[Test]
		public void AsVactorOutTest(){
			Vector2Variable _var = new Vector2Variable (99, -77.2f);
			Assert.AreEqual (_var.AsVector.x, 99);
			Assert.AreEqual (_var.AsVector.y, -77.2f);
		}

		[Test]
		public void createByStringTest(){
			Vector2Variable _var = new Vector2Variable (99, -100);
			string _json = "\"1.12, -10\"";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual (_var.X, 1.12f);
			Assert.AreEqual (_var.Y, -10);
		}

        [Test]
        public void createByStringTest2(){
            Vector2Variable _var = new Vector2Variable (99, -100);
            string _json = "\"1,-10.123\"";
            int _counter = 0;
            _var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

            Assert.AreEqual (_var.X, 1);
            Assert.AreEqual (_var.Y, -10.123f);
        }

        [Test]
        public void createByStringTest3(){
            Vector2Variable _var = new Vector2Variable (99, -100);
            string _json = "{\"y\":1,\"x\":-10.123}";
            int _counter = 0;
            _var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

            Assert.AreEqual (_var.Y, 1);
            Assert.AreEqual (_var.X, -10.123f);
        }

		[Test]
		public void buildStringTest(){
			Vector2Variable _var = new Vector2Variable (99.1f, -100);
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			_var.BuildFormattedString(_stringBuilder, JsonConvertor.GetInstance());

			Assert.AreEqual (_stringBuilder.ToString(), "\"99.1,-100\"");
		}
	}
}
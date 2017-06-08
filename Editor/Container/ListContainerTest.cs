using UnityEngine;
using System.Collections;
using NUnit.Framework;
using BicDB.Container;
using BicDB;
using BicDB.Utility;
using System.Linq;
using BicDB.Variable;

namespace BicDB.Container
{
	public class ListTest
	{
		[Test]
		public void CreateStringTest1()
		{

			ListContainer _var = new ListContainer ();

			string _json = "[\"test\", \"123\", \"456\"]";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsVariable.AsString, "test");
			Assert.AreEqual(_var[1].AsVariable.AsString, "123");
			Assert.AreEqual(_var[2].AsVariable.AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			ListContainer _var = new ListContainer ();
			string _json = "[123,456,789]";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsVariable.AsInt, 123);
			Assert.AreEqual(_var[1].AsVariable.AsInt, 456);
			Assert.AreEqual(_var[2].AsVariable.AsInt, 789);
		}

		[Test]
		public void GetSizeTest()
		{
			ListContainer _var = new ListContainer ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("3"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);
		}

		[Test]
		public void AddTest()
		{
			ListContainer _var = new ListContainer ();
			_var.Add(new StringVariable("1"));

			Assert.AreEqual(_var[0].AsVariable.AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			ListContainer _var = new ListContainer ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("2"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);

			_var.RemoveAt(0);
			Assert.AreEqual(_var.Count, 2);
			Assert.AreEqual(_var[0].AsVariable.AsString, "2");
		}

		[Test]
		public void OnChangedElementNotifyTest(){
			ListContainer _var1 = new ListContainer ();
			int _count = 0;
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("1"));

			_var1.OnChangedElementActions[0] += (int _index, IDataBase _variable) => {
				if(_variable.AsVariable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[0] += (int _index, IDataBase _variable) => {
				if(_variable.AsVariable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[1] += (int _index, IDataBase _variable) => {
				_count = -1;
			};

			_var1[0] = new StringVariable("test");

			Assert.AreEqual(_count, 2);
		}

		[Test]
		public void LinqTest1(){
			ListContainer _var1 = new ListContainer ();
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("2"));

			var _var2 =  _var1.FirstOrDefault(_row=>_row.AsVariable.AsString=="2");

			Assert.AreNotEqual(_var2, null);

		}

		[Test]
		public void LinqTest2(){
			ListContainer _var1 = new ListContainer ();
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("2"));

			var _var2 =  _var1.FirstOrDefault(_row=>_row.AsVariable.AsString=="3");

			Assert.AreEqual(_var2, null);

		}
	}
}


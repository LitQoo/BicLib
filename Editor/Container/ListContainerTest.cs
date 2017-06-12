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

			ListContainer<IVariable> _var = new ListContainer<IVariable> ();

			string _json = "[\"test\", \"123\", \"456\"]";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsString, "test");
			Assert.AreEqual(_var[1].AsString, "123");
			Assert.AreEqual(_var[2].AsString, "456");
		}

		[Test]
		public void CreateIntTest()
		{

			ListContainer<IVariable> _var = new ListContainer<IVariable> ();
			string _json = "[123,456,789]";
			int _counter = 0;
			_var.BuildVariable(ref _json, ref _counter, JsonConvertor.GetInstance());

			Assert.AreEqual(_var[0].AsInt, 123);
			Assert.AreEqual(_var[1].AsInt, 456);
			Assert.AreEqual(_var[2].AsInt, 789);
		}

		[Test]
		public void GetSizeTest()
		{
			ListContainer<IVariable> _var = new ListContainer<IVariable> ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("3"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);
		}

		[Test]
		public void AddTest()
		{
			ListContainer<IVariable> _var = new ListContainer<IVariable> ();
			_var.Add(new StringVariable("1"));

			Assert.AreEqual(_var[0].AsString, "1");
		}

		[Test]
		public void RemoveAtTest()
		{
			ListContainer<IVariable> _var = new ListContainer<IVariable> ();
			_var.Add(new StringVariable("1"));
			_var.Add(new StringVariable("2"));
			_var.Add(new StringVariable("3"));

			Assert.AreEqual(_var.Count, 3);

			_var.RemoveAt(0);
			Assert.AreEqual(_var.Count, 2);
			Assert.AreEqual(_var[0].AsString, "2");
		}

		[Test]
		public void OnChangedElementNotifyTest(){
			ListContainer<IVariable> _var1 = new ListContainer<IVariable> ();
			int _count = 0;
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("1"));

			_var1.OnChangedElementActions[0] += (int _index, IVariable _variable) => {
				if(_variable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[0] += (int _index, IVariable _variable) => {
				if(_variable.AsString == "test"){
					_count++;
				}
			};

			_var1.OnChangedElementActions[1] += (int _index, IVariable _variable) => {
				_count = -1;
			};

			_var1[0] = new StringVariable("test");

			Assert.AreEqual(_count, 2);
		}

		[Test]
		public void LinqTest1(){
			ListContainer<IVariable> _var1 = new ListContainer<IVariable> ();
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("2"));

			var _var2 =  _var1.FirstOrDefault(_row=>_row.AsString=="2");

			Assert.AreNotEqual(_var2, null);

		}

		[Test]
		public void LinqTest2(){
			ListContainer<IVariable> _var1 = new ListContainer<IVariable> ();
			_var1.Add(new StringVariable("1"));
			_var1.Add(new StringVariable("2"));

			var _var2 =  _var1.FirstOrDefault(_row=>_row.AsString=="3");

			Assert.AreEqual(_var2, null);

		}
	}
}


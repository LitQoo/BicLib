using System;
using System.Collections.Generic;
using BicDB.Variable;
using NUnit.Framework;

namespace BicDB
{
	public class ModelTest {
		[Test]
		public void AddManagedColumnTest(){
			var _model = new ModelVariable ();
			var _var = new IntVariable (0);
			_model.AddManagedColumn ("key", _var);

			Assert.AreEqual (_model["key"], _var);
		}

		[Test]
		public void NotifyTest(){
			var _model = new ModelVariable ();
			string _msg = "msg";
			_model.OnChangedValueActions += (IVariable _model2, string _message) => {
				if(_model2 == _model && _message == _msg){
					Assert.Pass();
				}
			};

			_model.NotifyChanged (_msg);

			Assert.Fail ();
		
		}

		[Test]
		public void IndexerTest(){
			int _testValue = 123;
			var _model = new ModelVariable ();
			var _var = new IntVariable (_testValue);
			_model.AddManagedColumn ("key", _var);

			Assert.AreEqual(_model["key"].AsInt, _testValue); 
		}

		[Test]
		public void GetFieldNamesTest(){
			var _model = new ModelVariable ();
			var _var1 = new IntVariable (0);
			var _var2 = new IntVariable (0);
			_model.AddManagedColumn ("key1", _var1);
			_model.AddManagedColumn ("key2", _var2);

			var _fieldList = _model.GetColumnNameList();

			Assert.AreEqual(_fieldList.Count, 2);
			Assert.AreEqual(_fieldList[0], "key1");
			Assert.AreEqual(_fieldList[1], "key2");
		}
	}
}
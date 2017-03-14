using System;
using System.Collections.Generic;
using BicDB.Variable;
using NUnit.Framework;
using BicDB.Variable;
using BicDB.Container;

namespace BicDB
{
	public class ModelTest {
		[Test]
		public void AddManagedColumnTest(){
			var _model = new ModelContainer ();
			var _var = new IntVariable (0);
			_model.AddManagedColumn ("key", _var);

			Assert.AreEqual (_model["key"], _var);
		}

		[Test]
		public void NotifyTest(){
			var _model = new ModelContainer ();
			string _msg = "msg";
			_model.OnChangedValueActions += (IModelContainer _model2, string _message) => {
				if(_model2 == _model && _message == _msg){
					Assert.Pass();
				}
			};

			_model.NotifyChanged (_msg);

			Assert.Fail ();
		
		}

		[Test]
		public void IndexerTest(){
			var _model = new ModelContainer ();
			var _var = new IntVariable (123);
			_model.AddManagedColumn ("key", _var);

			Assert.AreEqual((_model["key"] as IntVariable).AsInt, 123); 
		}

		[Test]
		public void GetFieldNamesTest(){
			var _model = new ModelContainer ();
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
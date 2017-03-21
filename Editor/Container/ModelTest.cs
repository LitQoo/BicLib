using System;
using System.Collections.Generic;
using BicDB.Container;
using NUnit.Framework;
using BicDB.Variable;
using System.Linq;
using UnityEditorInternal.VersionControl;

namespace BicDB.Container
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

			var _fieldList = _model.Keys.ToArray();

			Assert.AreEqual(_fieldList.Length, 2);
			Assert.AreEqual(_fieldList[0], "key1");
			Assert.AreEqual(_fieldList[1], "key2");
		}

		[Test]
		public void CopyByTest1(){
			var _model = new ModelContainer ();
			var _var1 = new IntVariable (1);
			var _var2 = new IntVariable (2);
			_model.AddManagedColumn ("key1", _var1);
			_model.AddManagedColumn ("key2", _var2);

			var _model2 = new ModelContainer ();
			var _var3 = new IntVariable (3);
			var _var4 = new IntVariable (4);
			_model2.AddManagedColumn ("key1", _var3);
			_model2.AddManagedColumn ("key3", _var4);

			_model.CopyBy(_model2);

			Assert.AreEqual((_model["key1"] as IntVariable).AsInt, 3);
			Assert.AreEqual((_model["key2"] as IntVariable).AsInt, 2);
			Assert.AreEqual((_model["key3"] as IntVariable).AsInt, 4);
		}

		[Test]
		public void CopyByTest2(){
			var _model = new ModelContainer ();
			var _var1 = new ListContainer<IntVariable>();
			_var1.Add(new IntVariable(11));
			var _var2 = new ListContainer<IntVariable>();
			_var2.Add(new IntVariable(22));
			_model.AddManagedColumn ("key1", _var1);
			_model.AddManagedColumn ("key2", _var2);

			var _model2 = new ModelContainer ();
			var _var3 = new ListContainer<IntVariable>();
			_var3.Add(new IntVariable(33));
			var _var4 = new ListContainer<IntVariable>();
			_var4.Add(new IntVariable(44));
			_model2.AddManagedColumn ("key1", _var3);
			_model2.AddManagedColumn ("key3", _var4);

			_model.CopyBy(_model2);

			Assert.AreEqual((_model["key1"] as IListContainer<IntVariable>)[0].AsInt, 33);
			Assert.AreEqual((_model["key2"] as IListContainer<IntVariable>)[0].AsInt, 22);
			Assert.AreEqual((_model["key3"] as IListContainer<IntVariable>)[0].AsInt, 44);
		}
	}
}
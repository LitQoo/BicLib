using System;
using System.Collections.Generic;
using BicDB.Variable;
using NUnit.Framework;

namespace BicDB
{
	public class ModelTest {
		[Test]
		public void AddManagedColumnTest(){
			var _model = new Model ();
			var _var = new IntVariable (0);
			_model.AddManagedColumn ("key", _var);

			Assert.AreEqual (_model.Columns ["key"], _var);
		}

		[Test]
		public void NotifyTest(){
			var _model = new Model ();
			string _msg = "msg";
			_model.OnNotifyActions += (IModel _model2, string _message) => {
				if(_model2 == _model && _message == _msg){
					Assert.Pass();
				}
			};

			_model.Notify (_msg);

			Assert.Fail ();
		
		}
	}
}
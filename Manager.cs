using System;
using System.Collections.Generic;

namespace BigjamLibrary.BicDB
{
	public static class Manager{

		static private List<object> tables = new List<object> ();

		static public ITable<T> GetTable<T> (){

			foreach (var _item in tables) {
				if (_item is ITable<T>) {
					return _item as ITable<T>;
					//var v = (Table<IModel>)Convert.ChangeType(new Table<User>("User","no"), typeof(Table<IModel>));
				}
			}

			return null;
		}

		static public void AddTable<T>(ITable<T> _table){
			tables.Add (_table);
		}

		static public ITable<T> CreateTable<T>(string _name, string _primaryKeyName) where T : class, IRow{
			var _table = new Table<T>(_name, _primaryKeyName);
			Manager.AddTable<T>(_table);
			return _table;
		}
	}
}


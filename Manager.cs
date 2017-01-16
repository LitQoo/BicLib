using System;
using System.Collections.Generic;

namespace BicDB
{
	public static class Manager{

		static private List<object> tables = new List<object> ();

		static public ITable<T> GetTable<T> (string _tableName = ""){

			foreach (var _item in tables) {
				if (_item is ITable<T>) {
					var _table = _item as ITable<T>;
					if (_table.Name == _tableName || string.IsNullOrEmpty (_tableName)) {
						return _table;
					}
				}
			}

			return null;
		}

		static public void AddTable<T>(ITable<T> _table){
			tables.Add (_table);
		}

		static public ITable<T> CreateTable<T>(string _name, string _primaryKeyName) where T : class, IModel, new() {
			var _table = new Table<T>(_name, _primaryKeyName);
			Manager.AddTable<T>(_table);
			return _table;
		}

		static public void ClearTables(){
			tables.Clear();
		}
	}
}


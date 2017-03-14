#define GPGSRanking
using System;
using System.Collections.Generic;
using System.Linq;

namespace BicDB
{
	public static class Manager{

		static private List<object> tables = new List<object> ();

		static public ITable<T> GetTable<T> (string _tableName = "") where T : IModelVariable, new(){

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

		static public void AddTable<T>(ITable<T> _table) where T : IModelVariable, new(){
			if (!tables.Contains (_table)) {
				tables.Add (_table);			
			} else {
				throw new SystemException (_table.Name + " table is already added");
			}
		}

		static public ITable<T> CreateTable<T>(string _name) where T : class, IModelVariable, new() {
			var _table = new Table<T>(_name);
			Manager.AddTable<T>(_table);
			return _table;
		}

		static public ITable<T> GetOrCreateTable<T>(string _name) where T : class, IModelVariable, new() {

			var _table = GetTable<T>(_name);

			if (_table != null) {
				return _table;
			} else {
				return CreateTable<T>(_name);
			}
		}

		static public void ClearTables(){
			tables.Clear();
		}
	}
}


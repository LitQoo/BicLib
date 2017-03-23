#define GPGSRanking
using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;

namespace BicDB
{
	public static class Manager{

		static private List<object> tables = new List<object> ();

		static public ITableContainer<T> GetTable<T> (string _tableName = "") where T : IRecordContainer, new(){

			foreach (var _item in tables) {
				if (_item is ITableContainer<T>) {
					var _table = _item as ITableContainer<T>;
					if (_table.Name == _tableName || string.IsNullOrEmpty (_tableName)) {
						return _table;
					}
				}
			}

			return null;
		}

		static public void AddTable<T>(ITableContainer<T> _table) where T : IRecordContainer, new(){
			if (!tables.Contains (_table)) {
				tables.Add (_table);			
			} else {
				throw new SystemException (_table.Name + " table is already added");
			}
		}

		static public ITableContainer<T> CreateTable<T>(string _name) where T : class, IRecordContainer, new() {
			var _table = new TableContainer<T>(_name);
			Manager.AddTable<T>(_table);
			return _table;
		}

		static public ITableContainer<T> GetOrCreateTable<T>(string _name) where T : class, IRecordContainer, new() {

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


using System.Collections;
using UnityEngine;
using System.IO;
using BicDB.Storage;
using System;

namespace BicDB.Storage{

	public class ResourceStorage : IStorage {
		static public string FILE_NAME_PREFIX = "bdb_"; 

		#region static
		static private IStorage instance = null;
		static public IStorage GetInstance(){
			if (instance == null) {
				instance = new FileStorage();
			}

			return instance;
		}
		#endregion

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			if (_callback != null) {
				_callback(false);
			}
		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			string _data = Read(getFileName(_table.Name));

			if (!string.IsNullOrEmpty(_data)) {
				JsonConvertor.ConvertJsonStringToTable(_data , _table);
			}

			if (_callback != null) {
				_callback(true);
			}
		}

		private string getFileName(string _tableName){
			return FILE_NAME_PREFIX + _tableName;
		}
		#endregion

		#region ResourceControl
		public string Read(string _fileName){
			#if UNITY_EDITOR
			string tPath = Application.dataPath + "/Resources/BicDB" + _fileName + ".json";
			if (File.Exists(tPath))
			{
				FileStream tFile = new FileStream (tPath, FileMode.Open, FileAccess.Read);
				StreamReader tStream = new StreamReader(tFile);
				string tStr = null;
				tStr = tStream.ReadLine ();
				tStream.Close();
				tFile.Close();
				return tStr;
			}
			else
			{
				Debug.Log("not found json file Resources/BicDB/" + _fileName + ".json");
				return null;
			}
			#else
			TextAsset tText = Resources.Load<TextAsset>("BicDB/"+_fileName);
			return tText.text;
			#endif 
		}
		#endregion
	}
}
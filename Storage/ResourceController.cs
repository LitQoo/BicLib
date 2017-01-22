using System.Collections;
using UnityEngine;
using System.IO;
using BicDB.Storage;

namespace BicDB.stroage{
	public class ResourceController : IFileController {
		#region Singleton
		static public ResourceController Instance = null;
		static public ResourceController GetInstance(){
			if (Instance == null) {
				Instance = new ResourceController();
			}

			return Instance;
		}
		#endregion

		public void Write(string _data, string _fileName){

		}

		public string Read(string _fileName){
			#if UNITY_EDITOR
			string tPath = Application.dataPath + "/Resources/" + _fileName + ".json";
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
				return null;
			}
			#else
			TextAsset tText = Resources.Load<TextAsset>(fileName);
			return tText.text;
			#endif 
		}
	}
}
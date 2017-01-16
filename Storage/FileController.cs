using System;
using UnityEngine;


namespace BicDB.Storage
{

	public interface IFileController{
		void Write(string _data, string _fileName);
		string Read(string _fileName);
	}

	public class FileController : IFileController
	{
		#region Singleton
		static public FileController Instance = null;
		static public FileController GetInstance(){
			if (Instance == null) {
				Instance = new FileController();
			}

			return Instance;
		}
		#endregion

		public void Write(string _data, string _fileName){
			#if !WEB_BUILD

			string _path = Application.persistentDataPath + "/" + _fileName;
			System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file);
			_streamWriter.WriteLine(_data);
			_streamWriter.Close();
			_file.Close();


			#else

			throw new System.Exception ("webbuild do not save to file");

			#endif
		}

		public string Read(string _fileName){
			#if !WEB_BUILD
			string _path = Application.persistentDataPath + "/" + _fileName;

			if (System.IO.File.Exists(_path))
			{
				System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.StreamReader _stream = new System.IO.StreamReader(_file);
				string _data = null;
				_data = _stream.ReadLine ();
				_stream.Close();
				_file.Close();
				return _data;
			}
			else
			{
				return null;
			}
			#else
			return null;
			#endif 
		}

	}
}


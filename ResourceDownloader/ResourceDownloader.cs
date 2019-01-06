#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace BicUtil.ResourceDownloader
{
	public class ResourceDownloader : MonoBehaviour {
		private struct DownloadParam
		{
			public Action<ResultParam> Callback;
			public string Url;
			public string SavePath;

			public DownloadParam(Action<ResultParam> _callback, string _url, string _savePath = ""){
				Callback = _callback;
				Url = _url;
				SavePath = _savePath;
			}
		}

		public struct ResultParam
		{
			public bool IsSuccess;
			public string Url;
			public string SavePath;
			public Texture2D Texture;

			public ResultParam(bool _isSuccess, string _url, Texture2D _texture, string _savePath = ""){
				Url = _url;
				SavePath = _savePath;
				Texture = _texture;
				IsSuccess = _isSuccess;
			}
		}

		#region singleton
		private static ResourceDownloader instance = null;  
		private static GameObject container;  
		public static ResourceDownloader GetInstance()  
		{  
			if(instance == null)  
			{  
				container = new GameObject();  
				container.name = "BicDBResourceDownloader";  
				instance = container.AddComponent(typeof(ResourceDownloader)) as ResourceDownloader;  
				DontDestroyOnLoad(container);
			}  

			return instance;  
		}  
		#endregion

		public void DownlaodImage(string _url, Action<ResultParam> _callback){
			StartCoroutine(getImageFromWWW(new DownloadParam(_callback, _url)));
		}

		public void DownlaodAndSaveImage(string _url, string _savePath, Action<ResultParam> _callback){
			Action<ResultParam> _callbackWrap = ((ResultParam _result) => {

				byte[] _bytes = _result.Texture.EncodeToPNG();

				#if !WEB_BUILD
				File.WriteAllBytes(Application.persistentDataPath + "/" + _result.SavePath, _bytes); 
				#else
				throw new System.Exception ("webbuild do not save to file");
				#endif

				if(_callback != null){
					_callback(_result);
				}
			});

			StartCoroutine(getImageFromWWW(new DownloadParam(_callbackWrap, _url, _savePath)));
		}

		private IEnumerator getImageFromWWW(DownloadParam _param) {

			Texture2D _texture = new Texture2D(1,1);
			WWW www = new WWW(_param.Url);
			yield return www;

			bool _isSuccess = false;

			if (www.error != null) {
				_isSuccess = false;
			} else {
				_isSuccess = true;
				www.LoadImageIntoTexture (_texture);
			}

			if (_param.Callback != null) {
				_param.Callback (new ResultParam(_isSuccess, _param.Url, _texture, _param.SavePath));
			}
		}
	}
}
#endif
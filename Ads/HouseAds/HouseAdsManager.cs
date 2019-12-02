#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using BicDB;
using BicDB.Variable;
using System;
using BicDB.Storage;
using System.Linq;
using BicDB.Container;
using BicUtil.ResourceDownloader;
using BicUtil.Tween;
using BicUtil.Ads;
using BicUtil.SingletonBase;
using System.Collections.Generic;

namespace BicUtil.Ads
{
	public class HouseAdsManager : MonoBehaviourHardBase<HouseAdsManager> , IAdsPlatform{
		
		#region InstantData
		private Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
		private string dataUrl = string.Empty;
		private bool isReadyByRandom = false;
		#endregion
		
		#region Init
		private void Awake(){
			setDatabase();
		}
		#endregion

		#region Database
		public ITableContainer<HouseAdsModel> HouseAdsTable;
		public ITableContainer<HouseAdsResourceModel> HouseAdsResourceTable;
		private void setDatabase(){
			HouseAdsTable = BicDB.Manager.GetOrCreateTable<HouseAdsModel> ("houseAdsTable");
			
			if(dataUrl == string.Empty){
				dataUrl = "https://allapp-7c9c4.firebaseio.com/localAds/ver1/" + Application.identifier.Replace(".", "") + "/" + Application.platform.ToString() + ".json";
			}

			var _syncStorage = SyncStorage.GetInstance();
			HouseAdsTable.Header[SyncStorage.LOAD_URL_KEY] = new StringVariable(dataUrl);
			HouseAdsTable.Header[SyncStorage.ENCRYPT_KEY] = new StringVariable("houseads");
			HouseAdsTable.PrimaryKey = "id";
			HouseAdsTable.SetStorage (_syncStorage);

			HouseAdsResourceTable = BicDB.Manager.GetOrCreateTable<HouseAdsResourceModel> ("houseAdsResource");
			HouseAdsResourceTable.SetStorage (FileStorage.GetInstance());
			HouseAdsResourceTable.Load (null, new FileStorageParameter("houseads"));

			loadByServer();
		}
		#endregion

		private float reloadInterval = 2f;
		private void loadByServer(){
			HouseAdsTable.Load (onLoadedData, new SyncStorageParameter(SyncStorageParameter.SyncMode.All, SyncStorageParameter.SyncTarget.All));
		}

		private void onLoadedData(Result _result){
			if (_result.Code == (int)SyncStorage.ResultCode.Success) {
				HouseAdsTable.Save ();
				downloadResource ();
				updateIsReadyByRandom();
			} else {
				BicTween.Delay (reloadInterval).SubscribeComplete(loadByServer);
				reloadInterval *= 2f;
			}
		}

        private void updateIsReadyByRandom()
        {
			if(HouseAdsTable.Property.ContainsKey("ViewRate") == true){
				var _viewRate = HouseAdsTable.Property["ViewRate"].AsVariable.AsInt;
				var _rand = UnityEngine.Random.Range(0, 100);
				if(_rand < _viewRate){
					isReadyByRandom = true;
					return;
				}
			}
			
			isReadyByRandom = false;
        }

        private void downloadResource(){
			for (int i = 0; i < HouseAdsTable.Count; i++) {
				for (int j = 0; j < HouseAdsTable [i].Images.Count; j++) {
					string _url = HouseAdsTable [i].Images [j].AsString;
					var _item = HouseAdsResourceTable.FirstOrDefault (_row => _row.Url.AsString == _url);
					if (_item == null) {
						ResourceDownloader.ResourceDownloader.GetInstance ().DownlaodAndSaveImage (_url, Guid.NewGuid ().ToString () + ".png", onFinishedDownloadResource);
					}
				}
			}
		}

		void onFinishedDownloadResource (ResourceDownloader.ResourceDownloader.ResultParam _param)
		{
			if (_param.IsSuccess) {
				saveResourceInfo (_param.Url, _param.SavePath);
			}
		}

		void saveResourceInfo(string _url, string _filePath){
			var _row = new HouseAdsResourceModel ();
			_row.FilePath.AsString = _filePath;
			_row.Url.AsString = _url;
			HouseAdsResourceTable.Add (_row);
			HouseAdsResourceTable.Save ();
		}
		
		public void SetDataUrl(string _url){
			this.dataUrl = _url;
		}

		public void SetAdsSetting(string _platformId, object _adsPlacement, string _prefabPath){
			adsData[_adsPlacement] = new AdsPlatformInfo(_platformId, _adsPlacement);
			adsData[_adsPlacement].Data = _prefabPath;
		}

        public void LoadInterstitial(object _adsPlacement)
        {
			
		}

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            return isReady(_adsPlacement);
        }

		private bool isReady(object _adsPlacement){
			if(adsData.ContainsKey(_adsPlacement) == false){
				return false;
			}

			if(isReadyByRandom == false){
				updateIsReadyByRandom();
				return false;
			}

			var _count = getAdsList(_adsPlacement).Count();

			if(_count > 0){
				return true;
			}else{
				return false;
			}
		}

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
			showInterstitial(_adsPlacement, _callback, 5);
			updateIsReadyByRandom();
        }

		private void showInterstitial(object _adsPlacement, Action<AdsResult> _callback, int _time){
			if(adsData.ContainsKey(_adsPlacement) == true){
				string _prefabPath = adsData[_adsPlacement].Data as string;

				var _ads = getHouseAds(_adsPlacement);

				if(_ads != null){
					var _interstitial = Instantiate(Resources.Load<HouseInterstitialController>(_prefabPath));
					_interstitial.OnClose += ()=>{
						_callback(AdsResult.Finished);
					};
					
					var _canvasList = Resources.FindObjectsOfTypeAll(typeof(Canvas));
					var _canvas = (_canvasList[0] as Canvas);
					_interstitial.transform.SetParent(_canvas.transform);
					_interstitial.transform.localScale = new Vector2(1f, 1f);
					_interstitial.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
					_interstitial.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
					_interstitial.Load(_adsPlacement, _ads, _time);
					_interstitial.Show();

				}else{
					_callback(AdsResult.Failed);
				}
			}else{
				_callback(AdsResult.Failed);
			}
		}

        public void LoadRewardBased(object _adsPlacement)
        {
			
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            return isReady(_adsPlacement);
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
			showInterstitial(_adsPlacement, _callback, 20);
			updateIsReadyByRandom();
        }


		public bool IsReadyBanner(object _adsPlacement){
			return adsData.ContainsKey(_adsPlacement);
		}

		public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
		{
			if(adsData.ContainsKey(_adsPlacement) == true){
				string _prefabPath = adsData[_adsPlacement].Data as string;
				var _ads = getHouseAds(_adsPlacement);

				if(_ads != null){
					var _banner = Instantiate(Resources.Load<HouseBannerController>(_prefabPath));
					_banner.Load(adsData[_adsPlacement].AdsPlacement.ToString(), _ads);
					_onLoadBannerAction(_banner);
					updateIsReadyByRandom();
					return _banner;
				}
			}

			return null;
		}

		private HouseAdsModel getHouseAds(object _adsPlacement){
			if(adsData.ContainsKey(_adsPlacement) == false){
				return null;
			}

			var _data = adsData[_adsPlacement];
			var _adsList = getAdsList(_adsPlacement);
			
			if(_adsList.Count() <= 0){
				return null;
			}
			
			int _totalWeight = _adsList.Sum(_row => _row.ViewWeight.AsInt);
			int _selectWeight = UnityEngine.Random.Range(0, _totalWeight);
			int _sumWeight = 0;

			foreach(var _ads in _adsList){
				_sumWeight += _ads.ViewWeight.AsInt;
				if (_sumWeight >= _selectWeight) {
					return _ads;
				}
			}

			return _adsList.ElementAt(0);
		}

        private List<HouseAdsModel> getAdsList(object _adsPlacement)
        {
			var _id = adsData[_adsPlacement].PlatformId;
            return HouseAdsManager.Instance.HouseAdsTable.Where(_row => _row.IsLoaded() && (_row.AdsId.AsString == "" || _row.AdsId.AsString.Contains(_id) == true)).ToList();
        }
    }
}
#endif
using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Analytics;

namespace BicUtil.Ads
{
	public class HouseBannerController : MonoBehaviour, IAdsBanner {

		#region LinkingObject
		[SerializeField]
		private UnityEngine.UI.Image bannerImages;
		[SerializeField]
		private UnityEngine.UI.Text mentText;
		#endregion

		#region InstantData
		private List<Sprite> bannerSprites = new List<Sprite>();
		private HouseAdsModel model = null;
		private string campaignName;
		#endregion

		#region IAdsBanner
		public void Load(string _campaignName, HouseAdsModel _model){
			model = _model;
			campaignName = _campaignName;
			bannerSprites.Clear();

			var _paths = model.GetImagesPath();
			for (int i = 0; i < _paths.Count; i++) {
				var _texture = new Texture2D(1, 1);
				_texture.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/" + _paths[i]));
				bannerSprites.Add(Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0, 0)));
			}
		}

		public bool IsReady(){
			return model != null;
		}

		public void Show()
        {
            this.gameObject.SetActive(true);
			StartCoroutine(changeBanner());
        }

        public void Hide()
        {
			StopCoroutine(changeBanner());
            this.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            AdsManager.Instance.RemoveBanner(this);
			Destroy(this.gameObject);
        }

		private void OnDestroy() {
			AdsManager.Instance.RemoveBanner(this);
		}

        public void SetPosition(Transform _parent, Vector2 _position)
        {
            this.transform.SetParent(_parent);
			this.transform.localPosition = _position;
			this.transform.localScale = new Vector2(1f, 1f);
        }
		#endregion

		#region Logic
		public void MoveToStore(){
			string _appID = model.AppId.AsString;
			string _campainName = campaignName;
			
			Analytics.CustomEvent("clickHouseAds", new Dictionary<string, object>
			{
				{ "type", "houseAds"  },
				{ "id", _appID},
				{ "campaign", campaignName}
			});

			if(model.Url.AsString != ""){
				Application.OpenURL(model.Url.AsString);
			}else{
				#if UNITY_EDITOR
				Application.OpenURL("https://play.google.com/store/apps/details?id="+_appID);
				#elif UNITY_IOS
				Application.OpenURL ("itms-apps://itunes.apple.com/app/id" + _appID);
				#elif UNITY_ANDROID
				Application.OpenURL("market://details?id="+_appID+"&referrer=utm_source%3Dbigjam%26utm_campaign%3D"+_campainName);
				#else
				Application.OpenURL("market://details?id="+_appID+"&referrer=utm_source%3Dbigjam%26utm_campaign%3D"+_campainName);
				#endif
			}
		}

		public string GetAppId(){
			return model.AppId.AsString;
		}
		#endregion

		#region Animation
		private int spriteCounter = 0;
		private int mentCounter = 0;
		private float mentIntervalCounter = 0;

		private IEnumerator changeBanner(){
			while (true) {
				changeSprite ();
				changeMent ();
				yield return new WaitForSeconds(model.BannerInterval.AsFloat);
			}
		}

		private void changeSprite(){
			bannerImages.sprite = bannerSprites[spriteCounter];
			bannerImages.SetNativeSize();
			
			spriteCounter++;
			if (spriteCounter >= bannerSprites.Count) {
				spriteCounter = 0;
			}
		}

        private void changeMent(){

			if (mentText != null && model.Ment.Count > 0) {
				mentText.text = model.Ment [mentCounter].AsString;
			}

			if (mentIntervalCounter > model.MentInterval.AsFloat) {
				mentCounter++;
				if (mentCounter >= model.Ment.Count) {
					mentCounter = 0;
				}
				mentIntervalCounter = 0;
			}

			mentIntervalCounter += model.BannerInterval.AsFloat;
		}
		#endregion        
    }
}
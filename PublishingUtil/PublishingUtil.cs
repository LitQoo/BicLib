using System.Collections;
using System.Collections.Generic;
using BicDB.Core;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.PublishingUtil{
	static public class PublishingUtil {
		static public void OpenMoreGames(){
			#if UNITY_ANDROID
			Application.OpenURL("market://search?q=pub:BIGJAM");
			#elif UNITY_IOS
			Application.OpenURL("https://appstore.com/litqooinc");
			#endif			
		}

		static public void OpenReview(string _androidAppId, string _iosAppId){
			SaveWriteReview();
			OpenStore(_androidAppId, _iosAppId, "review");
		}

		static public void OpenStore(string _androidAppId, string _iosAppId, string _utm){
			#if UNITY_ANDROID
			Application.OpenURL("market://details?id="+_androidAppId+"&referrer=utm_source%3Dbigjam%26utm_campaign%3D" + _utm);
			#elif UNITY_IOS
			Application.OpenURL("itms-apps://itunes.apple.com/app/id" + _iosAppId);
			#endif	
		}

		static public void OpenFacebookPage(){
			Application.OpenURL("https://www.facebook.com/bigjamgames/");
		}

		static public void SaveReviewRequest(){
			saveBoolValue("isRequestReview", true);
		}

		static public void SaveWriteReview(){
			saveBoolValue("isWriteReview", true);
		}

		static private void saveBoolValue(string _propertyName, bool _value){
			if(!TableService.TableInfo.Property.ContainsKey(_propertyName)){
				TableService.TableInfo.Property.Add(_propertyName, new BoolVariable(_value));
			}else{
				TableService.TableInfo.Property[_propertyName].AsVariable.AsBool = _value;
			}

			TableService.TableInfo.Save();
		}

		static public bool IsRequestedReview{
			get{
				return getBoolValue("isRequestReview");
			}
		}

		static public bool IsWriteReview{
			get{
				return getBoolValue("isWriteReview");
			}
		}

		static private bool getBoolValue(string _propertyName){
			if(!TableService.TableInfo.Property.ContainsKey(_propertyName)){
				return false;
			}else{
				return TableService.TableInfo.Property[_propertyName].AsVariable.AsBool;
			}
		}
	}
}
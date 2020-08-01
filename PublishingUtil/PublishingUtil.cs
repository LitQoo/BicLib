using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using BicDB.Core;
using BicDB.Variable;
using UnityEngine;
using UnityEngine.iOS;

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

			
			#if UNITY_IOS
			var _isOpen = Device.RequestStoreReview();
			if(_isOpen == false){
				OpenStore(_androidAppId, _iosAppId, "review");	
			}
			#else
			OpenStore(_androidAppId, _iosAppId, "review");
			#endif
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
			var _property = TableService.GetProperty(_propertyName, new BoolVariable(_value));
			_property.AsBool = _value;
			TableService.Save();
		}

		static public bool IsRequestedReview{
			get{
				return getBoolValue("isRequestReview");
			}
		}

		static public bool IsWriteReview{
			get{
				#if UNITY_WEBGL
				return true;
				#else
				return getBoolValue("isWriteReview");
				#endif
			}
		}

		static private bool getBoolValue(string _propertyName){
			var _property = TableService.GetProperty(_propertyName, null);
			
			if(_property == null){
				return false;
			}else{
				return _property.AsBool;
			}
		}

		static public bool IsReviewTime(int _trueCount, bool _countingReview = true){
			if(IsWriteReview == true){
				return false;
			}

			if(NotRequestReviewToday == true){
				return false;
			}

			var _reviewCount = TableService.GetProperty("reviewCount", new IntVariable(1));
			var _result = false;

			if(_reviewCount.AsInt % _trueCount == 0){
				_result = true;
			}

			if(_countingReview == true){
				_reviewCount.AsInt++;
				TableService.Save();
			}

			return _result;
		}

		static public bool NotRequestReviewToday = false;

		static public int ReviewCount{
			get{
				return TableService.GetProperty("reviewCount", new IntVariable(0)).AsInt;
			}
		}

		#region GDPR
		private const string EU_QUERY_URL = "http://adservice.google.com/getconfig/pubvendors";

		public static bool ShouldOpenGDPR{
			get{
				if(IsGDPRArea == false){
					return false;
				}

				if(isAgreedGDPR.AsBool == true){
					return false;
				}

				return true;
			}
		}

		private static IVariable isAgreedGDPR = null;
		public static bool IsAgreedGDPR{
			get{
				if(isAgreedGDPR == null){
					isAgreedGDPR = TableService.GetProperty("isAgreedGDPR", new BoolVariable(false));
				}

				return isAgreedGDPR.AsBool;
			}

			set{
				if(isAgreedGDPR == null){
					isAgreedGDPR = TableService.GetProperty("isAgreedGDPR", new BoolVariable(false));
				}

				isAgreedGDPR.AsBool = value;
				TableService.Save();
			}
		}

		private static IVariable isGDPRArea = null;
		public static bool IsGDPRArea{
			get{
				if(isGDPRArea == null){
					isGDPRArea = TableService.GetProperty("isGDPRArea", new BoolVariable(false));
					isGDPRArea.AsBool = checkingGdprArea();
					TableService.Save();
				}

				return isGDPRArea.AsBool;
			}
		}

		private static bool checkingGdprArea(){
			bool _result = false;
			
			#if UNITY_EDITOR
			return true;
			#endif

			try
			{
				using( WebClient webClient = new WebClient())
				{
					string response = webClient.DownloadString( EU_QUERY_URL );
					int index = response.IndexOf( "is_request_in_eea_or_unknown\":" );
					if( index < 0 )
						_result = true;
					else
					{
						index += 30;
						_result = index >= response.Length || !response.Substring( index ).TrimStart().StartsWith( "false" );
					}
				}
			}
			catch
			{
				_result = true;
			}

			return _result;
		}
		#endregion
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.PublishingUtil{
	static public class PublishingUtil {
		static public void OpenMoreGames(){
			#if UNITY_ANDROID
			Application.OpenURL("market://search?q=pub:BIGJAM");
			#elif UNITY_IOS
			Application.OpenURL("http://phobos.apple.com/WebObjects/MZSearch.woa/wa/search?submit=sellAllLockup&media=software&entity=software&term=litqoo");
			#endif			
		}

		static public void OpenReview(string _appID){
			#if UNITY_ANDROID
			Application.OpenURL("market://details?id="+_appID+"&referrer=utm_source%3Dbigjam%26utm_campaign%3Dreview");
			#elif UNITY_IOS
			Application.OpenURL("itms-apps://itunes.apple.com/app/id" + _appID);
			#endif
		}

		static public void OpenFacebookPage(){
			Application.OpenURL("https://www.facebook.com/bigjamgames/");
		}
	}
}
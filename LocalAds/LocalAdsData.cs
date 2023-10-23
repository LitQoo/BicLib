using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.LocalAds
{

    [CreateAssetMenu(fileName = "LocalAds Data", menuName = "LocalAds/LocalAds Data", order = int.MaxValue)]
    public class LocalAdsData : ScriptableObject
    {
        public string GooglePlayAppId;
        public string AppStoreAppId;
        public Sprite Icon;
        public Sprite Screenshot;
        public string Title;
        public string ServiceLanguage;
        public string Review;
        public string Donwload;
        public Color BackgroundColor = Color.gray;
        public Color ButtonColor = Color.black;
    }
}
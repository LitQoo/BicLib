using System.Collections;
using BicDB;
using BicDB.Variable;
using System;
using System.Collections.Generic;
using BicDB.Storage;
using BicDB.Container;
using System.Linq;

namespace BicUtil.Ads
{
	public class HouseAdsModel : BicDB.Container.RecordContainer{
		public IVariable AdsId = new StringVariable("");
		public IVariable AppId = new StringVariable("");
		public IVariable Title = new StringVariable("");
		public IVariable Url = new StringVariable("");
		public IVariable ViewWeight = new IntVariable(0);
		public IListContainer<StringVariable> Images = new ListContainer<StringVariable>();
		public IListContainer<StringVariable> Ment = new ListContainer<StringVariable>();
		public IVariable BannerInterval = new FloatVariable(0.1f);
		public IVariable MentInterval = new FloatVariable(1.5f);

		public HouseAdsModel(){
			AddManagedColumn ("adsId", AdsId);
			AddManagedColumn ("id", AppId);
			AddManagedColumn ("title", Title);
			AddManagedColumn ("url", Url);
			AddManagedColumn ("viewWeight", ViewWeight);
			AddManagedColumn ("images", Images);
			AddManagedColumn ("ment", Ment);
			AddManagedColumn ("bannerInterval", BannerInterval);
			AddManagedColumn ("mentInterval", MentInterval);
		}

		public bool IsLoaded(){
			for (int i = 0; i < Images.Count; i++) {
				var _item = HouseAdsManager.HouseAdsResourceTable.FirstOrDefault (_row => _row.Url.AsString == Images[i].AsString);
				if (_item == null) {
					return false;
				}
			}
			return true;
		}

		public List<string> GetImagesPath(){
			List<string> _result = new List<string>();

			for (int i = 0; i < Images.Count; i++) {
				var _item = HouseAdsManager.HouseAdsResourceTable.FirstOrDefault(_row => _row.Url.AsString == Images[i].AsString);
				_result.Add(_item.FilePath.AsString);
			}

			return _result;
		}
	}
}
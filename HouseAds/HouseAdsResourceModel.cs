using System.Collections;
using BicDB;
using BicDB.Variable;
using System;
using BicDB.Storage;


namespace HouseAds
{
	public class HouseAdsResourceModel : BicDB.Container.RecordContainer{
		public IVariable FilePath = new StringVariable("");
		public IVariable Url = new StringVariable("");

		public HouseAdsResourceModel(){
			AddManagedColumn ("filePath", FilePath);
			AddManagedColumn ("url", Url);
		}
	}
}
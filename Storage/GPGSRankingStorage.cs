#define GPGSRanking
using System;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicDB.Storage
{
	#if GPGSRanking
	public class GPGSRankingStorage : IStorage
	{
		#region static
		static private IStorage instance = null;
		static public IStorage GetInstance(){
			if (instance == null) {
				instance = new GPGSRankingStorage();
			}

			return instance;
		}
		#endregion

		#region IStorage
		private Action<bool> OnLoadAction = null;
		public void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {

			if (_callback != null) {
				_callback(true);
			}
		}

		public void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			OnLoadAction = _callback;
			_table.Clear();

			var _loadInfo = _parameter as GPGSUtil.RankingLoadParameter;
			_loadInfo.Table = _table as ITable<GPGSUtil.RankingModel>;
			_loadInfo.Callback = setData;
			GPGSUtil.Util.GetInstance().LoadLeaderBoard(_loadInfo);
		}

		void setData(ITable<GPGSUtil.RankingModel> _table)
		{
			if (OnLoadAction != null) {
				OnLoadAction(true);
				OnLoadAction = null;
			}
		}
		#endregion
	}


	#endif
}


using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Storage;
using BicUtil.Tween;

namespace BicDB.Core
{
    public class TableLoader{
        private List<TableLoadData> tableList = new List<TableLoadData>();
        private TweenCancelObject _cancelObject = new TweenCancelObject();

        public void AddTable(ITableStorageSuppoter _table, ITableStorage _storage, object _param, Action _successCallback = null){
            _table.SetStorage(_storage);
            tableList.Insert(0, new TableLoadData(_table, _param, _successCallback));
        }

        public void AddTable(TableLoadData _data){
            if(_data.Table == null){
                return;
            }
            
            tableList.Insert(0, _data);
        }

        public void Load(Action<Result> _callback, int _waitTime){

            loadTables();

            if(leftCount == 0 && tableList.Count == 0){
                //success
                _callback(new Result(0, string.Empty));
            }else{
                var _checkTween = BicTween.Interval(1f, _waitTime).SetCancelObject(_cancelObject);
                _checkTween.SubscribeRepeat((_tween, _count)=>{
                    if(leftCount == 0 && tableList.Count > 0){
                        //retry
                        UnityEngine.Debug.Log("TableLoader Retry LoadTables, TableCount = " + tableList.Count.ToString());
                        loadTables();
                    }else if(leftCount == 0 && tableList.Count == 0){
                        //success
                        _cancelObject.Cancel();
                        _callback(new Result(0, string.Empty));
                    }
                }).SubscribeComplete(()=>{
                    _callback(new Result(999, string.Empty, 0, errorMasssage));
                });
            }
        }

        private int leftCount = 0;
        private void loadTables(){
            leftCount = tableList.Count;
            for(int i = tableList.Count - 1; i >= 0 ; i--){
                var _index = i;
                var _loadData = tableList[i];
                tableList.RemoveAt(i);
                _loadData.Table.Load(_result=>{
                    if(_result.Code != 0){
                        tableList.Add(_loadData);
                        errorMasssage += _loadData.Table.Name + "/ leftCount : " + leftCount.ToString() + "/" + _result.Message + "/" + _result.Code.ToString() +"\n";
                    }else{
                        if(_loadData.SuccessCallback != null){
                            _loadData.SuccessCallback();
                        }
                    }

                    leftCount--;
                }, _loadData.Parameter);
            }
        }

        private string errorMasssage = "";
    }

    public struct TableLoadData{
        public ITableStorageSuppoter Table;
        public object Parameter;
        public Action SuccessCallback;

        public TableLoadData(ITableStorageSuppoter _table, object _param, Action _successCallback){
            this.Table = _table;
            this.Parameter = _param;
            this.SuccessCallback = _successCallback;
        }
    }
}
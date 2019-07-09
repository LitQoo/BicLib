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
            tableList.Insert(0, _data);
        }

        public void Load(Action<Result> _callback, int _sec){

            loadTables();

            if(leftCount == 0 && tableList.Count == 0){
                //success
                _callback(new Result(0, string.Empty));
            }else{
                var _checkTween = BicTween.Interval(0.1f, _sec).SetCancelObject(_cancelObject);
                _checkTween.SubscribeRepeat((_tween, _count)=>{
                    if(leftCount == 0 && tableList.Count > 0){
                        //retry
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
                        errorMasssage = leftCount.ToString() + _result.Message;
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
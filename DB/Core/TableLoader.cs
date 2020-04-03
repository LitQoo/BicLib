using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB.Storage;
using BicDB.Variable;
using BicUtil.Tween;
using UnityEngine;

namespace BicDB.Core
{
    public class TableLoader{
        private List<TableLoadData> tableList = new List<TableLoadData>();
        private TweenTracker loadTracker = new TweenTracker();
        private int tryTime = 0;
        public void AddTable(ITableStorageSuppoter _table, ITableStorage _storage, object _param, Func<Result, bool> _passCallback = null){
           
            _table.SetStorage(_storage);
            tableList.Insert(0, new TableLoadData(_table, _param, _passCallback));
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
                var _checkTween = BicTween.Interval(1f, _waitTime).SetTracker(loadTracker);
                _checkTween.SubscribeRepeat((_tween, _count)=>{
                    if(tryTime > 3){
                        return;
                    }
                    
                    if(leftCount == 0 && tableList.Count > 0){
                        //retry
                        loadTables();
                    }else if(leftCount == 0 && tableList.Count == 0){
                        //success
                        loadTracker.Cancel();
                        _callback(new Result(0, string.Empty));
                    }
                }).SubscribeComplete(()=>{
                    _callback(new Result(999, string.Empty, 0, errorMasssage));
                });
            }
        }

        private int leftCount = 0;
        private void loadTables(){
            tryTime++;
            leftCount = tableList.Count;
            for(int i = tableList.Count - 1; i >= 0 ; i--){
                var _index = i;
                var _loadData = tableList[i];
                tableList.RemoveAt(i);
                _loadData.Table.Load(_result=>{
                    
                    bool _isPass = false;
                    if(_loadData.PassCallback == null && _result.Code == 0){
                        _isPass = true;
                    }else if(_loadData.PassCallback != null){
                        _isPass = _loadData.PassCallback(_result);
                    }
                    
                    if(_isPass == false){
                        tableList.Add(_loadData);
                        errorMasssage += _loadData.Table.Name + "/ leftCount : " + leftCount.ToString() + "/" + _result.Message + "/" + _result.Code.ToString() +"\n";
                        
                        try{
                            var _string = FileStorageUtil.ReadAndDecrypt(_loadData.Table.Name, FileStorage.GetInstance().GetEncryptKey(_loadData.Table.Name));
                            errorMasssage += "/filestring : " + _string + "/";
                        }catch(System.Exception _e){
                            errorMasssage += "/error readbypath " + _e.ToString() + "/";
                        }
                    }

                    leftCount--;
                }, _loadData.Parameter);
            }
        }

        private string errorMasssage = "";

        public async Task<BicDB.Result> LoadAsync(int _retryCount){
            int loadCount = 0;
            while(true)
            {
                List<Task<Result>> _taskList = getTaskList();

                var _result = await Task.WhenAll(_taskList.ToArray());
                
                removeLoadedTable(_result);

                if (tableList.Count == 0)
                {
                    return new BicDB.Result(0);
                }
                else
                {
                    loadCount++;
                    
                    if (loadCount > _retryCount)
                    {   
                        return new BicDB.Result(1);
                    }
                }
            }
        }

        private void removeLoadedTable(Result[] _result)
        {
            for (int i = tableList.Count - 1; i >= 0; i--)
            {
                var _tableInfo = tableList[i];
                var _isComplete = _tableInfo.PassCallback != null ? _tableInfo.PassCallback(_result[i]) : true;

                if (_result[i].IsSuccess == true && _isComplete == true)
                {
                    tableList.RemoveAt(i);
                }
                else
                {
                    Debug.Log("Task failed load table " + _tableInfo.Table.Name + "/ task.id " + i.ToString() + "/code " + _result[i].Code.ToString());
                }
            }
        }

        private List<Task<Result>> getTaskList()
        {
            List<Task<BicDB.Result>> _taskList = new List<Task<BicDB.Result>>();
            for (int i = 0; i < tableList.Count; i++)
            {
                var _param = tableList[i].Parameter;
                var _task = tableList[i].Table.LoadAsync(_param);
                _taskList.Add(_task);
            }

            return _taskList;
        }
    }

    public struct TableLoadData{
        public ITableStorageSuppoter Table;
        public object Parameter;
        public Func<Result, bool> PassCallback;

        public TableLoadData(ITableStorageSuppoter _table, object _param, Func<Result, bool> _passCallback = null){
            this.Table = _table;
            this.Parameter = _param;
            this.PassCallback = _passCallback;
        }
    }
}
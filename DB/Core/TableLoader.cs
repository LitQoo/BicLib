using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB.Storage;
using BicDB.Variable;
using BicUtil.Tween;

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
                        UnityEngine.Debug.Log("TableLoader Retry LoadTables, TableCount = " + tableList.Count.ToString());
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
                        }catch(SystemException _e){
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
            List<Task<BicDB.Result>> _taskList = new List<Task<BicDB.Result>>();
            for(int i = 0; i < tableList.Count; i++){
                var _param = tableList[i].Parameter;
                var _task = tableList[i].Table.LoadAsync(_param); 
                tableList[i].TaskId.AsInt = _task.Id;
                _taskList.Add(_task);
            }

            while(true){
                var _result = await Task.WhenAll(_taskList.ToArray());
                for(int i = _taskList.Count - 1; i >= 0; i--){
                    var _tableInfo = tableList.FirstOrDefault(_row=>_row.TaskId.AsInt==_taskList[i].Id);
                    var _isComplete = _tableInfo.PassCallback != null ? _tableInfo.PassCallback(_result[i]) : true;

                    if(_result[i].IsSuccess == true && _isComplete == true){
                        _taskList.RemoveAt(i);
                    }
                }

                if(_taskList.Count == 0){
                    return new BicDB.Result(0);
                }else{
                    loadCount++;
                    if(loadCount > _retryCount){
                        return new BicDB.Result(1);
                    }
                }
            }
        }
    }

    public struct TableLoadData{
        public ITableStorageSuppoter Table;
        public object Parameter;
        public Func<Result, bool> PassCallback;
        public IntVariable TaskId;

        public TableLoadData(ITableStorageSuppoter _table, object _param, Func<Result, bool> _passCallback = null){
            this.Table = _table;
            this.Parameter = _param;
            this.PassCallback = _passCallback;
            this.TaskId = new IntVariable(-1);
        }
    }
}
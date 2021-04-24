using System;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using BicDB.Variable;

namespace BicUtil.TableView
{
    public class TableInfiniteDataSourceAuto<T> : TableDataSourceAuto<T>  where T : IRecordContainer, new(){
        #region Instant
        private BoolVariable isCancelLoad = new BoolVariable(false);
        private BoolVariable isLoading = new BoolVariable(false);
        private BoolVariable isLoadedAll = new BoolVariable(false);
        private int rowCountForStartToLoad = 10;
        private string duplicationCheckFieldName = "";
        private int fistLoadErrorCount = 0;

        public IVariableReadOnly IsLoading{get=>this.isLoading;}
        public IVariableReadOnly IsLoadedAll{get=>this.isLoadedAll;}

        private Func<int, IRecordContainer, Task<IList<T>>> dataLoader;
        
        #endregion

        public TableInfiniteDataSourceAuto(TableView _tableView, Func<int, IRecordContainer, Task<IList<T>>> _dataLoader, Func<TableView, IList<T>, int, float> _getRowHeightFunc = null) : base(_tableView, null, _getRowHeightFunc){
            tableView = _tableView;
            getRowHeightFunc = _getRowHeightFunc;
            dataLoader = _dataLoader;
            _tableView.onRowVisibilityChanged.AddListener(reloadData);
            _tableView.SubscribeDestory(cancelLoad);
        }

        private void cancelLoad()
        {
            isCancelLoad.AsBool = true;
        }

        public async Task LoadFirst(){
            if(table != null && fistLoadErrorCount == 0){
                return;
            }

            if(fistLoadErrorCount > 2){
                return;
            }

            isLoading.AsBool = true;
            table = await dataLoader(0, null);

            if(isCancelLoad.AsBool == true){
                return;
            }

            if(table == null){
                await Task.Delay(3000);

                table = new List<T>();
                isLoading.AsBool = false;
                isLoadedAll.AsBool = true;
                fistLoadErrorCount++;
                tableView.ReloadData();
                return;
            }

            isLoadedAll.AsBool = false;
            fistLoadErrorCount = 0;
            tableView.ReloadData();
            isLoading.AsBool = false;
        }
        
        public void SetRowCountForStartToLoad(int _count){
            this.rowCountForStartToLoad = _count;
        }

        public void SetDuplicationCheckFieldName(string _fieldName){
            duplicationCheckFieldName = _fieldName;
        }

        private async void reloadData(int index, bool isVisible)
        {
            if(fistLoadErrorCount > 0 && isLoading.AsBool == false){
                await this.LoadFirst();
                return;
            }

            if((table != null && isVisible == true && isLoading.AsBool == false && isLoadedAll.AsBool == false)){
                if((this.GetRowCount() - rowCountForStartToLoad < index)){
                    var _offset = 1;

                    if(HasFootRow == true){
                        _offset++;
                    }

                    if(HasHeadRow == true){
                        _offset++;
                    }

                    var _lastData = this.GetCellData(this.GetRowCount() - _offset, 0, 0);
                    isLoading.AsBool = true;
                    var _list = await dataLoader(index, _lastData);

                    if(isCancelLoad.AsBool == true){
                        return;
                    }

                    if(_list == null){
                        isLoading.AsBool = false;
                        return;
                    }

                    if(_list.Count == 0){
                        isLoading.AsBool = false;
                        isLoadedAll.AsBool = true;
                        return;
                    }

                    for(int i = 0; i < _list.Count; i++){
                        if(string.IsNullOrEmpty(duplicationCheckFieldName) == false){
                            addIfNotContain(_list[i], duplicationCheckFieldName);
                        }else{
                            this.table.Add(_list[i]);
                        }
                    }

                    try{
                        this.tableView.ReloadData();
                    }catch{
                        //씬전환, 프로그램 종료로 인해 null exception 이 생길수도 있음.
                        Debug.LogWarning("TableInfiniteDataSourceAuto null exception");
                    }

                    isLoading.AsBool = false;
                }
            } 
        }

        private void addIfNotContain(T _item, string _key)
        {

            if(this.table.Contains(_item) == true){
                return;
            }

            var _itemPrimaryValue = _item[_key].AsVariable.AsString;
            T _foundRow = this.table.FirstOrDefault(_row=>_row[_key].AsVariable.AsString == _itemPrimaryValue);

            if(_foundRow == null){
                this.table.Add(_item);
            }
        }	

        public void ClearAllData(){
            isLoadedAll.AsBool = false;
            isLoading.AsBool = false;
            isCancelLoad.AsBool = false;
            if(table != null){
                table.Clear();
            }
            
            table = null;
        }
    }
}
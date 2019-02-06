using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;
using BicDB.Storage;
using System;
using UnityEngine;

namespace BicDB.Core
{
	static public class TableService{
        #region Const
        static public readonly string TABLENAME = "BICSYSTEM";
        #endregion

		#region Member
        static private bool isInit = false;
        static public TableContainer<TableModel> TableInfo;
        #endregion

        #region Event
        static public event Action OnSetup{
            add{
                if(isInit == true && isSetup == true){
                    value();
                }else{
                    onSetup += value;
                }
            }

            remove{
                
            }
        }

        static public event Action<string, string> OnUpdate{
            add{
                if(isInit == true && isUpdate == true && isSetup == false){
                    value(lastVersion, currentVersion);
                }else{
                    onUpdate += value;
                }
            }

            remove{

            }
        }
        
        static public bool IsSetup{get{ return isSetup; }}
        static public bool IsUpdate{get{ return isUpdate; }}

        static private string lastVersion = "";
        static private string currentVersion = "";
        static private bool isSetup = false;
        static private bool isUpdate = false;
        static private Action onSetup;
        static private Action<string, string> onUpdate;
        #endregion

        #region Logic
        static public void Init(){
            if(isInit == true){
                return;
            }

            try{
                currentVersion = Application.version;
            }catch(System.MissingMethodException _e){
                currentVersion = "0";
                return;
            }

            isInit = true;
            TableInfo = new TableContainer<TableModel>(TABLENAME);
            TableInfo.SetStorage(BicDB.Storage.FileStorage.GetInstance());

            TableInfo.Load(_result=>{
                if(TableInfo.Property.ContainsKey("version")){
                    lastVersion = TableInfo.Property["version"].AsVariable.AsString;
                }

                if(!TableInfo.Property.ContainsKey("isSetup")){
                    TableInfo.Property.Add("isSetup", new BoolVariable(true));
                    TableInfo.Property.Add("initCount", new IntVariable(1));
                    TableInfo.Property.Add("version", new StringVariable(currentVersion));
                    TableInfo.Save(_tableInfoSaveResult=>{
                        if(_tableInfoSaveResult.Code == (int)FileStorage.ResultCode.Success){
                            if(onSetup != null){
                                onSetup();
                            }

                            isSetup = true;
                        }
                    });
                }else if(currentVersion != lastVersion){
                    TableInfo.Property["version"].AsVariable.AsString = currentVersion;
                    TableInfo.Property["initCount"].AsVariable.AsInt++;
                    TableInfo.Save(_tableInfoSaveResult=>{
                        if(_tableInfoSaveResult.Code == (int)FileStorage.ResultCode.Success){
                             if(onUpdate != null){
                                onUpdate(lastVersion, currentVersion);
                            }

                            isUpdate = true;
                        }
                    });

                }else{
                    TableInfo.Property["initCount"].AsVariable.AsInt++;
                    TableInfo.Save();
                }

            }, new FileStorageParameter("filesystem"));

        }

        static public TableModel GetTableInfo(string _tableName, bool _createIfNotExsit = false){
            var _tableInfo = TableInfo.FirstOrDefault(_row=>_row.Name.AsString == _tableName);
            if(_tableInfo == null && _createIfNotExsit == true){
                _tableInfo = new TableModel();
                _tableInfo.Name.AsString = _tableName;
                TableInfo.Add(_tableInfo);
            }

            return _tableInfo;
        }

        static public IVariable GetProperty(string _key, IVariable _defaultVariable){
            Init();
            
            if(TableInfo.Property.ContainsKey(_key) == false){
                TableInfo.Property.Add(_key, _defaultVariable);
                TableInfo.Save();
            }

            return TableInfo.Property[_key].AsVariable;
        }

        static public void Save(){
            TableInfo.Save();
        }
        #endregion
	}
}
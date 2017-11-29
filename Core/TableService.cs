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
        static string lastVersion = "";
        static string currentVersion = "";
        static bool isSetup = false;
        static bool isUpdate = false;
        static private Action onSetup;
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

        static private Action<string, string> onUpdate;
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
        #endregion

        #region Logic
        static public void Init(){
            if(isInit == true){
                return;
            }

            currentVersion = Application.version;
            isInit = true;
            TableInfo = new TableContainer<TableModel>(TABLENAME);
            TableInfo.SetStorage(BicDB.Storage.FileStorage.GetInstance());

            TableInfo.Load(_result=>{
                if(TableInfo.Property.ContainsKey("version")){
                    lastVersion = TableInfo.Property["version"].AsVariable.AsString;
                }

                if(!TableInfo.Property.ContainsKey("isSetup")){
                    TableInfo.Property.Add("isSetup", new BoolVariable(true));
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
                    TableInfo.Save(_tableInfoSaveResult=>{
                        if(_tableInfoSaveResult.Code == (int)FileStorage.ResultCode.Success){
                             if(onUpdate != null){
                                onUpdate(lastVersion, currentVersion);
                            }

                            isUpdate = true;
                        }
                    });

                }

            }, new FileStorageParameter("filesystem"));

        }

        static public TableModel GetTableInfo(string _tableName){
            var _tableInfo = TableInfo.FirstOrDefault(_row=>_row.Name.AsString == _tableName);
            if(_tableInfo == null){
                _tableInfo = new TableModel();
                _tableInfo.Name.AsString = _tableName;
                TableInfo.Add(_tableInfo);
            }

            return _tableInfo;
        }
        #endregion
	}
}
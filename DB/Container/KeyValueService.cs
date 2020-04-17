using System;
using System.Collections.Generic;
using System.Linq;
using BicDB;
using BicDB.Container;
using BicDB.Core;
using BicDB.Storage;
using BicDB.Variable;

namespace BicDB.Container
{
    public class KeyValueService<KEY, VALUE> where KEY : struct where VALUE: class, IVariable, new() {
        private TableContainer<KeyValueRecord<KEY, VALUE>> table;
        private Dictionary<KEY, string> setting = new Dictionary<KEY, string>();

        public KeyValueService(string _name){
            table = new TableContainer<KeyValueRecord<KEY, VALUE>>(_name);
        }

        public void Setup(KEY _type, string _defaultValue){
            setting[_type] = _defaultValue;
        }

        public IVariableReadOnly this[KEY _type]  
        {  
            get{
                return this.get(_type).Value;
            }
        }

        public void SaveValue(Action<Func<KEY,VALUE>> _setter, Action<Result> _saveCallback = null){
            _setter(getVariable);
            Save(_saveCallback);
        }

        private VALUE getVariable(KEY _type){
            return get(_type).Value;
        }

        private KeyValueRecord<KEY, VALUE> get(KEY _type){
            return this.table.FirstOrDefault(_row=>_row.Key.AsString == _type.ToString());
        }

        private bool onLoadedTable(Result _result)
        {
            if(_result.IsSuccess == true){
                foreach(var _setting in this.setting){
                    var _keyValue = this.get(_setting.Key);
                    if(_keyValue == null){
                        _keyValue = new KeyValueRecord<KEY, VALUE>(_setting.Key);
                        _keyValue.Value.AsString = _setting.Value;
                        this.table.Add(_keyValue);
                    }
                }
                return true;
            }else{
                return false;
            }
        }

        public void Save(Action<Result> _callback = null, object _parameter = null){
            table.Save(_callback, _parameter);
        }

        public TableLoadData GetTableLoadData(){
            this.table.SetStorage(FileStorage.GetInstance());
            return new TableLoadData(this.table, new FileStorageParameter("filestorage"), onLoadedTable);
        }
    }

    public class KeyValueRecord<T, VALUE> : RecordContainer where T : struct where VALUE: class, IVariable, new()
    {
        #region FieldName

        #endregion

        #region Field
        public EnumVariable<T> Key = new EnumVariable<T>();
        public VALUE Value = new VALUE();
        #endregion

        #region LifeCycle
        public KeyValueRecord(){
            AddManagedColumn("KEY", this.Key);
            AddManagedColumn("VALUE", this.Value);
        }

        public KeyValueRecord(T _type) : this(){
            this.Key.AsEnum = _type;
            this.Value.AsInt = 0;
        }
        #endregion
    }
}

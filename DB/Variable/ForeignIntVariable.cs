using System;
using System.Linq;
using BicDB;
using BicDB.Container;
using BicDB.Storage;

namespace BicDB.Variable
{
    public class ForeignIntVariable<T> : VariableBase, IVariable where T : class, IRecordContainer, new() 
    {
        #region AsValue
        protected int data;
        public int AsInt{ get{ return data; } set{ data = value; NotifyChanged();} }
        public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = IntVariable.parse (value);} }
        public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
        public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
        public DataType Type { get { return DataType.Int; }}
        #endregion

        #region LifeCycle
        public ForeignIntVariable(ITableContainer<T> _targetTable, string _targetFieldName, Func<T, bool> _condition = null) : base(){
            targetTable = _targetTable;
            targetFieldName = _targetFieldName;
            condition = _condition;
        }
        #endregion

        #region IDataBase
        public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildNumberVariable(this, ref _json, ref _counter);
        }

        public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
            _formatter.BuildFormattedString(this, _stringBuilder);
        }

        public IVariable AsVariable{ 
            get{ 
                return this;	
            } 
        }

        public D As<D>() where D : class, IDataBase{
            return this as D;
        }
        #endregion

        #region Foreign
        protected ITableContainer<T> targetTable;
        private T record = null;
        private IntVariable targetField;
        protected string targetFieldName;
        private Func<T, bool> condition;

        public T Record{
            get{
                if(record == null){
                    cached();
                }else if(targetField != null && data != targetField.AsInt){
                    cached();
                }

                return record;
            }
        }

        private void cached(){
            if(condition == null){
                record = targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsInt == data);
            }else{
                record = targetTable.Where(condition).FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsInt == data);
            }

            if(record != null){
                targetField = record[targetFieldName].As<IntVariable>();
            }else{
                targetField = null;
            }
        }
        #endregion
    }
}
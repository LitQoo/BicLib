using System;
using System.Linq;
using BicDB.Container;
using BicDB.Storage;

namespace BicDB.Variable
{
    public class ForeignStringVariable<T> : VariableBase, IVariable where T : class, IRecordContainer, new() 
    {
        #region AsValue
        private string stringData = string.Empty;
        virtual protected string data { get=>stringData; set{stringData = value;} }
        public int AsInt{ get{ return Int32.Parse(AsString); } set{ AsString = value.ToString();} }
        public string AsString{ get{ return data; } set{ data = value;  NotifyChanged ();} }
        public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ AsString = value.ToString();} }
        public bool AsBool{ get{ return AsString.ToLower()== "true" ? true : false; } set{ AsString = (value ? "true" : "false") ;} }
        public DataType Type { get { return DataType.String; }}
        #endregion

        #region LifeCycle
        public ForeignStringVariable(ITableContainer<T> _targetTable, string _targetFieldName, Func<T, bool> _condition = null) : base(){
            targetTable = _targetTable;
            targetFieldName = _targetFieldName;
            condition = _condition;
        }
        #endregion

        #region IDataBase
        public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildStringVariable(this, ref _json, ref _counter);
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
        private StringVariable targetField;
        protected string targetFieldName;
        private Func<T, bool> condition;

        public T Record{
            get{
                if(record == null){
                    cached();
                }else if(targetField != null && data != targetField.AsString){
                    cached();
                }

                return record;
            }
        }

        private void cached(){
            if(condition == null){
                record = targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsString == data);
            }else{
                record = targetTable.Where(condition).FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsString == data);
            }

            if(record != null){
                targetField = record[targetFieldName].As<StringVariable>();
            }else{
                targetField = null;
            }
        }
        #endregion
    }
}
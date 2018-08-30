using System;
using System.Linq;
using BicDB;
using BicDB.Container;
using BicDB.Storage;

namespace BicDB.Variable
{
    public class ForeignKeyIntVariable<T> : VariableBase, IVariable where T : class, IRecordContainer, new() 
    {
        #region AsValue
        protected int data;
        public int AsInt{ get{ return data; } set{ data = value; NotifyChanged();} }
        public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = int.Parse (value);} }
        public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
        public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
        public DataType Type { get { return DataType.Int; }}
        #endregion

        #region LifeCycle
        public ForeignKeyIntVariable(ITableContainer<T> _targetTable, string _targetFieldName) : base(){
            targetTable = _targetTable;
            targetFieldName = _targetFieldName;
        }
        #endregion

        #region IDataBase
        public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildNumberVariable(this, ref _json, ref _counter);
        }

        public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
            _formatter.BuildFormattedString(this, ref _json);
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
        private ITableContainer<T> targetTable;
        private T linkedRecord = null;
        private IntVariable targetField;
        private string targetFieldName;

        public T LinkedRecord{
            get{
                if(linkedRecord == null){
                    cached();
                }else if(targetField != null && data != targetField.AsInt){
                    cached();
                }

                return linkedRecord;
            }
        }

        private void cached(){
            linkedRecord = targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsInt == data);

            if(linkedRecord != null){
                targetField = linkedRecord[targetFieldName].As<IntVariable>();
            }else{
                targetField = null;
            }
        }
        #endregion
    }
}
using BicDB.Variable;
using System.Linq;
using System;

namespace BicDB.Container
{
    public class CachedRecordForIntKey<T> where T : class, IRecordContainer, new(){
        private IntVariable foreignKeyField;
        private IntVariable targetField;
        private string targetFieldName;
        private Func<ITableContainer<T>> targetTable;
        private T value = null;
        
        public T Value{
            get{
                if(value == null){
                    cached();
                }else if(targetField != null && foreignKeyField.AsInt != targetField.AsInt){
                    cached();
                }

                return value;
            }
        }

        private void cached(){
            value = targetTable().First(_row=>_row[targetFieldName].AsVariable.AsInt == this.foreignKeyField.AsInt);
            if(value != null){
                targetField = value[targetFieldName].As<IntVariable>();
            }else{
                targetField = null;
            }
        }

        //_foreignKeyField 와 _targetTable의 레코드 멤버 _targetFieldName 를 비교하여 Value를 최신으로 캐쉬
        public CachedRecordForIntKey(IntVariable _foreignKeyField, Func<ITableContainer<T>> _targetTable, string _targetFieldName){
            foreignKeyField = _foreignKeyField;
            targetTable = _targetTable;
            targetFieldName = _targetFieldName;
        }
    }
}
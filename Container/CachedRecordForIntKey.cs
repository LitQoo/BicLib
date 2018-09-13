using BicDB.Variable;
using System.Linq;
using System;

namespace BicDB.Container
{
    public class CachedRecordForIntKey<T> where T : class, IRecordContainer, new(){
        private IVariable foreignKeyField;
        private IVariable targetField;
        private string targetFieldName;
        private ITableContainer<T> targetTable;
        private T record = null;
        
        public T Record{
            get{
                if(record == null){
                    cached();
                }else if(targetField != null && foreignKeyField.AsInt != targetField.AsInt){
                    cached();
                }

                return record;
            }
        }

        private void cached(){
            record = targetTable.FirstOrDefault(_row=>_row[targetFieldName].AsVariable.AsInt == this.foreignKeyField.AsInt);
            
            if(record != null){
                targetField = record[targetFieldName].AsVariable;
            }else{
                targetField = null;
            }
        }

        //_foreignKeyField 와 _targetTable의 레코드 멤버 _targetFieldName 를 비교하여 Value를 최신으로 캐쉬
        public CachedRecordForIntKey(IVariable _foreignKeyField, ITableContainer<T> _targetTable, string _targetFieldName){
            foreignKeyField = _foreignKeyField;
            targetTable = _targetTable;
            targetFieldName = _targetFieldName;
        }
    }
}
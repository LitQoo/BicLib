using System;
using BicDB;
using BicDB.Storage;
using BicDB.Variable;

namespace BicUtil.InfinityNumber
{
    public class InfinityNumberVariable : InfinityNumber, IVariable
    {
        public InfinityNumberVariable(int _quantity, int _unit) : base(_quantity, _unit)
        {

        }

        #region AsValue
		public int AsInt{ get{ throw new SystemException("Not Support"); } set{ throw new SystemException("Not Support"); } }
        public string AsString{ get{ return this.GetNumberString(); } set{ this.SetByString(value);  NotifyChanged ();} }
        public float AsFloat{ get{ throw new SystemException("Not Support"); } set{ throw new SystemException("Not Support"); } }
        public bool AsBool{ get{ throw new SystemException("Not Support"); } set{ throw new SystemException("Not Support");} }
        public DataType Type { get { return DataType.String; }}
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
        
        #region Logic
        public event Action<IVariableReadOnly> OnChangedValueActions = delegate{};

        public void NotifyChanged(){
            if(isTurnOffNotify == false){
                OnChangedValueActions (this as IVariableReadOnly);
            }
        }

        public void NotifyChanged(IVariableReadOnly _value){
            if(isTurnOffNotify == false){
                OnChangedValueActions (this as IVariableReadOnly);
            }
        }

        public void Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false){
            OnChangedValueActions += _callback;
            if(_needFirstCall == true){
                _callback(this as IVariable);
            }
        }

        public void Unsubscribe(Action<IVariableReadOnly> _callback){
            OnChangedValueActions -= _callback;
        }

        public void UnsubscribeAll(){
            OnChangedValueActions = delegate{};
        }

        public bool IsEqual(IVariable _variable){
            return VariableUtil.IsEqual(this as IVariable, _variable);
        }

        public void ClearNotifyAndBinding(){
            OnChangedValueActions = delegate{};
        }
        
        protected override void adjustmentUnit(){
            base.adjustmentUnit();
            NotifyChanged();
        }

        private bool isTurnOffNotify = false;
        public void SetWithoutNotify(Action _action){
            isTurnOffNotify = true;
            _action();
            isTurnOffNotify = false;
        }
        #endregion
    }
}
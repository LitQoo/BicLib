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
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ AsString = value.ToString();} }
		public string AsString{ get{ return this.GetMoneyString(" "); } set{ SetByString(value, ' ');  NotifyChanged ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ AsString = value.ToString();} }
		public bool AsBool{ get{ return AsString.ToLower()== "true" ? true : false; } set{ AsString = (value ? "true" : "false") ;} }
		public DataType Type { get { return DataType.Object; }}
		#endregion
        
        #region IDataBase
        public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildStringVariable(this, ref _json, ref _counter);
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
        
        #region Logic
        public event Action<IVariable> OnChangedValueActions = delegate{};

        public void NotifyChanged(){
            OnChangedValueActions (this as IVariable);
        }

        public void NotifyChanged(IVariable _value){
            OnChangedValueActions (this as IVariable);
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
        #endregion
    }
}
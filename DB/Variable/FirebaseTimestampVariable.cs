using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using BicDB.Storage;

namespace BicDB.Variable
{
    public class FirebaseTimestampVariable : DictionaryContainer<IntVariable>, IVariable
    {
        static string SECONDS = "_seconds";
        static string NANO_SECONDS = "_nanoseconds";

        #region Event
        public event Action<FirebaseTimestampVariable> OnChangedValueActions;

        public void Subscribe(Action<FirebaseTimestampVariable> _callback, bool _needFirstCall = false){
            
            throw new NotImplementedException();
            // OnChangedValueActions += _callback;
            // if(_needFirstCall == true){
            //     _callback(this);
            // }
        }

        public void Unsubscribe(Action<FirebaseTimestampVariable> _callback){
            
            throw new NotImplementedException();
            // OnChangedValueActions -= _callback;
        }
        #endregion

        #region LifeCycle
        public FirebaseTimestampVariable(){
            this [SECONDS] = new IntVariable (0);
            this [NANO_SECONDS] = new IntVariable (0);
        }

        event Action<IVariableReadOnly> IVariableReadOnly.OnChangedValueActions
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Member
        public IntVariable Seconds {
            get{ 
                return this [SECONDS];
            }
        }

        public IntVariable NonoSeconds {
            get{ 
                return this [NANO_SECONDS];
            }
        }

        public string AsString { 
            get{
                return this.Seconds.AsString + "." + this.NonoSeconds.AsString;
            } 

            set{
                throw new NotImplementedException();
            }
        }

        public int AsInt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float AsFloat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AsBool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Logic
        public new void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
        {
            _parser.BuildDictionaryContainer(this, ref _json, ref _counter);
        }

        public new void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
            _formatter.BuildFormattedString(this as IVariable, _stringBuilder);
        }

        public void NotifyChanged(){
            if (OnChangedValueActions != null) {
                OnChangedValueActions (this);
            }
        }

        public void ClearNotifyAndBinding (){
            OnChangedValueActions = null;
        }

        void IVariableReadOnly.Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.Unsubscribe(Action<IVariableReadOnly> _callback)
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.UnsubscribeAll()
        {
            throw new NotImplementedException();
        }

        void IVariableReadOnly.NotifyChanged(IVariableReadOnly _value)
        {
            throw new NotImplementedException();
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
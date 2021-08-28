using System;
using System.Text;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.InfinityNumber
{
    public struct InfinityNumberData{
        public int Quantity;
        public int Unit;

        public InfinityNumberData(int _quantity, int _unit){
            this.Quantity = _quantity;
            this.Unit = _unit;
        }
    }
    public class InfinityNumber{
        #region Static
        static public string[] UnitStrings = {"", "K", "M", "B", "aa", "bb", "cc", "dd", "ee", "ff","hh","ii","jj","kk","ll","nn","oo","pp","qq","rr","ss","tt","uu","vv","ww","xx","yy","zz", "AA","BB","CC","DD","EE","FF","GG","HH","II","JJ","KK","LL","MM","NN","OO","PP","QQ","RR","SS","TT","UU","VV","WW","XX","YY","ZZ"};
        static public string GetUnitString(int _unit){
            return UnitStrings[_unit];
        }

        static public InfinityNumberData StringToInfinityNumberData(string _numberString, char _splitChar = ' '){
            if(_numberString.Contains(_splitChar.ToString()) == true){
                var _numberStrings = _numberString.Split(_splitChar);
                int _unit = 0;
                for(int i = 0; i < UnitStrings.Length; i++){
                    if(UnitStrings[i] == _numberStrings[1]){
                        _unit = i;
                        break;
                    }
                }

                if(_numberStrings[0].Contains(".") == true){
                    var _numberStrings2 = _numberStrings[0].Split('.');
                    var _quantity = Int32.Parse(_numberStrings2[0]) * 1000;
                    if(_quantity > 0){
                        _quantity += Int32.Parse(_numberStrings2[1]);
                    }else{
                        _quantity -= Int32.Parse(_numberStrings2[1]);
                    }
                    return new InfinityNumberData(_quantity, _unit - 1);
                }else{
                    return new InfinityNumberData(Int32.Parse(_numberStrings[0]), _unit);
                }
            }else{
                return new InfinityNumberData(Int32.Parse(_numberString), 0);
            }
        }

        public static int GetQuantityAtUnit(InfinityNumber _number, int _unit){
            var _unitDiff = _number.Unit - _unit;
            var _quantity = _number.Quantity;

            if(_unitDiff == 0){
                return _quantity;
            }else if(_unitDiff > 0){
                for(int i = 0; i < _unitDiff; i++){
                    _quantity /= 1000;
                }
            }else{
                for(int i = 0; i < _unitDiff; i++){
                    _quantity *= 1000;
                }
            }

            return _quantity;
        }

        public static bool operator >(InfinityNumber c1, InfinityNumber c2)
        {   
            var _sub = (c1 - c2);
            if(_sub.Quantity > 0){
                return true;
            }else{
                return false;
            }
        }

        public static bool operator <(InfinityNumber c1, InfinityNumber c2)
        {
            var _sub = (c2 - c1);
            if(_sub.Quantity > 0){
                return true;
            }else{
                return false;
            }
        }

        public static bool operator >=(InfinityNumber c1, InfinityNumber c2)
        {
            var _sub = (c1 - c2);
            if(_sub.Quantity >= 0){
                return true;
            }else{
                return false;
            }
        }

        public static bool operator <=(InfinityNumber c1, InfinityNumber c2)
        {
            var _sub = (c2 - c1);
            if(_sub.Quantity >= 0){
                return true;
            }else{
                return false;
            }
        }

        public static float operator / (InfinityNumber c1, InfinityNumber c2)
        {
            float _quantity = (float)c1.Quantity / (float)c2.Quantity;
            int _unitDiff = c1.Unit - c2.Unit;
            //Debug.Log("q = " + _quantity.ToString() + "/ u = " + _unitDiff);
            if(_unitDiff > 0){
                for(int i = 0; i < _unitDiff; i++){
                    _quantity *= 1000f;
                }
            }else if(_unitDiff < 0){
                _unitDiff *= -1;
                for(int i = 0; i < _unitDiff; i++){
                    _quantity /= 1000f;
                }
            }

            return _quantity;
        }

        public static InfinityNumber operator * (InfinityNumber c1, float c2)
        {
            InfinityNumber _result = new InfinityNumber(c1.Quantity, c1.Unit);
            _result.Multiply(c2);
            return _result;
        }

        public static InfinityNumber operator / (InfinityNumber c1, float c2)
        {
            InfinityNumber _result = new InfinityNumber(c1.Quantity, c1.Unit);
            _result.Multiply(1f / c2);
            return _result;
        }

        public static InfinityNumber operator - (InfinityNumber c1, InfinityNumber c2)
        {
            InfinityNumber _result = new InfinityNumber(c1.Quantity, c1.Unit);
            _result.Sub(c2.Quantity, c2.Unit);

            return _result;
        }

        public static InfinityNumber operator + (InfinityNumber c1, InfinityNumber c2)
        {
            InfinityNumber _result = new InfinityNumber(c1.Quantity, c1.Unit);
            _result.Add(c2.Quantity, c2.Unit);

            return _result;
        }
        #endregion
        
        #region Data
        private int seed = 0;
        private int unit = 0;

        public int Unit {get{ return unit ^ seed; } private set{ unit = value ^ seed;}}
        public int Quantity {get; private set;}

        #endregion

        #region LifeCycle
        public InfinityNumber(string _numberString){
            seed = EncryptedIntVariable.Random.Next(int.MaxValue);
            SetByString(_numberString);
        }

        public InfinityNumber(int _quantity, int _unit){
            seed = EncryptedIntVariable.Random.Next(int.MaxValue);
            Set(_quantity, _unit);
        }
        #endregion

        #region Logic

        public void Set(int _quantity, int _unit){
            Unit = _unit;
            Quantity = _quantity;

            adjustmentUnit();
        }

        public long AsLong{
            get{
                long _result = Quantity;
                int _unit = this.Unit;
                while(_unit > 0){
                    _result = _result * 1000;
                    _unit--;
                }

                return _result; 
            }
        }

        public override string ToString(){
            return this.GetNumberString();
        }

        public string ToStringFullWithComma(){
            StringBuilder _result = new StringBuilder(); 
            _result.Append(Quantity.ToString("#,##0"));
            for(int i = 0; i < this.Unit; i++){
                _result.Append(",000");
            }

            return _result.ToString();
        }


        public string GetNumberString(char _splitString = ' '){
            StringBuilder _result = new StringBuilder(); 
            if(Quantity >= 1000 || Quantity <= -1000){
                 
                 if(Quantity % 1000 > 0){
                     _result.Append(Quantity / 1000);
                     _result.Append(".");
                     _result.Append((Quantity % 1000).ToString("000"));
                 }else if(Quantity % 1000 < 0){
                     _result.Append(Quantity / 1000);
                     _result.Append(".");
                     _result.Append((Mathf.Abs(Quantity % 1000)).ToString("000"));
                 }else{
                     _result.Append(Quantity / 1000);
                 }

                 if(Unit + 1 != 0){
                     _result.Append(_splitString);
                     _result.Append(GetUnitString(Unit + 1));
                 }

                 return _result.ToString();
                 
            }else{
                _result.Append(Quantity);

                if(Unit != 0){
                    _result.Append(_splitString);
                    _result.Append(GetUnitString(Unit));
                }

                return _result.ToString();
            }

        }

        
        public void SetByString(string _numberString, char _splitChar = ' '){
            var _data = StringToInfinityNumberData(_numberString, _splitChar);
            this.Set(_data.Quantity, _data.Unit);
        }

        public void Add(int _quantity, int _unit){
            if(Unit == _unit){
                Quantity += _quantity;
            }else if(Unit > _unit){
                int _unitDiff = Unit - _unit;
                for(int i = 0; i < _unitDiff; i++){
                    _quantity = _quantity / 1000;
                }

                Quantity += _quantity;
            }else if(Unit < _unit){
                int _unitDiff = _unit - Unit;
                for(int i = 0; i < _unitDiff; i++){
                    Quantity = Quantity / 1000;
                }

                Unit = _unit;
                Quantity += _quantity;
            }

            adjustmentUnit();
        }

        public void Add(string _numberString, char _splitChar = ' '){
            var _data = StringToInfinityNumberData(_numberString, _splitChar);
            this.Add(_data.Quantity, _data.Unit);
        }
        
        public void Add(InfinityNumber _subNumber){
            this.Add(_subNumber.Quantity, _subNumber.Unit);
        }

        public void Sub(int _quantity, int _unit){
            Add(_quantity * -1, _unit);
        }

        public void Sub(string _numberString, char _splitChar = ' '){
            var _data = StringToInfinityNumberData(_numberString, _splitChar);
            this.Sub(_data.Quantity, _data.Unit);
        }

        public void Sub(InfinityNumber _subNumber){
            this.Sub(_subNumber.Quantity, _subNumber.Unit);
        }


        public void Multiply(int _quantity, int _unit){
            Quantity *= _quantity;
            Unit += _unit;
            adjustmentUnit();
        }

        public void Divide(int _quantity, int _unit){
            Quantity /= _quantity;
            Unit -= _unit;
        }

        public void Multiply(float _rate){
            int _unit = 0;

            float _isMinus = _rate < 0 ? -1f : 1f;
            _rate = Mathf.Abs(_rate);

            if(_rate == 0){
                Quantity = 0;
                adjustmentUnit();
                return;
            }
            
            while(true){
                if(_rate > 1000f){
                    _rate = _rate / 1000f;
                    _unit++;
                }else if(_rate < 1f){
                    _rate = _rate * 1000f;
                    _unit--;
                }else{
                    break;
                }
            }
            
            Quantity = (int)(Quantity * _rate * _isMinus);
            Unit += _unit;
            adjustmentUnit();
        }

        public void Divide(float _rate){
            if(_rate == 0){
                throw new DivideByZeroException();
            }

            this.Multiply(1f / _rate);
        }

        protected virtual void adjustmentUnit(){
            while(true){
                if(Mathf.Abs(Quantity) >= 1000000){
                    Quantity = Quantity / 1000;
                    Unit++;
                }else if(Mathf.Abs(Quantity) < 1000 && Unit > 0){
                    Quantity = Quantity * 1000;
                    Unit--;
                }else if(Unit < 0){
                    Quantity /= 1000;
                    Unit++;
                }else{
                    break;
                }
            }
        }


        #endregion

        static public (long Min, long Max, long Value) CaculateForGarph(InfinityNumber _from, InfinityNumber _to, InfinityNumber _current){
            var _lowFieldUnit = _from.Unit;
            var _lowNumber = new InfinityNumber(_from.Quantity, 0);
            var _highNumber = new InfinityNumber(_to.Quantity, _to.Unit - _lowFieldUnit);
            var _currentNumber = new InfinityNumber(_current.Quantity, _current.Unit - _lowFieldUnit);
            return (_lowNumber.AsLong, _highNumber.AsLong, _currentNumber.AsLong);
        }
    }
}
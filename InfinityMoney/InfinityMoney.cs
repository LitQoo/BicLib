using System;
using System.Text;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.InfinityMoney
{
    public struct InfinityMoneyData{
        public int Quantity;
        public int Unit;

        public InfinityMoneyData(int _quantity, int _unit){
            this.Quantity = _quantity;
            this.Unit = _unit;
        }
    }
    public class InfinityMoney{
        #region Static
        static public string[] UnitStrings = {"", "K", "M", "B", "aa", "bb", "cc", "dd", "ee", "ff","hh","ii","jj","kk","ll","nn","oo","pp","qq","rr","ss","tt","uu","vv","ww","xx","yy","zz", "AA","BB","CC","DD","EE","FF","GG","HH","II","JJ","KK","LL","MM","NN","OO","PP","QQ","RR","SS","TT","UU","VV","WW","XX","YY","ZZ"};
        static public string GetUnitString(int _unit){
            return UnitStrings[_unit];
        }

        static public InfinityMoneyData StringToInfinityMoneyData(string _moneyString, char _splitChar = ' '){
            if(_moneyString.Contains(_splitChar.ToString()) == true){
                var _moneyStrings = _moneyString.Split(_splitChar);
                int _unit = 0;
                for(int i = 0; i < UnitStrings.Length; i++){
                    if(UnitStrings[i] == _moneyStrings[1]){
                        _unit = i;
                        break;
                    }
                }

                if(_moneyStrings[0].Contains(".") == true){
                    var _moneyStrings2 = _moneyStrings[0].Split('.');
                    var _quantity = Int32.Parse(_moneyStrings2[0]) * 1000;
                    if(_quantity > 0){
                        _quantity += Int32.Parse(_moneyStrings2[1]);
                    }else{
                        _quantity -= Int32.Parse(_moneyStrings2[1]);
                    }
                    return new InfinityMoneyData(_quantity, _unit - 1);
                }else{
                    return new InfinityMoneyData(Int32.Parse(_moneyStrings[0]), _unit);
                }
            }else{
                return new InfinityMoneyData(Int32.Parse(_moneyString), 0);
            }
        }

        public static int GetQuantityAtUnit(InfinityMoney _money, int _unit){
            var _unitDiff = _money.Unit - _unit;
            var _quantity = _money.Quantity;

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

        public static bool operator >(InfinityMoney c1, InfinityMoney c2)
        {   
            var _quantity = GetQuantityAtUnit(c2, c1.Unit);

            if(c1.Quantity > _quantity){
                return true;
            }else{
                return false;
            }
        }

        public static bool operator <(InfinityMoney c1, InfinityMoney c2)
        {
            var _quantity = GetQuantityAtUnit(c2, c1.Unit);

            if(c1.Quantity < _quantity){
                return true;
            }else{
                return false;
            }
        }


        // public static bool operator ==(InfinityMoney c1, InfinityMoney c2)
        // {
        //     var _quantity = GetQuantityAtUnit(c2, c1.Unit);

        //     if(c1.Quantity == _quantity){
        //         return true;
        //     }else{
        //         return false;
        //     }
        // }

        // public static bool operator !=(InfinityMoney c1, InfinityMoney c2)
        // {
        //     var _quantity = GetQuantityAtUnit(c2, c1.Unit);

        //     if(c1.Quantity != _quantity){
        //         return true;
        //     }else{
        //         return false;
        //     }
        // }

        public static bool operator >=(InfinityMoney c1, InfinityMoney c2)
        {
            var _quantity = GetQuantityAtUnit(c2, c1.Unit);

            if(c1.Quantity >= _quantity){
                return true;
            }else{
                return false;
            }
        }

        public static bool operator <=(InfinityMoney c1, InfinityMoney c2)
        {
            var _quantity = GetQuantityAtUnit(c2, c1.Unit);

            if(c1.Quantity <= _quantity){
                return true;
            }else{
                return false;
            }
        }

        public static float operator / (InfinityMoney c1, InfinityMoney c2)
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

        public static InfinityMoney operator - (InfinityMoney c1, InfinityMoney c2)
        {
            InfinityMoney _result = new InfinityMoney(c1.Quantity, c1.Unit);
            _result.Sub(c2.Quantity, c2.Unit);

            return _result;
        }

        public static InfinityMoney operator + (InfinityMoney c1, InfinityMoney c2)
        {
            InfinityMoney _result = new InfinityMoney(c1.Quantity, c1.Unit);
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
        public InfinityMoney(string _moneyString){
            seed = EncryptedIntVariable.Random.Next(int.MaxValue);
            SetByString(_moneyString);
        }

        public InfinityMoney(int _quantity, int _unit){
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

        public string ToString(){
            return GetMoneyString(string.Empty);
        }

        public string GetMoneyString(string _splitString = ""){
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

        
        public void SetByString(string _moneyString, char _splitChar = ' '){
            var _data = StringToInfinityMoneyData(_moneyString, _splitChar);
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

        public void Add(string _moneyString, char _splitChar = ' '){
            var _data = StringToInfinityMoneyData(_moneyString, _splitChar);
            this.Add(_data.Quantity, _data.Unit);
        }
        
        public void Add(InfinityMoney _subMoney){
            this.Add(_subMoney.Quantity, _subMoney.Unit);
        }

        public void Sub(int _quantity, int _unit){
            Add(_quantity * -1, _unit);
        }

        public void Sub(string _moneyString, char _splitChar = ' '){
            var _data = StringToInfinityMoneyData(_moneyString, _splitChar);
            this.Sub(_data.Quantity, _data.Unit);
        }

        public void Sub(InfinityMoney _subMoney){
            this.Sub(_subMoney.Quantity, _subMoney.Unit);
        }


        public void Multiply(int _quantity, int _unit){
            Quantity *= _quantity;
            Unit += _unit;
            adjustmentUnit();
        }

        public void Multiply(float _rate){
            int _unit = 0;
            
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
            
            Quantity = (int)(Quantity * _rate);
            Unit += _unit;
            adjustmentUnit();
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
    }
}
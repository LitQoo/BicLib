using System;
using System.Text;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.InfinityMoney
{

    public class InfinityMoney{

        private int seed = 0;
        private int unit = 0;

        public int Unit {get{ return unit ^ seed; } private set{ unit = value ^ seed;}}
        public int Quantity {get; private set;}

        public InfinityMoney(int _quantity, int _unit){
            seed = EncryptedIntVariable.Random.Next(int.MaxValue);
            Set(_quantity, _unit);
        }

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

        public static string[] UnitStrings = {"", "K", "M", "B", "aa", "bb", "cc", "dd", "ee", "ff","hh","ii","jj","kk","ll","nn","oo","pp","qq","rr","ss","tt","uu","vv","ww","xx","yy","zz", "AA","BB","CC","DD","EE","FF","GG","HH","II","JJ","KK","LL","MM","NN","OO","PP","QQ","RR","SS","TT","UU","VV","WW","XX","YY","ZZ"};
        public static string GetUnitString(int _unit){
            return UnitStrings[_unit];
        }
        
        public void SetByString(string _moneyString, char _splitChar = ' '){
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
                    var _quantity = Int32.Parse(_moneyStrings2[0]) * 1000 + Int32.Parse(_moneyStrings2[1]);   
                    this.Set(_quantity, _unit - 1);
                }else{
                    this.Set(Int32.Parse(_moneyStrings[0]), _unit);
                }
            }else{
                this.Set(Int32.Parse(_moneyString), 0);
            }
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

        public void Sub(int _quantity, int _unit){
            Add(_quantity * -1, _unit);
        }
    }
}
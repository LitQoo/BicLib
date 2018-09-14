# InfinityNumber

## What is this?
- 클리커류 게임에서 무한대의 재화를 구현하기 위한 클래스입니다.

## How to setup
~~~
using BicUtil.InfinityNumber;
~~~

## How to use
- InfinityNumber(int _quantity, int _unit);
    - _quantity : 수량
    - _unit : 단위 1000^_unit , 1 = K, 2 = M, 3 = B, 4 = AA ...
    ~~~
    var _gold0 = new InfinityNumber(123, 0);  // 123
    var _gold1 = new InfinityNumber(9999, 0); // 9.999K (자동단위 변환)
    var _gold2 = new InfinityNumber(123, 1);  // 123K
    var _gold3 = new InfinityNumber(8880, 1); // 8.880M (자동단위 변환)
    ~~~
- void Set(int _quantity, int _unit)
    - 값 설정
- string GetMoneyString(string _splitString = "")
    - 단위가 붙은 문자열을 돌려줍니다.
    - _splitString : 수량과 단위 사이에 넣을 문자열을 설정합니다.
- void Add(int _quantity, int _unit)
    - 더하기
  
- void Sub(int _quantity, int _unit)
    - 뺴기
- void Multiply(int _quantity, int _unit)
    - 곱하기
- void Multiply(float _rate)
    - 곱하기
- void SetByString(string _moneyString, char _splitChar = ' ')
    - _moneyString : 문자열로 값설정, ex) _1800 K_
    - _splitChar : 수량과 단위를 구분하는 문자
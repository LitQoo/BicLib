# PerpomanceChecker

## What is this?
- 퍼포먼스(코드실행시간)를 측정해 보기위한 유틸리티입니다.

## How to setup
~~~
using BicUtil.PerformanceChecker;
~~~

## How to use
- void PerformanceChecker.Instance.SpeedTest(*CaseName*, *실행할코드*, *코드반복횟수*)
    - CODE
        ~~~
        PerformanceChecker.Instance.SpeedTest("case1", ()=>{
            // TEST CODE
        }, 10000);
        ~~~
    - RESULT  
        결과는 Debug.WaringLog를 통해 콘솔에서 나옵니다.
        ~~~
        [PerformanceChecker] Time:2285 / case1
        ~~~
- long PerformanceChecker.Instance.SpeedTest(*실행할코드*, *코드반복횟수*)
    - CODE
        ~~~
        PerformanceChecker.Instance.SpeedTest(()=>{
            // TEST CODE
        }, 10000);
        ~~~
    - RESULT  
        실행시간을 돌려줍니다.
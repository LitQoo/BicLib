# README #

### What is This? ###

콤포넌트 생성시 등록된 객체들의 Initialize 메소드를 순서에 맞게 호출 합니다.
Disable상태로 시작하는 오브젝트들은 Awake함수가 즉시 호출되지 않으므로 이 유틸리티를 이용하여 첫실행 타임에 초기화를 할 수 있습니다.
컴포넌트가 소멸되면 등록된 객체의 Deinitialize() 메소드가 호출됩니다.

### How to setup ###

* Active 상태로 시작되는 객체에 ClassInitializer 콤포넌트를 추가합니다.
* 등록할 객체에 IClassInitializerObject 인터페이스를 상속하여 구현합니다.
* ClassInitializer에 객체와 실행순서를 입력하여 등록합니다.
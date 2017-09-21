# README #

### What is This? ###


등록된 ClassInitializerObject 객체들의 Initialize() 메소드를 순차적으로 호출 합니다.

Disable상태로 시작하는 오브젝트들은  Awake함수가 즉시 호출되지 않으므로 이 유틸리티를 이용하여 첫실행 타임에 초기화를 할 수 있습니다.

Active상태로 시작하는 오브젝트에 이 컴포넌트를 추가하고 ClassInitializerObject를 등록하여 사용하면 됩니다.

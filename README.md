# README #


### What is this repository for? ###

BicDB?
IntVariable, FloatVariable, StringVariable, BoolVariable 등의 통지/암호화 기능을 포함한 데이터형을 지원하며
위 데이터형을 이용한 List, Dictionary, Table, DataStore 등의 데이터 컨테이너를 제공합니다.
데이터 컨테이너는 Linq를 지원하는 다양한 종류의 Storage객체에 의해 다양한 경로(로컬, 웹, 파이어베이스, 구글플레이서비스 등)로 데이터를 저장할 수 있습니다.
유니티 클라이언트 내에서 데이터베이스를 다루듯 편리하게 데이터 검색, 정렬, 불러오기, 저장, 웹동기화등의 작업을 할 수 있습니다.

### How do I get set up? ###
BicDB를 유니티 프로젝트 Asset폴더내 적절한 곳에 submoudle 형태로 추가합니다.
(혹은 소스파일 다운로드후 디렉토리를 임포트하세요.)

### How to use? ###

*Variable
**IntVariable
{
// 1로 초기화된 객체 생성
IVariable _variable = new IntVariable(1);
// int 형태로 출력
int _value1 = _variable.AsInt;
// string 형태로 출력
string _value2 = _variable.AsString

//값 변경시 통지 받음
_variable.OnChangedValue += (IVariable _variable, string _message)=>{
	Debug.Log("값이 변경되었습니다. " + _variable.AsString);
}

//아래 처럼 값을 변경하면 OnChangedValue에 등록한 함수가 실행됩니다.
_variable.AsInt = 10;
}
**Container
**TableContainer & RecordContainer
{
class Character : RecordContainer
{
    StringVariable Name = new StringVariable();
    IntVariable Health = new IntVariable();
    FloatVariable Speed = new FloatVariable();
    
    public Caracter(){
        AddManagedColumn("Name", Name);
        AddManagedColumn("Health", Health);
        AddManagedColumn("Speed", Speed);
    }
}


//새로운 테이블 추가
ITableContainer<Character> characterTable = new TableContainer<Character>("characterTable");

//스토리지 로컬 저장소로 세팅
characterTable.SetStroage(LocalStorage.GetInstance());

//로컬에 저장된 데이터 로드
characterTable.Load(_result=>{
    if(_result == LocalStorageResult.Success){
        //성공
    }
}, new LocalStorageParameter("filename"));

//----- 로드 완료된 후 ----

//0번째 캐릭터 값 변경
characterTable[0].Name.AsString = "Jhon";
characterTable[0].Health.AsInt = 100;
characterTable[0].Speed.Asfloat = 10f;

//새로운 캐릭터 추가
characterTable.Add(new Character());

//Linq를 이용하여 테이블내에 이름으로 캐릭터찾기
Character _findCharacter = characterTable.FirstOrDefault(_row=>_row.Name.AsString == "Jhon"); 

//로컬에 데이터 저장
characterTable.Save(_result=>{
 //...
});
}


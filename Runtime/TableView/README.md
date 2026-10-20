# README

## What is This?

- 테이블UI입니다.

- BicDB.Table과 연동하여 테이블의 각 셀UI와 레코드데이터(BicDB.RecordContainer)를 자동으로 매칭시켜 줌으로 간편하게 테이블 UI을 구성할 수 있습니다.

## How to setup
- 호환성 점검 (BicLib 업데이트 시 모든 사용 프로젝트에서 확인)
    - 비활성 오브젝트를 포함한 모든 씬·프리팹·프리팹 변형 및 패키지 프리팹의 TableView/TableRow를 조사합니다.
    - 각 TableRow의 LayoutElement 참조를 같은 행의 LayoutElement 컴포넌트에 연결합니다. 런타임 생성 행도 확인합니다.
    - 예전 에셋에서 참조만 누락된 경우 TableView가 컴포넌트를 찾아 캐시합니다. 컴포넌트 자체가 없으면 행 식별자가 포함된 오류를 발생시킵니다.
    - 가로/세로 목록의 첫 열기, 닫고 재열기, 스크롤과 행 재사용, 빈 목록과 단일 항목을 검증합니다.
    - 공통 라이브러리 수정은 BicLib을 먼저 커밋·푸시한 후 사용 프로젝트의 서브모듈 커밋을 갱신합니다.
    - 에디터 수정은 기존 설치 빌드에 반영되지 않습니다. 기기 검증 시 새 빌드의 버전·빌드 번호를 확인합니다.
- 테이블 배치
    - TableView 폴더내에 있는 TableViewHorizontal(가로테이블) 혹은 TableViewVertical(세로테이블)을 원하는 곳에 배치합니다.
    - 배치한 오브젝트를 선택 후 Unity>GameObject>Break Prefab Intance를 통해 프리팹연결을 해제합니다.
- 셀 갯수 설정
    - 한줄에 몇개의 셀을 보이게 할지 설정하기 위해 배치한 오브젝트 내에 Row 오브젝트의 TableRow 콤포넌트의 CellCount를 입력합니다.
- 테이블뷰에 BicDB.Table 연결
    - TableView에 원하는 BicDB.Table을 연결합니다.
    ~~~
    [SerializeField]
    private TableView table;

    ...
    // table에 HeroShopTable 데이터가 전달됩니다.
    table.SetDBSource(GameService.Instance.HeroShopTable);
    ~~~
- 셀에 데이터 표시
    - Row오브젝트 내에 Cell오브젝트에 새로운 콤포넌트를 추가합니다.
    - 추가한 스크립트에 ITableCellWithBinder를 상속시키고 구현합니다.
    ~~~
    public class CustomCellController : MonoBehaviour, ITableCellWithBinder {
        #region Bind
        private ModelBinder binder = new ModelBinder();
        public void Bind(IRecordContainer _data, string _msg){
            binder.ClearBinding();

            var _item = _data as HeroShop;

            binder.BindModelToController(_item.Price, discriptionText, true);
            binder.BindModelToController(_item.HeroValue.Name, titleText, true);
            binder.BindModelToController(_item.Has, setBuyButtonEnabled, true);

        }

        public void Unbind(IRecordContainer _data, string _msg){
            binder.ClearBinding();
        }
        #endregion
    }
    ~~~

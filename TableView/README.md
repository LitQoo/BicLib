# README

## What is This?

- 테이블UI입니다.

- BicDB.Table과 연동하여 테이블의 각 셀UI와 레코드데이터(BicDB.RecordContainer)를 자동으로 매칭시켜 줌으로 간편하게 테이블 UI을 구성할 수 있습니다.

## How to setup
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
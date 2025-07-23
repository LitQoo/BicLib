# README #

### What is This? ###
인앱구매를 편리하게 추가하기 위한 유틸

### How to setup ###
- 구글플레이, 애플디벨롭센터 및 아이튠즈커넥트에서 새로운 앱을 등록합니다.
- service 메뉴에서 In App Purchase 를 On시킵니다.
- 서비스 IAP설정에서 iap sdk를 임포트합니다.
- 프로젝트 플레이어 세팅에 scripting define symbols 에 BICUTIL_IAP 를 등록합니다.


### How to use ###
- init code , 게임 시작시 실행되도록 (ex. gameservice.init 에 추가.)
~~~
private static void setupInAppPurchase(){
    PurchasingTable.PrimaryKey = ProductModel.KEY_PRIMARY;
    PurchasingTable.SetStorage(FileStorage.GetInstance());
    PurchasingTable.Load(_result=>{
        PurchasingTable.AddWithoutDuplication(new ProductModel("com.bigjam.spacequiz.premium", 0, string.Empty));

        if(PurchasingTable.GetRowByPrimaryKey("com.bigjam.spacequiz.premium").Value.AsInt > 0){
            AdsManager.SetNoAds(true);
        }
    }, new FileStorageParameter("bigjam"));

    PurchasingManager.Instance.AddProduct("com.bigjam.spacequiz.premium", ProductType.NonConsumable, _result=>{
        var _row = PurchasingTable.GetRowByPrimaryKey("com.bigjam.spacequiz.premium");
        if(_result == PurchasingResult.Complete){
            _row.Value.AsInt = 1;
            AdsManager.SetNoAds(true);
            PurchasingTable.Save();
            Debug.Log("NOADS ON");
        }else{
            _row.Value.AsInt = 0;
            AdsManager.SetNoAds(false);
            PurchasingTable.Save();
            Debug.Log("NOADS OFF");
        }

        Analytics.CustomEvent("finishToBuyNoAds", new Dictionary<string, object>
        {
            { "result", _result.ToString() }
        });
    });

    PurchasingManager.Instance.Initialize();
}
~~~

db 구성, gameservice
~~~
public class GameService{
    static public TableContainer<ProductModel> PurchasingTable = new TableContainer<ProductModel>("Purchasing");

}

public class ProductModel : RecordContainer{
    #region const
    public const string KEY_PRIMARY = "id";
    //public const string ID_NOADS = "com.bigjam.spacequiz.noads";
    public const string ID_PREMIUM = "com.bigjam.spacequiz.premium";
    #endregion

    #region Field
    public StringVariable Id = new StringVariable();
    public EncryptedIntVariable Value = new EncryptedIntVariable();
    public StringVariable Recipt = new StringVariable();
    #endregion

    #region LifeCycle
    public ProductModel(){
        init();
    }

    public ProductModel(string _id, int _value, string _recipt){
        Id.AsString = _id;
        Value.AsInt = _value;
        Recipt.AsString = _recipt;

        init();
    }

    private void init(){
        AddManagedColumn (KEY_PRIMARY, Id);
        AddManagedColumn ("value", Value);
        AddManagedColumn ("recipt", Recipt);
    }
    #endregion
}
~~~

- 구매시
~~~
private void bindModel(){
    binder.ClearBinding();

    binder.BindModelToController(GameService.PurchasingTable.GetRowByPrimaryKey(ProductModel.ID_PREMIUM).Value, setNoAds);
}


private void bindModel(){
    binder.ClearBinding();

    binder.BindModelToController(GameService.PurchasingTable.GetRowByPrimaryKey(ProductModel.ID_PREMIUM).Value, setNoAds);
}

private void setNoAds(IVariable _option, string _msg = ""){
    if(_option.AsInt == 1){
        setAlert(true, "결제가 완료되었습니다 감사합니다! 더 큰 즐거움으로 보답하겠습니다.", ()=>{
            UIFlow.Instance.Back(CloseMode.Disable);
        });
    }else{
        setAlert(true, "결제에 실패하였습니다. 다시 시도해주세요", ()=>{
            setAlert(false);
        });
    }
}
~~~

- 리스토어
~~~
private void bindModel(){
    binder.BindModelToController(GameService.PurchasingTable.GetRowByPrimaryKey(ProductModel.ID_PREMIUM).Value, setRestoreButton, true);
}

public void OnClickedRestoreButton(){
    if(GameService.PurchasingTable.GetRowByPrimaryKey(ProductModel.ID_PREMIUM).Value.AsInt == 0){

        PurchasingManager.Instance.RestorePurchases();

        Analytics.CustomEvent("RestoreNoAds", new Dictionary<string, object>
        {
                { "where", "option" }
        });
    }
}

private void setRestoreButton(IVariable _option, string _message)
{
    #if UNITY_ANDROID
    restoreButton.gameObject.SetActive(false);
    #else

    if(_option.AsInt == 0){
        restoreButton.gameObject.SetActive(true);
    }else{
        restoreButton.gameObject.SetActive(false);
    }
    #endif
}
~~~
### How to test ###

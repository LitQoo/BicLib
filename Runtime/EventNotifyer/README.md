# README #

### What is This? ###
UI.BUTTON의 메소드 실행방식을 조금 우아하게 표현해보고자 만들었습니다.
(사실 만든 이유 까먹음..)

### How to setup ###
UI.BUTTON을 연결할 객체를 다음과 같이 구현합니다.

~~~
public class ExitPopup : MonoBehaviour{
    public const string BUTTON_EXIT = "exit";
    public void OnClicked (string _actionName)
    {
		EventNotifyer.Notify(this, _actionName);
	}
    
    [SubscribeEvent(BUTTON_EXIT)]
	private void exit(){
		Debug.log("clicked Button");
	}
    
    void Awake(){ // ClassInitializer를 이용하면 더욱 좋음.
        EventNotifyer.SubscribeBySelf(this);
    }
    
    Void OnDestroy(){
        EventNotifyer.ClearSubscription(this);
    }    
}
~~~
UI.BUTTON에 OnClicked메소드를 연결하고 파라메터로 exit를 입력합니다.

다른 객체에서 이벤트 등록시
~~~
[SubscribeEvent(ExitPopup.BUTTON_EXIT, TargetType = typeof(ExitPopup))]
private void closePopup(){
    Debug.log("clicked button");
}

void Awake(){
    EventNotifyer.SubscribeByAttribute(exitPopup, this);
}

void OnDestroy(){
	EventNotifyer.ClearSubscription(this);
}
~~~

다른 객체에서 호출시


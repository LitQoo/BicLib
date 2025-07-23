# UIFlow

## What is this?
- 각종 UI와 팝업의 흐름을 정리합니다.

- 새로운 창을 열때 기존창을 닫고 열것인지 위에 열것인지, 창을 닫은 후에는 어떤창을 뛰울지에 대한 정의를 할 수 있습니다.

- 그리고 창을 열고 닫을 때 해당 창에 이벤트 함수를 호출을 통해 파라메터를 전달하여 특별한 처리를 할 수 있도록 합니다.

- UI혹은 팝업오브젝트에 IUIFlowObject를 상속시키면 싱글톤 객체인 UIFlow.Instance를 통해 제어할 수 있습니다.

## How to setup
- OnCloseUIResult OnClosedUI(IUIFlowObject _fromUI, Action _finishCallback, object _parameter)
    - 창이 닫힐때 호출됩니다.
    - OnCloseUIResult
        - OnCloseUIResult.DoNotWait
            - 창닫기 연출이 따로 없을 때 리턴합니다.
        - OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext
            - 현재창과 다음창의 닫힘/열림 연출이 동시에 일어날때 사용합니다.
            - 다음창 오픈은 즉시 합니다.
            - _finishCallback 을 호출하여야 현재창에 대한 닫힘처리를 완료합니다.
        - OnCloseUIResult.WaitForFinishCallback
            - 현재창 닫힘 연출을 완료한 후 순차적으로 다음창 열림 연출을 할때 사용합니다.
            - _finishCallback 을 호출하여야 현재창에 대한 닫힘처리를 완료하고 다음창에 대한 오픈 처리를 시작합니다.

- void OnOpenedUI(IUIFlowObject _fromUI, object _paramter)
    - 창이 열릴때 호출됩니다.
- 예제
~~~
using BicUtil.UIFlow;

public class BoxOpenPopup : MonoBehaviour, IUIFlowObject
{
    public OnCloseUIResult OnClosedUI(IUIFlowObject _fromUI, Action _finishCallback, object _parameter)
    {
        return OnCloseUI.DoNotWait;
    }

    public void OnOpenedUI(IUIFlowObject _fromUI, object _paramter)
    {
        
    }
}
~~~


## How to use
- UIFlow.Instance.SetBase
    - 최상위 UI를 지정합니다.
    - void SetBase(IUIFlowObject _ui, OpenMode _openMode, object _param = null)

- UIFlow.Instance.RegisterUI
    - UI를 등록합니다.
    - IClassInitializerObject 와 등록여부를 보장 받을 수 있어 좋습니다.
    - 클래스당 1개의 UI만 등록할 수 있습니다. 그외에는 수동모드 사용.
    - void RegisterUI(IUIFlowObject _object)
        ~~~
        public class BigjamPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject {
        ...
            public void IClassInitializerObject.Initialize ()
            {
                UIFlow.Instance.RegisterUI(this);
            }
        ...
        ~~~
- UIFlow.Instance.UnregisterUI
    - 등록된 UI를 제거합니다.
    - void UnregisterUI(IUIFlowObject _object)
        ~~~
        public class BigjamPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject {
        ...
            public void IClassInitializerObject.Deinitialize ()
            {
                UIFlow.Instance.UnregisterUI(this);
            }
        ...
        ~~~
- UIFlow.Instance.ClearRegisteredUI
    - 등록된 모든 UI를 제거합니다.
    - 씬을 전환할때(LoadScene을 사용할때는 제외), 더이상 UI를 사용하지 않을 때 반드시 호출해줍니다.
    - 주로 base로 지정된 UI가 제거될때 호출되면 좋습니다.
    - void ClearRegisteredUI()
        ~~~
        public class BigjamPopup : MonoBehaviour, IClassInitializerObject, IUIFlowObject {
        ...
            public void IClassInitializerObject.Deinitialize ()
            {
                UIFlow.Instance.ClearRegisteredUI();
            }
        ...
        ~~~
    - 
- UIFlow.Instance.GetUI
    - UI를 얻어 옵니다. 
    - IUIFlowObject GetUI<UIClass>()
        - 등록된 UI를 얻어 옵니다.
    - IUIFlowObject GetUI(Type _type)
        - 등록된 UI를 얻어 옵니다.
- UIFlow.Instance.Enter
    - UI를 오픈합니다. 현재 오픈되어있는 UI는 히스토리에 저장됩니다.
    - void Enter<UIClass>(OpenMode _openMode, object _parameter = null)
        - 등록된 UIClass를 찾아 오픈합니다.
    - void Enter(string _objectName, OpenMode _openMode, object _parameter = null)
        - 게임오브젝트에 지정된 이름을 이용하여 UI를 찾아 오픈합니다. 비교적 느립니다.
    - void Enter(IUIFlowObject _ui, OpenMode _openMode, object _parameter = null)
        - 지정된 UI를 오픈합니다.
- UIFlow.Instance.Replace
    - UI를 오픈합니다. 현재 오픈되어있는 UI는 제거하며 히스토리에도 저장되지 않습니다.
    - void Replace(string _objectName, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null)
        - 게임오브젝트에 지정된 이름을 이용하여 UI를 찾아 오픈합니다. 비교적 느립니다.
    - void Replace<UIClass>(OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null)
        - 등록된 UIClass를 찾아 오픈합니다.
    - void Replace(IUIFlowObject _ui, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null)
        - 지정된 UI를 오픈합니다.
- UIFlow.Instance.Back
    - 현재 UI를 닫고 히스토리에 저장되어있는 마지막 UI로 돌아갑니다.
    - void Back(CloseMode _closeMode, object _parameter = null)
- UIFlow.Instance.BackTo
    - 현재 UI를 닫고 히스토리에 저장되어있는 특정 UI로 돌아갑니다.
    - void BackTo<UIClass>(Action _callback)
    - void BackTo(IUIFlowObject _ui, Action _callback)
- UIFlow.Instance.LoadScene
    - 지정한 씬으로 전환합니다.
    - 기존 등록된 UI는 모두 제거됩니다.
    - void LoadScene(string _sceneName)
- UIFlow.Instance.QuitApp
    - 어플리케이션을 종료합니다.
    - void QuitApp()
- UIFlow.Instance.SetBackKeyAction
    - 안드로이드의 백키를 눌렀을때 동작을 지정합니다.
    - 기본적으로는 Back() 메소드가 지정되어 있습니다.
    - void SetBackKeyAction(Action _action)

- OpenMode
    - UI오픈시 동작을 정의합니다.
    - OpenMode.Overlap
        - 현재창이 유지된 상태로 그위에 UI를 오픈합니다.
    - OpenMode.Change
        - 현재창을 disable (gameobject.SetActive(false)) 하고 그위에 UI를 오픈 합니다.
- CloseMode
    - CloseMode.Disable
        - 현재창을 disable(gameobject.SetActive(false)) 합니다.
        - 재활용하는 UI의 경우에 사용합니다.
    - CloseMode.Destory
        - 현재창을 완전 삭제합니다.
        - 그때 그때 생성하고 제거하는 UI의 경우에 사용합니다.
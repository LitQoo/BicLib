# PerpomanceChecker

## What is this?
- 오브젝트풀링을 편리하게 할 수 있도록 도와줍니다.

## How to setup
~~~
using BicUtil.ObjectPuller;
~~~

## How to use **PrefabPuller**
- Setup
    ~~~
    public class OneObject : MonoBehaviour, IPullingObject{
        #region Pulling
        public static PrefabPuller<OneObject> Puller;

        public void ReadyPulling(){
            //제거(풀링)될 때 실행되는 코드
            this.gameObject.SetActive(false);
        }

        public void ReadyUsing(){
            //사용될 때 실행되는 코드
            this.gameObject.SetActive(true);
        }

        public static OneObject Create(){
            var _object = Puller.GetObject();
            return _object;
        }

        #endregion

        ...
    }
    ~~~

    상위 컨트롤러에서 세팅
    ~~~
    void Awake(){
        OneObject.Puller = new PrefabPuller<OneObject>(0, "프리팹위치", _object=>{
            // 오브젝트 생성 후 실행될 코드
        });
    }

    void OnDestory(){
        puzzleController.OnDestroys += OneObject.Puller.Clear;
    }
    ~~~
- Use
    ~~~
    OneObject _obj = OneObject.Create();
    ~~~
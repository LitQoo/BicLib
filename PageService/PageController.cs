using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.PageService
{
    public class PageController : MonoBehaviour
    {
        #region LinkingObject
        [SerializeField]
        private Transform pageParent;

        private bool isDeinitialize = false;
        #endregion

        #region Instant
        [HideInInspector]
        public string SceneName;
        private object sceneTransitionParameter = null;
        private Dictionary<Type, IPage> pages = new Dictionary<Type, IPage>();
        private Stack<IPage> pageStack = new Stack<IPage> ();
        public IPage CurrentPage { 
            get{ 
                return pageStack.Peek(); 
            }
        }

        public void LogPage(){
            Debug.Log("[PageController] LogPage - start");
            foreach(var _v in pageStack){
                Debug.Log(_v.GetType().ToString());
            }
            Debug.Log("[PageController] LogPage - finish");
        }
        #endregion

        #region TweenPool
        private void OnEnable() {
            BicTween.DefaultPool = getTweenPool();
        }

        private TweenPool tweenPool = null;
        private TweenPool getTweenPool(){
            if(tweenPool == null){  
                tweenPool = this.gameObject.AddComponent(typeof(TweenPool)) as TweenPool; 
            }

            return tweenPool;
        }
        #endregion


        #region LifeCycle
        private void Awake(){
            this.SceneName = gameObject.scene.name;
            BicTween.DefaultPool = getTweenPool();
            this.sceneTransitionParameter = PageManager.Instance.PopSceneTransitionParameter();
            PageManager.Instance.AddController(this);
            initialize ();
        }

        private void Start(){
            SceneManager.SetActiveScene(this.gameObject.scene);
        }

        private void OnDestroy() {
            deinitialize();
        }

        private void OnApplicationQuit() {
            deinitialize();
        }
        #endregion

        #region initialize
        private void initialize(){
            DebugForEditor.Log("[PageControlelr.initialize]");

            if(pageParent != null){
                var _initCount = pages.Count;
                var _pages = pageParent.GetComponentsInChildren<IPage>(true);
                for(int i = 0; i < _pages.Length; i++){
                    var _page = _pages[i];
                    var _type = _page.GetType();
                    if(pages.ContainsKey(_type) == false){
                        pages[_type] = _page;
                    }
                }
            }

            var _orderedList = pages.OrderBy (_object => _object.Value.InitializeOrder);

            foreach (var _page in _orderedList) {
                _page.Value.PageController = this;
                _page.Value.InitializePage();
            }
        }

        private void deinitialize(){
            if(isDeinitialize == false){
                DebugForEditor.Log("[PageControlelr.deinitialize] " + this.SceneName);

                if(tweenPool != null){
                    tweenPool.CancelAll();
                }

                if(BicTween.IsCurrentDefaultPool(tweenPool)){
                    BicTween.DefaultPool = null;
                }

                var _orderedList = pages.OrderBy (_object => _object.Value.InitializeOrder);

                foreach (var _page in _orderedList) {
                    _page.Value.DeinitializePage();
                }

                this.pages.Clear();
                this.pageStack.Clear();

                isDeinitialize = true;
            }
        }
        #endregion
        
        public void Register(IPage _page){
            if(pages.ContainsKey(_page.GetType()) == true){
                throw new SystemException("[PageManager] " + _page.GetType().ToString() + " is already registered");
            }

            pages[_page.GetType()] = _page;
        }

        public void Unregister(IPage _page){
            pages.Remove(_page.GetType());
        }

        public void UnregisterAll(){
            pages.Clear();
        }

        public void SetBasePage(IPage _page, object _param = null){
            if(pageStack.Count > 0){
                throw new SystemException("[PageManager] already set base");
            }

            Debug.Log("SetBasePage");
            if(sceneTransitionParameter != null){
                _param = sceneTransitionParameter;
                sceneTransitionParameter = null;
            }
            

            Debug.Log("stackpush");
            pageStack.Push(_page);

            Debug.Log("onopenpage " + CurrentPage.GetType().ToString());
            CurrentPage.OnOpenedPage(null, _param);
        }

        public void ClearVariables(){
            sceneTransitionParameter = null;
            UnregisterAll();
            pageStack.Clear();
        }

        public async Task<AsyncOperation> SceneReplaceAsync(string _sceneName, object _param = null){
            var _result = await PageManager.Instance.SceneReplaceAsync(_sceneName, _param);
            return _result;
        }

        public async void SceneReplace(string _sceneName, object _param = null){
            await SceneReplaceAsync(_sceneName, _param);
        }

        public async Task<AsyncOperation> SceneEnterAsync(string _sceneName, object _openParam = null){
            var _result = await PageManager.Instance.SceneEnterAsync(_sceneName, _openParam);
            return _result;
        }

        public async void SceneEnter(string _sceneName, object _openParam = null){
            await PageManager.Instance.SceneEnterAsync(_sceneName, _openParam);
        }

        public async Task SceneBackAsync(object _openParam = null, object _closeParam = null){
            await PageManager.Instance.SceneBackAsync(_openParam, _closeParam);
        }

        public async void SceneBack(object _openParam = null, object _closeParam = null){
            await this.SceneBackAsync(_openParam, _closeParam);
        }

        public async Task EnterAsync<PageClass>(object _param = null){
            var _openPageType = typeof(PageClass);
            if(pages.ContainsKey(_openPageType) == false){
                throw new SystemException("[PageManager] "+_openPageType.ToString() + " is not resistered");
            }
            
            var _openPage = pages[_openPageType];
            var _lastPage = CurrentPage;
            pageStack.Push(_openPage);

            await _openPage.OnOpenedPage(_lastPage, _param);
        }

        public async void Enter<PageClass>(object _param = null){
            await this.EnterAsync<PageClass>(_param);
        }

        public async Task ChangeAsync<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null)
        {
            var _openPageType = typeof(PageClass);
            if (pages.ContainsKey(_openPageType) == false)
            {
                throw new SystemException("[PageManager] " + _openPageType.ToString() + " is not resistered");
            }

            var _openPage = pages[_openPageType];
            var _lastPage = CurrentPage;
            pageStack.Push(_openPage);

            var _closeTask = _lastPage.OnClosedPage(_openPage, _closeParam);
            var _openTask = _openPage.OnOpenedPage(_lastPage, _openParam);

            await transitionPage(_transition, _closeTask, _openTask);
        }

        public async void Change<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null){
            await this.ChangeAsync<PageClass>(_transition, _openParam, _closeParam);
        }

        public async Task ReplaceAsync<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null){
            var _openPageType = typeof(PageClass);
            if(pages.ContainsKey(_openPageType) == false){
                throw new SystemException("[PageManager] "+_openPageType.ToString() + " is not resistered");
            }

            var _openPage = pages[_openPageType];
            var _lastPage = CurrentPage;
            pageStack.Pop();
            pageStack.Push(_openPage);

            var _closeTask = _lastPage.OnClosedPage(_openPage, _closeParam);
            var _openTask = _openPage.OnOpenedPage(_lastPage, _openParam);

            await transitionPage(_transition, _closeTask, _openTask);
        }

        public async void Replace<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null){
            await this.ReplaceAsync<PageClass>(_transition, _openParam, _closeParam);
        }

        public async Task BackAsync(object _closeParam = null){
            var _lastPage = pageStack.Pop();
            await _lastPage.OnClosedPage(CurrentPage, _closeParam);
        }

        public async void Back(object _closeParam = null){
            await this.BackAsync(_closeParam);
        }

        public async Task BackAsync(PageTransition _transition, object _openParam = null, object _closeParam = null){
            var _lastPage = pageStack.Pop();
            var _openPage = CurrentPage;

            var _closeTask = _lastPage.OnClosedPage(_openPage, _closeParam);
            var _openTask = _openPage.OnOpenedPage(_lastPage, _openParam);

            await transitionPage(_transition, _closeTask, _openTask);
        }

        public async void Back(PageTransition _transition, object _openParam = null, object _closeParam = null){
            await this.BackAsync(_transition, _openParam, _closeParam);
        }

        public async void BackTo<PageClass>(PageTransition _transition, object _openParam = null){
            await this.BackToAsync<PageClass>(_transition, _openParam);
        }

        public async Task BackToAsync<PageClass>(PageTransition _transition, object _openParam = null){
            Task _openTask = null; 
            Task _closeTask = null;
            while(true){
                var _lastPage = pageStack.Pop();
                _closeTask = _lastPage.OnClosedPage(CurrentPage, null);

                if(this.CurrentPage is PageClass){
                    if(_openParam != null){
                        _openTask = this.CurrentPage.OnOpenedPage(_lastPage, _openParam);
                    }

                    break;
                }

                if(pageStack.Count == 1){
                    Debug.LogError("[PageManager] BackToAsync is Fail");
                    break;
                }
            }

            await transitionPage(_transition, _closeTask, _openTask);
        }

        internal async Task reopenFromSceneAsync(string _sceneName, Type _lastPageType, object _openParam){
            await CurrentPage.OnOpenedPage(new ScenePage(_sceneName, _lastPageType), _openParam);
        }

        private async Task transitionPage(PageTransition _transition, Task _closeTask, Task _openTask)
        {
            if (_transition == PageTransition.Sequance)
            {
                if(_closeTask != null){
                    await _closeTask;
                }

                if(_openTask != null){
                    await _openTask;
                }
            }
            else
            {
                if(_closeTask != null && _openTask != null){
                    await Task.WhenAll(new[] { _closeTask, _openTask });
                }else if(_closeTask == null && _openTask == null){

                }else if(_openTask == null){
                    await _closeTask;
                }else if(_closeTask == null){
                    await _openTask;
                }
            }
        }

        public void SetBackKeyAction(Action _action){
            //TODO: setbackkey
            Debug.LogWarning("SetBackeyaction impl");
        }
    }

    public class ScenePage : IPage
    {
        public string SceneName{get; private set;}
        public Type LastPageType{get; private set;}
        public ScenePage(string _name, Type _lastPageType){
            SceneName = _name;
            LastPageType = _lastPageType;
        }

        public int InitializeOrder => throw new NotImplementedException();

        public PageController PageController { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DeinitializePage()
        {
            throw new NotImplementedException();
        }

        public void InitializePage()
        {
            throw new NotImplementedException();
        }

        public Task OnClosedPage(IPage _fromUI, object _param = null)
        {
            throw new NotImplementedException();
        }

        public Task OnOpenedPage(IPage _fromPage, object _param = null)
        {
            throw new NotImplementedException();
        }
    }
}

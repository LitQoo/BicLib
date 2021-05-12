using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public string SceneName;
        private object sceneTransitionParameter = null;
        private Dictionary<Type, IPage> pages = new Dictionary<Type, IPage>();
        private Stack<IPage> pageStack = new Stack<IPage> ();
        public IPage CurrentPage { 
            get{ 
                return pageStack.Peek(); 
            }
        }
        #endregion


        #region LifeCycle
        private void Awake(){
            this.SceneName = gameObject.scene.name;
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
            DebugForEditor.Log("[PageControlelr.deinitialize]");
            if( isDeinitialize == false){
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

            if(sceneTransitionParameter != null){
                _param = sceneTransitionParameter;
                sceneTransitionParameter = null;
            }
            
            pageStack.Push(_page);
            CurrentPage.OnOpenedPage(null, _param);
        }

        public void ClearVariables(){
            sceneTransitionParameter = null;
            UnregisterAll();
            pageStack.Clear();
        }

        public AsyncOperation SceneReplaceAsync(string _sceneName, object _param = null){
            return PageManager.Instance.SceneReplaceAsync(_sceneName, _param);
        }

        public void SceneReplace(string _sceneName, object _param = null){
            _ = SceneReplaceAsync(_sceneName, _param);
        }

        public AsyncOperation SceneEnterAsync(string _sceneName, object _param = null){
            return PageManager.Instance.SceneEnterAsync(_sceneName, _param);
        }

        public void SceneEnter(string _sceneName, object _param = null){
            _ = PageManager.Instance.SceneEnterAsync(_sceneName, _param);
        }

        public async Task SceneBackAsync(){
            await PageManager.Instance.SceneBackAsync();
        }

        public void SceneBack(){
            _ = this.SceneBackAsync();
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

        public void Enter<PageClass>(object _param = null){
            _ = this.EnterAsync<PageClass>(_param);
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

        public void Change<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null){
            _ = this.ChangeAsync<PageClass>(_transition, _openParam, _closeParam);
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

        public void Replace<PageClass>(PageTransition _transition = PageTransition.Sequance, object _openParam = null, object _closeParam = null){
            _ = this.ReplaceAsync<PageClass>(_transition, _openParam, _closeParam);
        }

        public async Task BackAsync(object _closeParam = null){
            var _lastPage = pageStack.Pop();
            await _lastPage.OnClosedPage(CurrentPage, _closeParam);
        }

        public void Back(object _closeParam = null){
            _ = this.BackAsync(_closeParam);
        }

        public async Task BackAsync(PageTransition _transition, object _openParam = null, object _closeParam = null){
            var _lastPage = pageStack.Pop();
            var _openPage = CurrentPage;

            var _closeTask = _lastPage.OnClosedPage(_openPage, _closeParam);
            var _openTask = _openPage.OnOpenedPage(_lastPage, _openParam);

            await transitionPage(_transition, _closeTask, _openTask);
        }

        public void Back(PageTransition _transition, object _openParam = null, object _closeParam = null){
            _ = this.BackAsync(_transition, _openParam, _closeParam);
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
        }
    }


    // class PageIntializer : MonoBehaviour{
    //     #region LinkingObject
    //     [SerializeField]
    //     private Transform pageParent;
    //     [SerializeField]
    //     private List<IPage> pages;

    //     private bool isDeinitialize = false;
    //     #endregion

    //     #region LifeCycle
    //     private void Awake(){
    //         PageManager.Instance.stack
    //         initialize ();
    //     }

    //     private void OnDestroy() {
    //         deinitialize();
    //     }

    //     private void OnApplicationQuit() {
    //         deinitialize();
    //     }
    //     #endregion

    //     #region logic
    //     private void initialize(){
    //         if(pageParent != null){
    //             var _initCount = pages.Count;
    //             var _pages = pageParent.GetComponentsInChildren<IPage>(true);
    //             for(int i = 0; i < _pages.Length; i++){
    //                 var _page = _pages[i];
    //                 if(pages.Contains(_page) == false){
    //                     PageManager.Instance.CurrentController.Register(_page);
    //                     pages.Add(_page);
    //                 }
    //             }
    //         }

    //         var _orderedList = pages.OrderBy (_object => _object.InitializeOrder);

    //         foreach (var _page in _orderedList) {
    //             _page.InitializePage();
    //         }
    //     }

    //     private void deinitialize(){
    //         if( isDeinitialize == false){
    //             var _orderedList = pages.OrderBy (_object => _object.InitializeOrder);

    //             foreach (var _page in _orderedList) {
    //                 PageManager.Instance.CurrentController.Unregister(_page);
    //                 _page.DeinitializePage();
    //             }

    //             isDeinitialize = true;
    //         }
    //     }
    //     #endregion
    // }
}

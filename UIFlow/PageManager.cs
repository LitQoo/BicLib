using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicUtil.SingletonBase;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.PageFlow
{
    public class PageManager : MonoBehaviourHardBase<PageManager>
    {
        private object sceneTransitionParameter = null;
        private Dictionary<Type, IPage> pages = new Dictionary<Type, IPage>();
        private Stack<IPage> pageStack = new Stack<IPage> ();
        public IPage CurrentPage { 
            get{ 
                return pageStack.Peek(); 
            }
        }

        private void Awake() {
            this.name = "PageManager";    
        }
        
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

        public void RegisterBase(IPage _page, object _param = null){
            if(pageStack.Count > 0){
                throw new SystemException("[PageManager] already set base");
            }

            if(sceneTransitionParameter != null){
                _param = sceneTransitionParameter;
                sceneTransitionParameter = null;
            }
            
            this.Register(_page);
            pageStack.Push(_page);
            CurrentPage.OnOpenedPage(null, _param);
        }

        private void clearValriables(){
            sceneTransitionParameter = null;
            UnregisterAll();
            pageStack.Clear();
        }

        public AsyncOperation SceneReplaceAsync(string _sceneName, object _param = null){
            clearValriables();

            if(ClassInitializer.ClassInitializer.Instance != null){
                ClassInitializer.ClassInitializer.Instance.Deinitialize();
            }

            sceneTransitionParameter = _param;
            return SceneManager.LoadSceneAsync(_sceneName);
        }

        public void SceneReplace(string _sceneName, object _param = null){
            _ = SceneReplaceAsync(_sceneName, _param);
        }

        //TODO: scene마다 pageStack을 따로 두고 관리해야함.
        // public AsyncOperation SceneEnterAsync(string _sceneName, object _param = null){
        //     sceneTransitionParameter = _param;
        //     return SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        // }

        // public AsyncOperation BackSceneAsync(object _param = null){
        //     sceneTransitionParameter = _param;
        //     return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        // }

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
            this.Back(_transition, _openParam, _closeParam);
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

    public enum PageTransition{
        Sequance,
        Spawn
    }

    public interface IPage : IPageController
    {
        Task OnOpenedPage(IPage _fromPage, object _param = null);
        Task OnClosedPage(IPage _fromUI, object _param = null);
    }

    public interface IPageController{

    }

    static public class PageManagerExtensions{
        public static PageManager GetPageManager(this IPageController _page){
            return PageManager.Instance;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using BicUtil.SingletonBase;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.PageService
{
    public class PageManager : MonoBehaviourSingleton<PageManager>, ISingleton{
        private Stack<PageController> sceneStack = new Stack<PageController>();
        private object sceneTransitionParameter = null;
        public PageController CurrentController{
            get=>this.sceneStack.Peek();
        }

        static public void LoadScene(string _sceneName){
            UnityEngine.SceneManagement.SceneManager.LoadScene (_sceneName);
        }

        public object PopSceneTransitionParameter(){
            var _result = sceneTransitionParameter;
            sceneTransitionParameter = null;
            return _result;
        }

        public void OnCreatedSingleton(){
            this.gameObject.name = "PageManager";
        }

        public async Task<AsyncOperation> SceneReplaceAsync(string _sceneName, object _openParam = null){
            this.sceneTransitionParameter = _openParam;
            var _currentPageController = sceneStack.Pop();
            var _currentScene = SceneManager.GetActiveScene();
            var _nextScene = SceneManager.GetSceneByName(_sceneName);
            var _result = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            await _result;
            await SceneManager.UnloadSceneAsync(_currentScene);
            return _result;
        }

        public async Task<AsyncOperation> SceneEnterAsync(string _sceneName, object _openParam = null){
            this.sceneTransitionParameter = _openParam;
            var _currentScene = SceneManager.GetActiveScene();
            var _nextScene = SceneManager.GetSceneByName(_sceneName);
            var _result = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            setGameObjectsActive(_currentScene.GetRootGameObjects(), false);
            await _result;
            return _result;
            
        }
        
        public async Task SceneBackAsync(object _openParam = null, object _closeParam = null){
            var _pageController = sceneStack.Pop();
            var _nextPageController = sceneStack.Peek();
            var _currentSceneName = _pageController.SceneName;
            var _currentPageType = _pageController.CurrentPage.GetType();

            await _pageController.CurrentPage.OnClosedPage(new ScenePage(_nextPageController.SceneName, _nextPageController.CurrentPage.GetType()), _closeParam);
            await SceneManager.UnloadSceneAsync(_pageController.SceneName);

            var _currentScene = SceneManager.GetSceneByName(_nextPageController.SceneName);
            SceneManager.SetActiveScene(_currentScene);
            setGameObjectsActive(_currentScene.GetRootGameObjects(), true);

            if(_openParam != null){
                await _nextPageController.reopenFromSceneAsync(_currentSceneName, _currentPageType, _openParam);
            }
        }

        internal void AddController(PageController _controller){
            this.sceneStack.Push(_controller);
        }

        private void setGameObjectsActive(GameObject[] objects, bool active)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].gameObject.SetActive(active);
            }
        }


        public void LogScene(){
            Debug.Log("[PageManager] LogPage - start");
            foreach(var _v in sceneStack){
                Debug.Log(_v.SceneName);
            }
            Debug.Log("[PageManager] LogPage - finish");
        }
    }

    public enum PageTransition{
        Sequance,
        Spawn
    }
}

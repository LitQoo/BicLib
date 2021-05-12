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

        public object PopSceneTransitionParameter(){
            var _result = sceneTransitionParameter;
            sceneTransitionParameter = null;
            return _result;
        }

        public void OnCreatedSingleton(){
            this.gameObject.name = "PageManager";
        }

        public AsyncOperation SceneReplaceAsync(string _sceneName, object _param = null){
            this.sceneTransitionParameter = _param;
            return SceneManager.LoadSceneAsync(_sceneName);
        }

        public AsyncOperation SceneEnterAsync(string _sceneName, object _param = null){
            this.sceneTransitionParameter = _param;
            var _currentScene = SceneManager.GetActiveScene();
            var _nextScene = SceneManager.GetSceneByName(_sceneName);
            setGameObjectsActive(_currentScene.GetRootGameObjects(), false);
            var _result = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            return _result;
            
        }
        
        public async Task SceneBackAsync(){
            var _pageController = sceneStack.Pop();
            await SceneManager.UnloadSceneAsync(_pageController.SceneName);
            var _currentScene = SceneManager.GetActiveScene();
            setGameObjectsActive(_currentScene.GetRootGameObjects(), true);
        }

        public void AddController(PageController _controller){
            this.sceneStack.Push(_controller);
        }

        private void setGameObjectsActive(GameObject[] objects, bool active)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].gameObject.SetActive(active);
            }
        }
    }

    public enum PageTransition{
        Sequance,
        Spawn
    }
}

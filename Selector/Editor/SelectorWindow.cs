using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BicUtil.Selector{
    public class SelectorWindow : EditorWindow
    {
        
        private HashSet<string> savedPatternList = new HashSet<string>();
        private string searchPattern = "";
        private Vector2 scrollPosition;
        private int selectedObjectIndex = -1;

        [MenuItem("BicLib/Selector", false, 501)]
        public static void ShowWindow(){
            SelectorWindow _window = GetWindow<SelectorWindow>("Selector");
            UnityEngine.Object.DontDestroyOnLoad(_window);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
        }
        
        private void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        private void OnSceneGUI(SceneView _view)
        {
            for (int i = 0; i < foundObjects.Length; i++)
            {
                GameObject obj = foundObjects[i];
                DrawBoxAroundGameObject(obj);
            }
        }

        GameObject[] foundObjects = new GameObject[]{};

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Search Pattern:");


            GUI.SetNextControlName("SearchPattern");
            searchPattern = EditorGUILayout.TextField(searchPattern);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                Search();

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    Event.current.Use(); // 이벤트 사용 처리
                }

            }

            if (GUILayout.Button(".$#",GUILayout.Width(50f))){
                EditorGUI.FocusTextInControl("");

                SelectingClass[] selectingClass = FindObjectsOfType<SelectingClass>();
                HashSet<string> classNames = new HashSet<string>();
                foreach(var sc in selectingClass){
                    classNames.UnionWith(sc.Classes);
                }

                GenericMenu menu = new GenericMenu();
                foreach(var name in classNames){
                    var _name = "."+name;
                    menu.AddItem(new GUIContent(_name), false, selectName, _name);
                }

                menu.AddSeparator("");

                if(foundObjects.Length > 0){
                    HashSet<string> componentNames = new HashSet<string>();
                    foreach(var foundObject in foundObjects){
                        var components = foundObject.GetComponents<Component>().Select(comp=>{
                            var _name = comp.GetType().ToString(); 
                            if(_name.Contains(".") == true){
                                _name = _name.Split(".").Last();
                            }
                            return _name;
                        });
                        componentNames.UnionWith(components);
                    }

                    foreach(var name in componentNames){
                        var _name = "$"+name;
                        menu.AddItem(new GUIContent(_name), false, selectName, _name);
                    }
                }

                menu.ShowAsContext();
            }


            
            GUILayout.EndHorizontal();


            GUILayout.Label("Selected " + foundObjects.Length + " objects");

            if(string.IsNullOrEmpty(searchPattern) == false){
                if(savedPatternList.Contains(searchPattern) == false){
                    if(GUILayout.Button("Save pattern")){
                        this.savedPatternList.Add(searchPattern);
                    }
                }else{
                    if(GUILayout.Button("Delete pattern")){
                        this.savedPatternList.Remove(searchPattern);
                    }
                }
            }


            foreach(var _savedPattern in this.savedPatternList){

                GUILayout.BeginHorizontal();
                GUILayout.Label(_savedPattern);
                if(GUILayout.Button("@",GUILayout.Width(30f))){
                    this.searchPattern = _savedPattern;
                    Search();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        private void Search()
        {
            foundObjects = FindGameObjectsWithPattern(searchPattern);
            Selection.objects = foundObjects;
            ShowSelectedObjectsInHierarchy();

            EditorApplication.ExecuteMenuItem("BicLib/Selector");
            EditorGUI.FocusTextInControl("SearchPattern");
        }

        private void selectName(object _object)
        {
            string _name = (string)_object;
            if(searchPattern.Length > 1 && searchPattern.EndsWith("/") == false){
                searchPattern += "/";
            }

            searchPattern += _name;
            
            EditorApplication.delayCall += Repaint;
        }

        private void ShowSelectedObjectsInHierarchy()
        {
            var hierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(hierarchyWindowType);
            var setExpandedRecursive = hierarchyWindowType.GetMethod("SetExpandedRecursive");
           
            foreach (var gameObject in Selection.gameObjects)
            {
                var parent = gameObject.transform.parent;
                while (parent != null)
                {
                    setExpandedRecursive.Invoke(hierarchyWindow, new object[] { parent.gameObject.GetInstanceID(), true });
                    parent = parent.parent;
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        private void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            // GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            // if (gameObject != null && foundObjects.Contains(gameObject))
            // {
            //     Rect labelRect = new Rect(selectionRect.x + 16f, selectionRect.y + 1, selectionRect.width, selectionRect.height);
            //     EditorGUI.LabelField(labelRect, new string('_', gameObject.name.Length), GetHierarchyNameStyle());
            // }
        }

        private GUIStyle GetHierarchyNameStyle()
        {
            GUIStyle style = new GUIStyle("Label");
            style.normal.textColor = Color.red;
            return style;
        }

        private GameObject[] FindGameObjectsWithPattern(string pattern)
        {
            string[] searchTerms = pattern.Split('/');
            string firstTerm = searchTerms[0];

            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            HashSet<GameObject> foundObjects = new HashSet<GameObject>();

            foreach (GameObject rootObject in rootObjects)
            {
                SearchGameObject(rootObject.transform, firstTerm, searchTerms, 1, foundObjects, true);
            }

            return foundObjects.ToArray();
        }

        private void SearchGameObject(Transform parent, string searchTerm, string[] searchTerms, int termIndex, HashSet<GameObject> foundObjects, bool searchNext)
        {
            if(parent.childCount == 0){
                if(searchTerm.StartsWith("$") == true){
                    string namePattern = searchTerm.Substring(1);
                    if(parent.GetComponent(namePattern) != null){
                        if(termIndex >= searchTerms.Length){
                            foundObjects.Add(parent.gameObject);
                        }else{
                            SearchGameObject(parent, searchTerms[termIndex], searchTerms, termIndex + 1, foundObjects, false);
                        }
                    }
                }

                return;
            }

            foreach (Transform child in parent)
            {
                if(searchTerm == "*"){
                    SearchGameObject(child, searchTerms[termIndex], searchTerms, termIndex + 1, foundObjects, false);
                    continue;
                }

                var gameObject = IsSearchTermMatch(child.gameObject, searchTerm);
                if (gameObject != null)
                {
                    
                    if (termIndex >= searchTerms.Length)
                    {
                        foundObjects.Add(gameObject);

                        if( searchNext == true && gameObject.transform.childCount > 0){
                            SearchGameObject(child, searchTerm, searchTerms, termIndex, foundObjects, searchNext);
                        }
                    }
                    else
                    {
                        SearchGameObject(gameObject.transform, searchTerms[termIndex], searchTerms, termIndex + 1, foundObjects, false);
                    }
                }
                else if(searchNext == true)
                {
                    SearchGameObject(child, searchTerm, searchTerms, termIndex, foundObjects, searchNext);
                }
            }
        }

        private GameObject IsSearchTermMatch(GameObject gameObject, string searchTerm)
        {
            if(searchTerm.StartsWith("~") == true){
                string namePattern = searchTerm.Substring(1);
                Regex regex = new Regex(namePattern);
                return regex.IsMatch(gameObject.name) == true ? gameObject : null;
            }else if(searchTerm.StartsWith("#") == true){
                string namePattern = searchTerm.Substring(1);
                return gameObject.CompareTag(namePattern) == true ? gameObject : null;
            }else if(searchTerm.StartsWith("$") == true){
                string namePattern = searchTerm.Substring(1);
                return gameObject.transform.parent.GetComponent(namePattern) != null ? gameObject.transform.parent.gameObject : null;
            }else if(searchTerm.StartsWith(".") == true){
                string namePattern = searchTerm.Substring(1);
                var selectingClass = gameObject.GetComponent<SelectingClass>();
                
                if(selectingClass != null){
                    return selectingClass.HasClass(namePattern) == true ? gameObject : null;

                }else{
                    return null;
                }
            }else{
                return gameObject.name.Equals(searchTerm) == true ? gameObject : null;
            }
        }

        private void DrawBoxAroundGameObject(GameObject gameObject)
        {
            Bounds bounds = GetBounds(gameObject);
            
            Handles.DrawSolidRectangleWithOutline(
                new Rect(bounds.center, bounds.size),
                Color.clear, Color.green
            );
        }

        private Bounds GetBounds(GameObject gameObject)
        {

            RectTransform trasnform = gameObject.GetComponent<RectTransform>();
            if(trasnform != null){
                return new Bounds((Vector2)trasnform.position - trasnform.sizeDelta * trasnform.localScale * trasnform.pivot, trasnform.sizeDelta * trasnform.localScale);
            }

            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds;
            }

            Collider collider = gameObject.GetComponent<Collider>();
            if(collider != null){
                return collider.bounds;
            }

            // 렌더러나 콜라이더가 없을 경우에는 Transform을 기준으로 Bounds를 계산합니다.
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer childRenderer in renderers)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }

            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider childCollider in colliders)
            {
                bounds.Encapsulate(childCollider.bounds);
            }

            return bounds;
        }

    }
}
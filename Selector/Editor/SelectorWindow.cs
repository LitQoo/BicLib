using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BicUtil.Selector{
    public class SelectorWindow : EditorWindow
    {
        
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


            if (GUILayout.Button("Select") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                foundObjects = FindGameObjectsWithPattern(searchPattern);
                Selection.objects = foundObjects;
                ShowSelectedObjectsInHierarchy();
                
                EditorApplication.ExecuteMenuItem("BicLib/Selector");
                EditorGUI.FocusTextInControl("SearchPattern");

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    Event.current.Use(); // 이벤트 사용 처리
                }
                
            }

            GUILayout.Label("Selected " + foundObjects.Length + " objects");


            GUILayout.EndVertical();

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
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
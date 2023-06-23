using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;
using BicDB.Container;
using BicDB;
using BicUtil.Json;
using UnityEditor.SceneManagement;
using BicDB.Variable;

namespace BicUtil.Selector{
    public class SelectorWindow : EditorWindow
    {
        private HashSet<string> savedPatternList = new HashSet<string>();
        private MutableDictionaryContainer modifiedPathInfo = new MutableDictionaryContainer();
        private MutableDictionaryContainer groups = new MutableDictionaryContainer();
        private GameObject[] selectedObjects = new GameObject[]{};

        private string searchInput = "";
        private string targetInput = "";
        private string targetPattern{
            get=>targetInput;
            set{
                searchInput = value;
                targetInput = value;
            }
        }
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

        private void checkDeselected()
        {
            if(targetPattern != "" && selectedObjects.Length > 0 && selectedObjects.Length != Selection.objects.Length){
                selectedObjects = new GameObject[]{};
                targetPattern = "";
                trackingTargetObject = null;
                this.Repaint();
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        private void OnSceneGUI(SceneView _view)
        {
            drawBoxAroundGameObject();
            checkDeselected();
        }
        private void OnGUI()
        {

            if(string.IsNullOrEmpty(groupTargetPath) == false){
                drawSetGroup();
                return;
            }

            drawSearchUI();
            GUILayout.Space(10f);
            drawModifyTracking();
            GUILayout.Space(10f);
            drawSavedPattern();

        }

        #region backup seri
        private TextAsset targetTextAsset;
        private GameObject trackingTargetObject;
        private Component[] components;
        private Dictionary<string, string> originComponentData;
        Vector2 modifiedScroll = new Vector2(0, 0);
        /*
        백업한다 -> targetObject의 컴포넌트들을 json으로 변경해놓음
        값 변경을 감지한다-> targetObject 컴포넌트들의 json값과 백업된 값을 비교
        변경된 값을 저장한다 (modifedList?) 이때 딱 변경된 값만 저장하고 백업본도 업데이트함.
        스타일 저장시 modifedList를 저장한다. 

        스타일로드시 modifedList에 미리 채워준다

        필요한 함수.
        json구조에서 지정한 필드 빼고 모두 삭제하는 기능 (추후 merge용도)
        */

        private void drawTxtAsset(){
            GUILayout.Label("SavingFile:");
            targetTextAsset = (TextAsset)EditorGUILayout.ObjectField(targetTextAsset, typeof(TextAsset), false);

            if(targetTextAsset == null){
                return;
            }


            GUILayout.BeginHorizontal();
            if (targetTextAsset != null && GUILayout.Button("Load")){
                this.modifiedPathInfo.ParseJson(targetTextAsset.text);
            }

            if (modifiedPathInfo.Count > 0 && GUILayout.Button("Apply")){
                //현재 선택된 오브젝트들의 공통 컴포넌트의 같은 값을 가져와 modifiedProp..에 넣기
                foreach(var _path in this.modifiedPathInfo.Keys){
                    var _data = this.modifiedPathInfo[_path].ToString();
                    var _objects = FindGameObjectsWithPattern(_path);
                    var _split = _path.Split("$");
                    var _componentName = _split[_split.Length - 1];
                    foreach(var _object in _objects){
                        var _component = _object.GetComponent(_componentName);
                        EditorJsonUtility.FromJsonOverwrite(_data, _component);
                        EditorUtility.SetDirty(_component);
                        _object.SetActive(!_object.activeSelf);
                        _object.SetActive(!_object.activeSelf);
                    }
                }

                Debug.Log("Applied");
            }


            if (modifiedPathInfo.Count > 0 && GUILayout.Button("Save")){

                string _path = AssetDatabase.GetAssetPath(targetTextAsset);
                // Save the modified content back to the asset file
                var _content = modifiedPathInfo.ToString();
                System.IO.File.WriteAllText(_path, _content);
                AssetDatabase.Refresh();
                Debug.Log("saved " + _content);
            }
            GUILayout.EndHorizontal();
        }

        private void drawModifyTracking()
        {
            drawTxtAsset();
            drawCustomInspector();
            setupTracking();
            drawModified();
        }

        private void drawModified()
        {
            if (modifiedPathInfo.Count <= 0)
            {
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label("Modified " + modifiedPathInfo.Count + " Properties");

            modifiedScroll = EditorGUILayout.BeginScrollView(modifiedScroll);

            EditorGUI.indentLevel++;

            foreach (var _path in modifiedPathInfo)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label(_path.Key);

                if (GUILayout.Button("Select", GUILayout.Width(50f)))
                {
                    var _parentPath = _path.Key.Split("$")[0];
                    search(_parentPath.Remove(_parentPath.Length - 1));
                }

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                {
                    var _target = _path.Value as MutableDictionaryContainer;
                    var _keyPaths = _target.GetKeyPath();
                    GenericMenu menu = new GenericMenu();
                    
                    //groups
                    foreach (var _keyPath in _keyPaths)
                    {
                        menu.AddItem(new GUIContent("Groups " + _keyPath), false, () =>
                        {
                            groupTargetPath = _path.Key + "." + _keyPath;
                        });
                    }

                    menu.AddItem(new GUIContent("Remove this"), false, () =>
                    {
                        modifiedPathInfo.Remove(_path.Key);
                    });


                    //remove
                    foreach (var _keyPath in _keyPaths)
                    {
                        menu.AddItem(new GUIContent("Remove " + _keyPath), false, () =>
                        {
                            _target.RemoveByKeyPath(_keyPath.Split('.'));
                        });
                    }

                    menu.ShowAsContext();

                }
                GUILayout.EndHorizontal();
                EditorGUILayout.TextField(_path.Value.ToString());
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }

        private void setupTracking()
        {
            GameObject _selectedObject = null;
            if (selectedObjects.Length > 0)
            {
                _selectedObject = selectedObjects[0];
            }

            if (_selectedObject != null)
            {
                if (_selectedObject != trackingTargetObject)
                {
                    trackingTargetObject = _selectedObject;
                    backupComponentValues();
                }

                if (originComponentData != null)
                {
                    bool _hasDiff = false;
                    foreach (var _key in originComponentData.Keys.ToArray())
                    {
                        var _componentName = _key;
                        if (_componentName.Contains(".") == true)
                        {
                            _componentName = _componentName.Split(".").Last();
                        }

                        var _currentComponent = trackingTargetObject.GetComponent(_componentName);
                        var _currentData = EditorJsonUtility.ToJson(_currentComponent);

                        if (originComponentData[_key] != _currentData)
                        {
                            var _origin = new MutableDictionaryContainer();
                            _origin.ParseJson(originComponentData[_key]);
                            var _modified = new MutableDictionaryContainer();
                            _modified.ParseJson(_currentData);
                            var _diff = _origin.GetDiff(_modified, -1);
                            var _path = targetPattern + "/$" + _componentName;
                            if (modifiedPathInfo.ContainsKey(_path) == false)
                            {
                                modifiedPathInfo[_path] = _diff;
                            }
                            else
                            {
                                var _record = (modifiedPathInfo[_path] as MutableDictionaryContainer);
                                _record.MergeCopyBy(_diff);
                            }

                            originComponentData[_key] = _currentData;
                            _hasDiff = true;
                        }
                    }

                    if (_hasDiff == true)
                    {
                        this.savedPatternList.Add(targetPattern);
                    }
                }
            }
            else
            {
                trackingTargetObject = null;
            }
        }

        private IDataBase findValue(IDictionary<string, IDataBase> _record, string _path){
            var _target = _record[_record.Keys.ElementAt(0)] as IDictionary<string, IDataBase>;

            string[] _pathList = null;
            if(_path.Contains(".") == false){
                _pathList = new string[]{_path};
            }else{
                _pathList = _path.Split(".");
            }

            for(int i = 0; i < _pathList.Length; i++){
                foreach(var _key in _target.Keys.ToArray()){
                    if(_key != _pathList[i]){
                        _target.Remove(_key);
                    } 
                }

                if(i == _pathList.Length - 1){
                    return _target[_pathList[i]];
                }

                _target = _target[_pathList[i]] as IDictionary<string, IDataBase>;
            }

            return null;
            
        }

        
        private void backupComponentValues()
        {
            Debug.Log("backup!");
            components = trackingTargetObject.GetComponents<Component>();
            originComponentData = new Dictionary<string, string>();
            
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null)
                {
                    originComponentData[component.GetType().ToString()] = EditorJsonUtility.ToJson(component);
                }
            }
        }

        #endregion
        private void drawSavedPattern()
        {
            GUILayout.BeginVertical();

            if (string.IsNullOrEmpty(targetPattern) == false)
            {
                if (savedPatternList.Contains(targetPattern) == false)
                {
                    if (GUILayout.Button("Save pattern " + targetPattern))
                    {
                        this.savedPatternList.Add(targetPattern);
                    }
                }
                else
                {
                    if (GUILayout.Button("Delete pattern " + targetPattern))
                    {
                        this.savedPatternList.Remove(targetPattern);
                    }
                }
            }


            foreach (var _savedPattern in this.savedPatternList)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label(_savedPattern);
                if (GUILayout.Button("Select", GUILayout.Width(50f)))
                {
                    search(_savedPattern);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void drawSearchUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Search Pattern:");


            GUI.SetNextControlName("SearchPattern");
            searchInput = EditorGUILayout.TextField(searchInput);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                search(searchInput);

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    Event.current.Use(); // 이벤트 사용 처리
                }

            }

            if (GUILayout.Button(".$#", GUILayout.Width(50f)))
            {
                EditorGUI.FocusTextInControl("");
                HashSet<string> _classNames = getClassNamesUsed();

                GenericMenu menu = new GenericMenu();
                foreach (var name in _classNames)
                {
                    var _name = "." + name;
                    menu.AddItem(new GUIContent(_name), false, selectName, _name);
                }

                menu.AddSeparator("");

                if (selectedObjects.Length > 0)
                {
                    HashSet<string> _componentNames = getComponentNamesInSelectedObjects(selectedObjects);

                    foreach (var name in _componentNames)
                    {
                        var _name = "$" + name;
                        menu.AddItem(new GUIContent(_name), false, selectName, _name);
                    }
                }

                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            if(selectedObjects.Length > 0){
                GUILayout.Label("Selected " + selectedObjects.Length + " objects");
            }

            GUILayout.EndVertical();
        }

        private HashSet<string> getComponentNamesInSelectedObjects(GameObject[] _targetObjects)
        {
            HashSet<string> _componentNames = new HashSet<string>();
            foreach (var _object in _targetObjects)
            {
                var components = _object.GetComponents<Component>().Select(comp =>
                {
                    var _name = comp.GetType().ToString();
                    if (_name.Contains(".") == true)
                    {
                        _name = _name.Split(".").Last();
                    }
                    return _name;
                });
                _componentNames.UnionWith(components);
            }

            return _componentNames;
        }

        private static HashSet<string> getClassNamesUsed()
        {
            SelectingClass[] _selectingClass = FindObjectsOfType<SelectingClass>();
            HashSet<string> _classNames = new HashSet<string>();
            foreach (var sc in _selectingClass)
            {
                _classNames.UnionWith(sc.Classes);
            }

            return _classNames;
        }

        private void search(string _pattern)
        {
            targetPattern = _pattern;
            selectedObjects = FindGameObjectsWithPattern(_pattern);
            Selection.objects = selectedObjects;
            ShowSelectedObjectsInHierarchy();

            EditorApplication.ExecuteMenuItem("BicLib/Selector");
            EditorGUI.FocusTextInControl("SearchPattern");
        }

        private void selectName(object _object)
        {
            string _name = (string)_object;
            if(searchInput.Length > 1 && searchInput.EndsWith("/") == false){
                searchInput += "/";
            }

            searchInput += _name;
            
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

                if(searchTerm == ".."){
                    SearchGameObject(parent.parent, searchTerms[termIndex], searchTerms, termIndex + 1, foundObjects, false);
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

        private void drawBoxAroundGameObject()
        {
            foreach (var _gameObject in selectedObjects)
            {
                Bounds _bounds = getBounds(_gameObject);
                Handles.DrawSolidRectangleWithOutline(
                    new Rect(_bounds.center - _bounds.size / 2f, _bounds.size),
                    Color.clear, Color.green
                );
            }
        }

        private Bounds getBounds(GameObject _gameObject)
        {

            RectTransform _trasnform = _gameObject.GetComponent<RectTransform>();
            if(_trasnform != null){
                Vector3 _worldPosition = _trasnform.TransformPoint(_trasnform.rect.center);
                Vector2 _size = Vector2.Scale(_trasnform.rect.size, _trasnform.lossyScale); 

                return new Bounds((Vector2)_worldPosition, _size);
            }

            Renderer _renderer = _gameObject.GetComponent<Renderer>();
            if (_renderer != null)
            {
                return _renderer.bounds;
            }

            Collider _collider = _gameObject.GetComponent<Collider>();
            if(_collider != null){
                return _collider.bounds;
            }

            // 렌더러나 콜라이더가 없을 경우에는 Transform을 기준으로 Bounds를 계산합니다.
            Bounds _bounds = new Bounds(_gameObject.transform.position, Vector3.zero);
            Renderer[] _renderers = _gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer _childRenderer in _renderers)
            {
                _bounds.Encapsulate(_childRenderer.bounds);
            }

            Collider[] _colliders = _gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider _childCollider in _colliders)
            {
                _bounds.Encapsulate(_childCollider.bounds);
            }

            return _bounds;
        }

        #region DrawCustomInspector 
        private string groupTargetPath = "";
        private string groupTargetName = "";

        private void drawCustomInspector()
        {
            if(groups.Count <=0){
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label("Custom Inspector");

            foreach(var _groupInfo in groups){
                var _groupPaths = (_groupInfo.Value as ListContainer<StringVariable>);
                var _targetPathWithProperty = _groupPaths[0].AsString;
                var _pathInfo = splitPath(_targetPathWithProperty);
                int dotIndex = _pathInfo.PropertyPath.IndexOf(".");
                var _pathWithoutRoot = _pathInfo.PropertyPath.Substring(dotIndex + 1);

                GUILayout.BeginHorizontal();
                GUILayout.Label(_groupInfo.Key);
                var _objects = FindGameObjectsWithPattern(_pathInfo.Path);
                var _component = _objects[0].GetComponent(_pathInfo.Component);
                var _selectedObject = new SerializedObject(_component);
                var _property = getSerializedPropertyByPath(_selectedObject, _pathWithoutRoot);
                EditorGUILayout.PropertyField(_property);
                if(_selectedObject.hasModifiedProperties == true){
                    
                    _selectedObject.ApplyModifiedProperties();
                    foreach(var _targetPath in _groupPaths){
                        Debug.Log("modifyed");
                        //1. diff만들기
                        //2. 타겟오브젝트에 넣기
                        var _componentJson = EditorJsonUtility.ToJson(_component);
                        Debug.Log(_pathInfo.PropertyPath);
                        Debug.Log(_componentJson);
                        var _value = getDataByPath(_componentJson, _pathInfo.PropertyPath);
                        var __pathInfo = splitPath(_targetPath.AsString);
                        var __objects = FindGameObjectsWithPattern(__pathInfo.Path);
                        foreach(var _object in __objects){
                            var _componentObject = _object.GetComponent(__pathInfo.Component);
                            EditorJsonUtility.FromJsonOverwrite(_value,_componentObject);
                            EditorUtility.SetDirty(_componentObject);
                            _object.SetActive(!_object.activeSelf);
                            _object.SetActive(!_object.activeSelf);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private string getDataByPath(string _json, string _propertyPath){
            var _data = new MutableDictionaryContainer();
            _data.ParseJson(_json);
            var _paths = _propertyPath.Split(".");
            var _target = _data;
            for(int i = 0; i < _paths.Length; i++){
                var _fieldName = _paths[i];
                foreach(var _key in _target.Keys.ToArray()){
                    if(_key != _fieldName){
                        _target.Remove(_key);
                    }
                }
                
                _target = _target[_fieldName] as MutableDictionaryContainer;
                if(_target == null){
                    break;
                }
            }

            return _data.ToString();
        }

        private SerializedProperty getSerializedPropertyByPath(SerializedObject _target, string _path){
            var _paths = _path.Split(".");
            var _result = _target.FindProperty(_paths[0]);

            if(_paths.Length > 1){
                for(int i = 1; i< _paths.Length; i++){
                    _result = _result.FindPropertyRelative(_paths[i]);
                }
            }

            return _result;
        }

        private (string Path, string Component, string PropertyPath) splitPath(string _targetPathWithProperty)
        {
            int _sIndex =_targetPathWithProperty.LastIndexOf('$');
            var _path = _targetPathWithProperty.Substring(0, _sIndex - 1);
            var input = _targetPathWithProperty.Substring(_sIndex + 1);
            int dotIndex = input.IndexOf(".");
            string _componentName = input.Substring(0, dotIndex);
            string _propertyPath = input.Substring(dotIndex + 1);
            return (_path, _componentName, _propertyPath);
        }

        private void drawSetGroup()
        {

            GUILayout.Label(groupTargetPath);
            GUILayout.Label("GroupName : ");
            groupTargetName = EditorGUILayout.TextField(groupTargetName);
            GUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(groupTargetName));
            if (GUILayout.Button("OK")){
                if(this.groups.ContainsKey(groupTargetName) == false){
                    this.groups[groupTargetName] = new ListContainer<StringVariable>();    
                }

                (this.groups[groupTargetName] as ListContainer<StringVariable>).Add(new StringVariable(groupTargetPath));
                
                groupTargetPath = "";
                groupTargetName = "";
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Cancel")){
                groupTargetPath = "";
                groupTargetName = "";
            }
            GUILayout.EndHorizontal();
        }
        #endregion

    }
}
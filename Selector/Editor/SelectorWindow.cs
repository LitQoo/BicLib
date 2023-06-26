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
        private Style style = new Style();
        private HashSet<string> savedPatternList = new HashSet<string>();
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

            if(string.IsNullOrEmpty(inspectTargetPath) == false){
                drawSetInspector();
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
                this.style = new Style();
                if(targetTextAsset.text.Length > 0){
                    this.style.ParseJson(targetTextAsset.text);
                }
            }

            if (style.ModifiedPathInfo.Count > 0 && GUILayout.Button("Apply")){
                //현재 선택된 오브젝트들의 공통 컴포넌트의 같은 값을 가져와 modifiedProp..에 넣기
                foreach(var _path in style.ModifiedPathInfo.Keys){
                    var _data = style.ModifiedPathInfo[_path].ToString();
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


            if (style.ModifiedPathInfo.Count > 0 && GUILayout.Button("Save")){

                string _path = AssetDatabase.GetAssetPath(targetTextAsset);
                // Save the modified content back to the asset file
                var _content = style.ToString();
                System.IO.File.WriteAllText(_path, _content);
                AssetDatabase.Refresh();
                Debug.Log("saved2 " + _content);
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
            if (style.ModifiedPathInfo.Count <= 0)
            {
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label("Modified " + style.ModifiedPathInfo.Count + " Properties");

            modifiedScroll = EditorGUILayout.BeginScrollView(modifiedScroll);

            EditorGUI.indentLevel++;

            foreach (var _path in style.ModifiedPathInfo)
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
                    _keyPaths.RemoveAt(0);
                    menu.AddSeparator("Inspect");
                    //groups
                    foreach (var _keyPath in _keyPaths)
                    {
                        var _inspectTarget = _path.Key + "." + _keyPath;
                        if(isInInspector(_inspectTarget) == false){
                            menu.AddItem(new GUIContent(_keyPath), false, () =>
                            {
                                inspectTargetPath = _inspectTarget;
                            });
                        }
                    }


                    menu.AddSeparator("Remove");

                    menu.AddItem(new GUIContent("this"), false, () =>
                    {
                        style.ModifiedPathInfo.Remove(_path.Key);
                    });

                    //remove
                    foreach (var _keyPath in _keyPaths)
                    {
                        menu.AddItem(new GUIContent(_keyPath), false, () =>
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
                            _origin.ParseJson(originComponentData[_key], false);
                            var _modified = new MutableDictionaryContainer();
                            _modified.ParseJson(_currentData, false);
                            var _diff = _origin.GetDiff(_modified, -1);
                            var _path = targetPattern + "/$" + _componentName;
                            if (style.ModifiedPathInfo.ContainsKey(_path) == false)
                            {
                                style.ModifiedPathInfo[_path] = _diff;
                            }
                            else
                            {
                                var _record = (style.ModifiedPathInfo[_path] as MutableDictionaryContainer);
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

        private IDataBase findValue(string _json, string _path){
            MutableDictionaryContainer _record = new MutableDictionaryContainer();
            _record.ParseJson(_json, false);
            var _target = _record as IDictionary<string, IDataBase>;

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
                HashSet<string> _tags = getUsedTags();

                GenericMenu menu = new GenericMenu();

                menu.AddSeparator("#SelectingTag");
                foreach (var name in _tags)
                {
                    var _name = "#" + name;
                    menu.AddItem(new GUIContent(_name + " ㅤ"), false, addObjectNameToSearchInput, _name);
                }


                if (selectedObjects.Length > 0)
                {

                    menu.AddSeparator("$Component");
                    HashSet<string> _componentNames = getComponentNamesInSelectedObjects(selectedObjects);

                    foreach (var name in _componentNames)
                    {
                        var _name = "$" + name;
                        menu.AddItem(new GUIContent(_name), false, addObjectNameToSearchInput, _name);
                    }


                    menu.AddSeparator("Child");

                    HashSet<string> _childNames = getChildNamesInSelectedObjects(selectedObjects);

                    foreach (var name in _childNames)
                    {
                        var _name =name;
                        menu.AddItem(new GUIContent(_name), false, addObjectNameToSearchInput, _name);
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
        private HashSet<string> getChildNamesInSelectedObjects(GameObject[] _targetObjects)
        {
            HashSet<string> _componentNames = new HashSet<string>();
            foreach (var _object in _targetObjects)
            {
                for(int i = 0; i < _object.transform.childCount; i++){
                    var _child = _object.transform.GetChild(i);
                    _componentNames.Add(_child.name);
                }
            }

            return _componentNames;
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

        private static HashSet<string> getUsedTags()
        {
            SelectingTag[] _selectingTag = FindObjectsOfType<SelectingTag>();
            HashSet<string> _tags = new HashSet<string>();
            foreach (var sc in _selectingTag)
            {
                _tags.UnionWith(sc.Tags);
            }

            return _tags;
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

        private void addObjectNameToSearchInput(object _object)
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
                    string _component = searchTerm.Substring(1);
                    if(parent.GetComponent(_component) != null){
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
            }else if(searchTerm.StartsWith("$") == true){
                string namePattern = searchTerm.Substring(1);
                return gameObject.transform.parent.GetComponent(namePattern) != null ? gameObject.transform.parent.gameObject : null;
            }else if(searchTerm.StartsWith("#") == true){
                string namePattern = searchTerm.Substring(1);
                var selectingClass = gameObject.GetComponent<SelectingTag>();
                
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
        private string inspectTargetPath = "";
        private string inspectName = "";

        private void drawCustomInspector()
        {
            if(style.InspectorInfo.Count <=0){
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label("Custom Inspector");

            foreach(var _groupInfo in style.InspectorInfo){
                var _groupPaths = (_groupInfo.Value as MutableListContainer);
                var _targetPathWithProperty = _groupPaths[0].AsVariable.AsString;
                var _pathInfo = splitPath(_targetPathWithProperty);
                int dotIndex = _pathInfo.PropertyPath.IndexOf(".");
                var _pathWithoutRoot = _pathInfo.PropertyPath.Substring(dotIndex + 1);

                GUILayout.BeginHorizontal();
                if(GUILayout.Button(_groupInfo.Key)){
                    GenericMenu menu = new GenericMenu();
                    var _key = _groupInfo.Key;
                    menu.AddSeparator("Remove");
                    menu.AddItem(new GUIContent("this"), false, () =>
                    {
                        style.InspectorInfo.Remove(_key);
                        Repaint();
                    });

                    for(int i = 0; i < _groupPaths.Count; i++){
                         var _index = i;
                         menu.AddItem(new GUIContent(_groupPaths[_index].AsVariable.AsString.Replace("/", "\\") + " ㅤ"), false, () =>
                         {
                             _groupPaths.RemoveAt(_index);
                             Repaint();
                         });
                    }


                    menu.ShowAsContext();
                }

                var _objects = FindGameObjectsWithPattern(_pathInfo.Path);
                if(_objects.Length <= 0){
                    GUILayout.EndHorizontal();
                    continue;
                }

                var _component = _objects[0].GetComponent(_pathInfo.Component);
                var _selectedObject = new SerializedObject(_component);
                var _property = getSerializedPropertyByPath(_selectedObject, _pathWithoutRoot);
                EditorGUILayout.PropertyField(_property, new GUIContent(""), true);
                if(_selectedObject.hasModifiedProperties == true){
                    _selectedObject.ApplyModifiedProperties();
                    var _componentJson = EditorJsonUtility.ToJson(_component);
                    var _value = findValue(_componentJson, _pathInfo.PropertyPath);

                    foreach(var _targetPath in _groupPaths){
                        var __pathInfo = splitPath(_targetPath.AsVariable.AsString);
                        var __objects = FindGameObjectsWithPattern(__pathInfo.Path);
                        var _record = new MutableDictionaryContainer();
                        _record.SetValueByKeyPath(_value, __pathInfo.PropertyPath.Split("."));
                        var _jsonForComponent =  _record.ToString();
                        
                        foreach(var _object in __objects){
                            var _componentObject = _object.GetComponent(__pathInfo.Component);
                            EditorJsonUtility.FromJsonOverwrite(_jsonForComponent,_componentObject);
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
            _data.ParseJson(_json, false);
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

        private void drawSetInspector()
        {
            // 여기서
            // 저장된 inspecrtor 서치패턴 돌면서
            // 서치패턴의 첫번째 오브젝트 json데이터에서 value만 뽑아온다음
            // 인스펙터타겟의 value와 json 형태(value말고 field명이 유사한지 혹은 데이터타입이 유사한지(string? int?)) 검사해서
            // 이미 존재하는 inspector를 추천해주기



            GUILayout.Label(inspectTargetPath);
            GUILayout.Label("InspectorName : ");
            inspectName = EditorGUILayout.TextField(inspectName);


            var _selectedSample = getSampleValue(inspectTargetPath);

            GUILayout.Label("recommend : ");
            foreach (var _inspectorInfo in style.InspectorInfo)
            {
                var _paths = _inspectorInfo.Value as MutableListContainer;
                if (_paths.Count <= 0)
                {
                    continue;
                }

                var _sample = getSampleValue(_paths[0].AsVariable.AsString);
                if (_sample != null)
                {
                    if (_selectedSample.Type == _sample.Type)
                    {
                        if (_selectedSample is IDictionary<string, IDataBase>)
                        {
                            var _selectedDict = _selectedSample as IDictionary<string, IDataBase>;
                            var _dict = _sample as IDictionary<string, IDataBase>;
                            if (areKeysEqual(_selectedDict, _dict) == true)
                            {
                                if(GUILayout.Button(_inspectorInfo.Key)){
                                    inspectName = _inspectorInfo.Key;
                                }
                            }
                        }
                        else
                        {
                            if(GUILayout.Button(_inspectorInfo.Key)){
                                inspectName = _inspectorInfo.Key;
                            }
                        }
                    }
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(inspectName));
            if (GUILayout.Button("OK"))
            {
                if (style.InspectorInfo.ContainsKey(inspectName) == false)
                {
                    style.InspectorInfo[inspectName] = new MutableListContainer();
                }

                (style.InspectorInfo[inspectName] as MutableListContainer).Add(new StringVariable(inspectTargetPath));

                inspectTargetPath = "";
                inspectName = "";
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Cancel"))
            {
                inspectTargetPath = "";
                inspectName = "";
            }
            GUILayout.EndHorizontal();
        }

        private IDataBase getSampleValue(string _targetPath)
        {
            var _pathInfo = splitPath(_targetPath);
            var _objects = FindGameObjectsWithPattern(_pathInfo.Path);
            if(_objects.Length <= 0){
                return null;
            }

            var _component = _objects[0].GetComponent(_pathInfo.Component);
            var _json = EditorJsonUtility.ToJson(_component);
            return this.findValue(_json, _pathInfo.PropertyPath);
        }

        private bool areKeysEqual(IDictionary<string, IDataBase> dict1, IDictionary<string, IDataBase> dict2)
        {
            if (dict1 == null || dict2 == null)
            {
                return false;
            }

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (string key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
            }

            return true;
        }

        private bool isInInspector(string _path){
            foreach(var _inspectorInfo in style.InspectorInfo){
                var _list = _inspectorInfo.Value as MutableListContainer;
                for(int i = 0; i  <_list.Count; i++){
                    if(_path.Contains(_list[i].AsVariable.AsString)){
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

    }

    public class Style : RecordContainer{
        public MutableDictionaryContainer ModifiedPathInfo = new MutableDictionaryContainer();
        public MutableDictionaryContainer InspectorInfo = new MutableDictionaryContainer();

        public Style() : base(){
            AddManagedColumn("path", this.ModifiedPathInfo);
            AddManagedColumn("inspector", this.InspectorInfo);
        }
    }
}
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
        private SelectorData selectorData = new SelectorData();
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
        private Dictionary<string, string> backupedComponentValues;
        private Vector2 modifiedScroll = new Vector2(0, 0);

        private void drawTxtAsset(){
            GUILayout.Label("SavingFile:");
            targetTextAsset = (TextAsset)EditorGUILayout.ObjectField(targetTextAsset, typeof(TextAsset), false);

            if(targetTextAsset == null){
                return;
            }


            GUILayout.BeginHorizontal();
            if (targetTextAsset != null && GUILayout.Button("Load")){
                this.selectorData = new SelectorData();
                if(targetTextAsset.text.Length > 0){
                    this.selectorData.ParseJson(targetTextAsset.text);
                }

                if (selectorData.ModifiedInfo.Count > 0){
                    foreach(var _searchPattern in selectorData.ModifiedInfo.Keys){
                        var _pathInfo = splitPath(_searchPattern);
                        var _data = selectorData.ModifiedInfo[_searchPattern].ToString();
                        var _objects = FindGameObjectsWithPattern(_pathInfo.SearchPattern);
                        foreach(var _object in _objects){
                            var _component = _object.GetComponent(_pathInfo.Component);
                            EditorJsonUtility.FromJsonOverwrite(_data, _component);
                            EditorUtility.SetDirty(_component);
                            _object.SetActive(!_object.activeSelf);
                            _object.SetActive(!_object.activeSelf);
                        }
                    }

                    Debug.Log("Loaded");
                }
            }

           


            if (selectorData.ModifiedInfo.Count > 0 && GUILayout.Button("Save")){

                string _path = AssetDatabase.GetAssetPath(targetTextAsset);
                // Save the modified content back to the asset file
                var _content = selectorData.ToString();
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

        private void UpdateAllModified(){
            foreach (var _modifiedInfo in selectorData.ModifiedInfo)
            {
                var _pathInfo = splitPath(_modifiedInfo.Key);
                updateModified(_pathInfo.Component, FindGameObjectsWithPattern(_pathInfo.SearchPattern)[0], _pathInfo.SearchPattern, false);

            }
        }

        private void drawModified()
        {
            if (selectorData.ModifiedInfo.Count <= 0)
            {
                return;
            }

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Modified " + selectorData.ModifiedInfo.Count + " Properties");
            if (GUILayout.Button("Update All", GUILayout.Width(80f)))
            {

                UpdateAllModified();
                this.Repaint();
            }

            GUILayout.EndHorizontal();

            modifiedScroll = EditorGUILayout.BeginScrollView(modifiedScroll);

            EditorGUI.indentLevel++;

            foreach (var _modifiedInfo in selectorData.ModifiedInfo)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label(_modifiedInfo.Key);
                var _pathInfo = splitPath(_modifiedInfo.Key);

                if (GUILayout.Button("Select", GUILayout.Width(50f)))
                {
                    search(_pathInfo.SearchPattern);
                }

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                {
                    
                    var _modifiedValue = _modifiedInfo.Value;
                    var _keyPaths = _modifiedValue.GetKeyPathList();
                    GenericMenu menu = new GenericMenu();
                    _keyPaths.RemoveAt(0);
                    
                    
                    menu.AddItem(new GUIContent("Update"), false, () =>
                    {
                        updateModified(_pathInfo.Component, FindGameObjectsWithPattern(_pathInfo.SearchPattern)[0], _pathInfo.SearchPattern, false);
                        this.Repaint();
                    });

                    menu.AddSeparator("Inspect");
                    //groups
                    foreach (var _keyPath in _keyPaths)
                    {
                        var _inspectTarget = _modifiedInfo.Key + "." + _keyPath;
                        if(this.selectorData.IsInInspector(_inspectTarget) == false){
                            menu.AddItem(new GUIContent(_keyPath), false, () =>
                            {
                                inspectTargetPath = _inspectTarget;
                            });
                        }
                    }


                    menu.AddSeparator("Remove");

                    menu.AddItem(new GUIContent("this"), false, () =>
                    {
                        selectorData.ModifiedInfo.Remove(_modifiedInfo.Key);
                    });

                    //remove
                    foreach (var _keyPath in _keyPaths)
                    {
                        menu.AddItem(new GUIContent(_keyPath + " ㅤ"), false, () =>
                        {
                            _modifiedValue.RemoveByKeyPath(_keyPath.Split('.'));
                        });
                    }

                    menu.ShowAsContext();

                }
                GUILayout.EndHorizontal();
                EditorGUILayout.TextField(_modifiedInfo.Value.ToString());
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

                if (backupedComponentValues != null)
                {
                    bool _hasDiff = false;
                    foreach (var _componentName in backupedComponentValues.Keys.ToArray())
                    {
                        _hasDiff = updateModified(_componentName, trackingTargetObject, targetPattern, true) || _hasDiff;
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

        private bool updateModified(string _componentName, GameObject _targetObject, string _searchPattern, bool _updateBackup)
        {
            var _currentComponent = _targetObject.GetComponent(_componentName);
            var _currentData = EditorJsonUtility.ToJson(_currentComponent);

            if (_updateBackup == false || backupedComponentValues[_componentName] != _currentData)
            {
                var _patternAndComponent = _searchPattern + "/$" + _componentName;
                MutableDictionaryContainer _origin = null;
                MutableDictionaryContainer _modified = MutableDictionaryContainer.CreateFromJson(_currentData);
                MutableDictionaryContainer _diff = null;

                if(_updateBackup == true){
                    _origin = MutableDictionaryContainer.CreateFromJson(backupedComponentValues[_componentName]);
                    _diff = _origin.GetDiff(_modified, -1);
                }else{
                    _origin = MutableDictionaryContainer.CreateFromJson(selectorData.ModifiedInfo[_patternAndComponent].ToString());
                    _origin.UpdateExistingFields(_modified);
                    _diff = _origin;
                }

                if (selectorData.ModifiedInfo.ContainsKey(_patternAndComponent) == false)
                {
                    selectorData.ModifiedInfo[_patternAndComponent] = _diff;
                }
                else
                {
                    selectorData.ModifiedInfo[_patternAndComponent].MergeCopyBy(_diff);
                }

                if(_updateBackup == true){
                    backupedComponentValues[_componentName] = _currentData;
                }
                return true;
            }

            return false;
        }
        
        private void backupComponentValues()
        {
            var components = trackingTargetObject.GetComponents<Component>();
            backupedComponentValues = new Dictionary<string, string>();
            
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null)
                {
                    var _componentName = component.GetType().ToString();
                    if (_componentName.Contains(".") == true)
                    {
                        _componentName = _componentName.Split(".").Last();
                    }

                    backupedComponentValues[_componentName] = EditorJsonUtility.ToJson(component);
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

             if (GUILayout.Button("Clear", GUILayout.Width(50f))){
                searchInput ="";
                search(searchInput);
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
                if(sc.Tags != null && sc.Tags.Length > 0){
                    _tags.UnionWith(sc.Tags);
                }
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
            if(selectorData.InspectorInfo.Count <=0){
                return;
            }

            GUILayout.Space(10f);
            GUILayout.Label("Custom Inspector");

            foreach(var _inspectorInfo in selectorData.InspectorInfo){
                var _inspectorList = _inspectorInfo.Value;
                var _targetPathWithProperty = _inspectorList[0].AsVariable.AsString;
                var _inpectorPathInfo = splitPath(_targetPathWithProperty);
                int dotIndex = _inpectorPathInfo.PropertyPath.IndexOf(".");
                var _pathWithoutRoot = _inpectorPathInfo.PropertyPath.Substring(dotIndex + 1);
                var _objects = FindGameObjectsWithPattern(_inpectorPathInfo.SearchPattern);
                if(_objects.Length <= 0){
                    continue;
                }

                GUILayout.BeginHorizontal();
                if(GUILayout.Button(_inspectorInfo.Key)){
                    GenericMenu menu = new GenericMenu();
                    var _key = _inspectorInfo.Key;
                    menu.AddItem(new GUIContent("Select All"), false, () =>{
                        HashSet<GameObject> _selectObjects = new HashSet<GameObject>(); 
                        for(int i = 0; i < _inspectorList.Count; i++){
                            var _targetPathWithProperty = _inspectorList[i].AsVariable.AsString;
                            var _inpectorPathInfo = splitPath(_targetPathWithProperty);
                            var _objects = FindGameObjectsWithPattern(_inpectorPathInfo.SearchPattern);
                            
                            _selectObjects.UnionWith(_objects);
                        }

                        Selection.objects = _selectObjects.ToArray();
                    });

                    menu.AddSeparator("Remove");
                    menu.AddItem(new GUIContent("this"), false, () =>
                    {
                        selectorData.InspectorInfo.Remove(_key);
                        Repaint();
                    });

                    for(int i = 0; i < _inspectorList.Count; i++){
                         var _index = i;
                         menu.AddItem(new GUIContent(_inspectorList[_index].AsVariable.AsString.Replace("/", "\\") + " ㅤ"), false, () =>
                         {
                             _inspectorList.RemoveAt(_index);
                             if(_inspectorList.Count <= 0){
                                selectorData.InspectorInfo.Remove(_key);
                             }

                             Repaint();
                         });
                    }


                    menu.ShowAsContext();
                }

                var _component = _objects[0].GetComponent(_inpectorPathInfo.Component);
                var _selectedObject = new SerializedObject(_component);
                var _property = getSerializedPropertyByPath(_selectedObject, _pathWithoutRoot);
                EditorGUILayout.PropertyField(_property, new GUIContent(""), true);
                if(_selectedObject.hasModifiedProperties == true)
                {
                    _selectedObject.ApplyModifiedProperties();
                    applyInspectorModified(_inspectorList, _inpectorPathInfo.PropertyPath, _component);
                    UpdateAllModified();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void applyInspectorModified(ListContainer<StringVariable> _targetPathList, string _targetValuePropertyPath, Component _component)
        {
            var _componentJson = EditorJsonUtility.ToJson(_component);
            var _value = MutableDictionaryContainer.CreateFromJson(_componentJson).FindValue(_targetValuePropertyPath.Split("."));

            foreach (var _targetPath in _targetPathList)
            {
                var _targetPathInfo = splitPath(_targetPath.AsVariable.AsString);
                var _targetObject = FindGameObjectsWithPattern(_targetPathInfo.SearchPattern);
                var _record = MutableDictionaryContainer.CreatePathAndValue(_targetPathInfo.PropertyPath.Split("."), _value);
                var _jsonForComponent = _record.ToString();

                foreach (var _object in _targetObject)
                {
                    var _componentObject = _object.GetComponent(_targetPathInfo.Component);
                    EditorJsonUtility.FromJsonOverwrite(_jsonForComponent, _componentObject);
                    EditorUtility.SetDirty(_componentObject);
                    _object.SetActive(!_object.activeSelf);
                    _object.SetActive(!_object.activeSelf);
                }
            }
        }

        private string getDataByPath(string _json, string _propertyPath){
            var _data = MutableDictionaryContainer.CreateFromJson(_json);
            _data.RemoveAllExceptAt(_propertyPath.Split("."));
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

        private (string SearchPattern, string Component, string PropertyPath) splitPath(string _path)
        {
            int _sIndex =_path.LastIndexOf('$');
            var _searchPattern = _path.Substring(0, _sIndex - 1);
            var _componentAndProperty = _path.Substring(_sIndex + 1);
            
            int dotIndex = _componentAndProperty.IndexOf(".");
            if(dotIndex >= 0){
                string _componentName = _componentAndProperty.Substring(0, dotIndex);
                string _propertyPath = _componentAndProperty.Substring(dotIndex + 1);
                return (_searchPattern, _componentName, _propertyPath);
            }else{
                return (_searchPattern, _componentAndProperty, null);
            }
        }

        private void drawSetInspector()
        {

            GUILayout.Label("Setup cusom inspector");
            GUILayout.Label("Target : " + inspectTargetPath);
            inspectName = EditorGUILayout.TextField("InspectorName", inspectName);


            var _selectedSample = getSampleValue(inspectTargetPath);

            GUILayout.Space(10f);
            foreach (var _inspectorInfo in selectorData.InspectorInfo)
            {
                var _paths = _inspectorInfo.Value;
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
                            if (_selectedDict.AreKeysEqual(_dict) == true)
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

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(inspectName));
            if (GUILayout.Button("OK"))
            {
                if (selectorData.InspectorInfo.ContainsKey(inspectName) == false)
                {
                    selectorData.InspectorInfo[inspectName] = new ListContainer<StringVariable>();
                }

                selectorData.InspectorInfo[inspectName].Add(new StringVariable(inspectTargetPath));

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
            var _objects = FindGameObjectsWithPattern(_pathInfo.SearchPattern);
            if(_objects.Length <= 0){
                return null;
            }

            var _component = _objects[0].GetComponent(_pathInfo.Component);
            var _json = EditorJsonUtility.ToJson(_component);
            return MutableDictionaryContainer.CreateFromJson(_json).FindValue(_pathInfo.PropertyPath.Split("."));
        }
        #endregion

    }

    public class SelectorData : RecordContainer{
        public DictionaryContainer<MutableDictionaryContainer> ModifiedInfo = new DictionaryContainer<MutableDictionaryContainer>();
        public DictionaryContainer<ListContainer<StringVariable>> InspectorInfo = new DictionaryContainer<ListContainer<StringVariable>>();

        public SelectorData() : base(){
            AddManagedColumn("path", this.ModifiedInfo);
            AddManagedColumn("inspector", this.InspectorInfo);
        }

        public bool IsInInspector(string _path){
            foreach(var _inspectorInfo in this.InspectorInfo){
                var _list = _inspectorInfo.Value;
                for(int i = 0; i  <_list.Count; i++){
                    if(_path.Contains(_list[i].AsVariable.AsString)){
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
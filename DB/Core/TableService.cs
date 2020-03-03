using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;
using BicDB.Storage;
using System;
using UnityEngine;
using System.Text.RegularExpressions;
using BicUtil.Json;
using System.Threading.Tasks;

namespace BicDB.Core
{
	static public class TableService{
        private const string PROP_FIELD_SESSION_COUNT = "initCount";
        private const string PROP_FIELD_USER_ID = "userId";
        private const string PROP_FIELD_VERSION = "version";
        private const string PROP_FIELD_INSTALL_VERSION = "installVersion";
        private const string PROP_FIELD_IS_SETUP = "isSetup";
        #region Const
        static public readonly string TABLENAME = "BICSYSTEM";
        #endregion

		#region Member
        static private bool isInit = false;
        static private TableContainer<TableModel> tableInfo;
        static private TableContainer<QueryModel> queryTable;
        #endregion

        #region Event
        static public event Action OnSetup{
            add{
                if(isInit == true && isSetup == true){
                    value();
                }else{
                    onSetup += value;
                }
            }

            remove{
                
            }
        }

        static public event Action<string, string> OnUpdate{
            add{
                if(isInit == true && isUpdate == true && isSetup == false){
                    value(lastVersion, currentVersion);
                }else{
                    onUpdate += value;
                }
            }

            remove{

            }
        }
        
        static public bool IsSetup{get{ return isSetup; }}
        static public bool IsUpdate{get{ return isUpdate; }}
        static public int SessionCount{get{
            Init();

            if(tableInfo == null){
                return 0;
            }

            if(hasProperty(PROP_FIELD_SESSION_COUNT) == false){
                return 0;
            }
            
            return getIntProperty(PROP_FIELD_SESSION_COUNT);
        ;}} 

        static public string UserId{
            get{
                Init();

                if(tableInfo == null){
                    return "0";
                }

                if(hasProperty(PROP_FIELD_USER_ID) == false){
                    return "0";
                }

                return getStringProperty(PROP_FIELD_USER_ID);
            }
        }

        static private string lastVersion = "";
        static private string currentVersion = "";
        static private bool isSetup = false;
        static private bool isUpdate = false;
        static private Action onSetup;
        static private Action<string, string> onUpdate;
        #endregion

        #region Logic
        static public void Init(){
            if(isInit == true){
                return;
            }

            throw new SystemException("TableService Not Load");
        }

        static public void init(){
            if(isInit == true){
                return;
            }

            isInit = true;

            tableInfo = new TableContainer<TableModel>(TABLENAME);
            queryTable = new TableContainer<QueryModel>("qa");
            
            try{
                currentVersion = Application.version;
            }catch(System.MissingMethodException){
                currentVersion = "0";
            }

        }

        static public void Load(Action<Result> _callback){
            init();
            
            var _loader = new TableLoader();
            _loader.AddTable(tableInfo, BicDB.Storage.FileStorage.GetInstance(), new FileStorageParameter("filesystem"), setupSysTable);
            _loader.AddTable(queryTable, BicDB.Storage.FileStorage.GetInstance(), new FileStorageParameter("filesystem"));
            _loader.Load(_callback, 3);
        }

        static public async Task<Result> LoadAsync(){
            init();

            var _loader = new TableLoader();
            _loader.AddTable(tableInfo, BicDB.Storage.FileStorage.GetInstance(), new FileStorageParameter("filesystem"), setupSysTable);
            _loader.AddTable(queryTable, BicDB.Storage.FileStorage.GetInstance(), new FileStorageParameter("filesystem"));
            var _result = await _loader.LoadAsync(3);
            return _result;
        }

        private static void setStringProperty(string _key, string _value){
            tableInfo.Property[_key] = new StringVariable(_value);
            PlayerPrefs.SetString(_key, _value);
        }

        private static void setIntProperty(string _key, int _value){
            tableInfo.Property[_key] = new IntVariable(_value);
            PlayerPrefs.SetInt(_key, _value);
        }

        private static bool hasProperty(string _key){
            if(tableInfo.Property.ContainsKey(_key) == false){
                if(PlayerPrefs.HasKey(_key) == false){
                    return false;
                }
            }

            return true;
        }

        private static string getStringProperty(string _key){
            try{
                return tableInfo.Property[_key].AsVariable.AsString;
            }catch{
                return PlayerPrefs.GetString(_key);
            }
        }

        private static int getIntProperty(string _key){
            try{
                return tableInfo.Property[_key].AsVariable.AsInt;
            }catch{
                return PlayerPrefs.GetInt(_key);
            } 
        }

        private static void increaseProperty(string _key){
            var _value = getIntProperty(_key);
            _value++;

            setIntProperty(_key, _value);
        }

        private static void saveProperty(Action<Result> _callback = null){
            tableInfo.Save(_callback);
            PlayerPrefs.Save();
        }

        private static bool setupSysTable(Result _result)
        {
                checkToSyncFileAndPrefs();
                Debug.Log("tableinfo load " + _result.Code.ToString());
                try{
                    Debug.Log("tableinfo is " + tableInfo.ToString());
                }catch{

                }
            //if(_result.IsSuccess == true){
                if(hasProperty(PROP_FIELD_VERSION)){
                    lastVersion = getStringProperty(PROP_FIELD_VERSION);
                }

                if(hasProperty(PROP_FIELD_USER_ID) == false){
                    setStringProperty(PROP_FIELD_USER_ID, Guid.NewGuid().ToString());
                }

                if(!hasProperty(PROP_FIELD_IS_SETUP)){
                    setStringProperty(PROP_FIELD_VERSION, currentVersion);
                    setStringProperty(PROP_FIELD_INSTALL_VERSION, currentVersion);
                    setIntProperty(PROP_FIELD_SESSION_COUNT, 1);
                    setStringProperty(PROP_FIELD_IS_SETUP, "true");

                    saveProperty(_tableInfoSaveResult=>{
                        if(_tableInfoSaveResult.Code == (int)FileStorage.ResultCode.Success){
                            if(onSetup != null){
                                onSetup();
                            }

                            isSetup = true;
                        }
                    });
                }else if(currentVersion != lastVersion){
                    setStringProperty(PROP_FIELD_VERSION, currentVersion);
                    setStringProperty(PROP_FIELD_VERSION, currentVersion);
                    increaseProperty(PROP_FIELD_SESSION_COUNT);
                    
                    saveProperty(_tableInfoSaveResult=>{
                        if(_tableInfoSaveResult.Code == (int)FileStorage.ResultCode.Success){
                            if(onUpdate != null){
                                onUpdate(lastVersion, currentVersion);
                            }

                            isUpdate = true;
                        }
                    });

                }else{
                    increaseProperty(PROP_FIELD_SESSION_COUNT);
                    saveProperty();
                }
           // }

            return true;
        }

        private static void checkToSyncFileAndPrefs()
        {
            if(tableInfo.Property.ContainsKey(PROP_FIELD_IS_SETUP) != PlayerPrefs.HasKey(PROP_FIELD_IS_SETUP)){
                #if UNITY_EDITOR
                Debug.LogError("[TableService] Sync warning, if you want uninstall, do Edit > Clear All PlayerPrefs");
                #else
                Debug.LogWarning("[TableService] Sync warning tableinfo.containskey(is_setup) = " + tableInfo.Property.ContainsKey(PROP_FIELD_IS_SETUP).ToString());
                #endif
            }
        }

        static public TableModel GetTableInfo(string _tableName, bool _createIfNotExsit = false){
            var _tableInfo = tableInfo.FirstOrDefault(_row=>_row.Name.AsString == _tableName);
            if(_tableInfo == null && _createIfNotExsit == true){
                _tableInfo = new TableModel();
                _tableInfo.Name.AsString = _tableName;
                _tableInfo.IsFirstSetup = true;
                tableInfo.Add(_tableInfo);
            }

            return _tableInfo;
        }

        static public bool HasProperty(string _key){
            Init();
            return hasProperty(_key);
        }

        static public void RemoveProperty(string _key){
            Init();
            tableInfo.Property.Remove(_key);
        }

        static public string GetStringProperty(string _key, string _default){
            if(hasProperty(_key) == false){
                setStringProperty(_key, _default);
                return _default;
            }

            return getStringProperty(_key);
        }

        static public int GetIntProperty(string _key, int _default){
            if(hasProperty(_key) == false){
                setIntProperty(_key, _default);
                return _default;
            }

            return getIntProperty(_key);
        }

        static public IVariable GetProperty(string _key, IVariable _defaultVariable){
            Init();
            
            if(hasProperty(_key) == false){
                if(_defaultVariable != null){
                    setStringProperty(_key, _defaultVariable.AsString);
                    saveProperty();
                }else{
                    return null;
                }
            }
            try{
                return tableInfo.Property[_key].AsVariable;
            }catch{
                return new StringVariable(PlayerPrefs.GetString(_key));
            }
        }

        static public void Save(){
            tableInfo.Save();
            PlayerPrefs.Save();
        }

        static public void BackupProperties(){
            try{
                PlayerPrefs.SetInt(PROP_FIELD_SESSION_COUNT, getIntProperty(PROP_FIELD_SESSION_COUNT));
                PlayerPrefs.SetString(PROP_FIELD_USER_ID, getStringProperty(PROP_FIELD_USER_ID));
                PlayerPrefs.SetString(PROP_FIELD_VERSION, getStringProperty(PROP_FIELD_VERSION));
                PlayerPrefs.SetString(PROP_FIELD_INSTALL_VERSION, getStringProperty(PROP_FIELD_INSTALL_VERSION));
                PlayerPrefs.SetString(PROP_FIELD_IS_SETUP, getStringProperty(PROP_FIELD_IS_SETUP));
            }catch{

            }
        }
        #endregion

        #region Query
        static public string Query(string _queryString){
            TableService.Init();
            /*
                필수 QueryId EIFHAKEIFAAKEIDFKA;
                필수아님 TargetUser 유저아이디;
                필수아님 Expiered 19062023;
                필수아님 Message 감사합니다. 완료되었습니다.;
                필수 Repeat 1;
                INSERT INTO 테이블이름(필드이름1, 필드이름2, 필드이름3, ...) VALUES (데이터값1, 데이터값2, 데이터값3, ...);
                UPDATE 테이블이름 SET 필드이름1=데이터값1, 필드이름2=데이터값2, ... WHERE 필드이름=데이터값 AND 필드이름=데이터값;
                DELETE FROM 테이블이름 WHERE (필드이름=데이터값 OR 필드이름=데이터값) AND 필드이름=데이터값;
             */

            string[] _querys = _queryString.Split(';');
            
            var _message = setupQuery(_querys[0]);

            if(_queryString.Length >= 2){
                for(int i = 1; i < _querys.Length; i++){
                    string _query = _querys[i];
                    if(_query != string.Empty){
                        string _type = _query.Substring(0, _query.IndexOf(' '));
                        doQuery(_type, _query);
                    }
                }
            }

            return _message;
        }

        static private void doQuery(string _type, string _query){
            switch(_type.ToLower()){
                case "insert":
                queryInsert(_query);
                break;
                case "update":
                queryUpdate(_query);
                break;
                case "delete":
                queryDelete(_query);
                break;
            }
        }

        private static string setupQuery(string _queryString)
        {
            var _rx = new Regex(@"\bsetup\s+([\S\s]+)", RegexOptions.IgnoreCase);
            var _match = _rx.Match(_queryString);

            if(_match.Success == false){
                throw new QueryException("It is not setup query", QueryExceptionType.SetupParse);
            }

            var _queryList = _match.Groups[1].Value.Split(new char[]{','});
            int _repeat = 0;
            string _message = "";
            string _queryId = "";
            for(int i = 0; i < _queryList.Length; i++){
                var _query = _queryList[i].Split(new char[]{'='});
                var _type = _query[0].Trim().ToLower();
                var _option = _query[1].Trim();

                switch(_type){
                    case "queryid":
                    _queryId = _option;
                    break;
                    case "targetuser":
                    checkTargetUser(_option);
                    break;
                    case "expired":
                    checkExpired(_option);
                    break;
                    case "repeat":
                    _repeat = int.Parse(_option);
                    break;
                    case "message":
                    _message = _option;
                    break;
                }
            }

            if(_queryId == ""){
                throw new QueryException("Not found query id", QueryExceptionType.NotFoundQueryId);
            }

            var _queryModel = GetQueryModel(_queryId);

            if(_queryModel.ExecCount.AsInt > _repeat){
                throw new QueryException("No Repeat", QueryExceptionType.CanNotRepeat);
            }

            _queryModel.ExecCount.AsInt ++;
            queryTable.Save();

            return _message;
        }

        private static void checkExpired(string _expiredDate)
        {
            var _now = int.Parse(DateTime.Now.ToString("yyMMddHH"));
            var _expired = int.Parse(_expiredDate);
            
            if(_now >= _expired){
                throw new QueryException("It is expired query", QueryExceptionType.Expired);
            }
        }

        private static void checkTargetUser(string targetId)
        {
            if(targetId != TableService.UserId){
                throw new QueryException("Your not targetUser", QueryExceptionType.NotTargetUser);
            }
        }

        static private void queryInsert(string _query){

        }

        static private void queryUpdate(string _query)
        {
            //UPDATE 테이블이름 SET 필드이름1=데이터값1, 필드이름2=데이터값2 WHERE 필드이름=데이터값 AND 필드이름=데이터값;
            // 이전에 해야할것 재화용테이블 만들기, 유저고유아이디 만들어 tableservice에 넣기
            var _rx = new Regex(@"\bupdate\s+(\w+)\s+set\s([\S\s]+)where\s+([\S\s]+)\b", RegexOptions.IgnoreCase);
            var _match = _rx.Match(_query);
            var _tableName = _match.Groups[1].Value;
            var _value = parseUpdateQuery(_match.Groups[2].Value);
            var _where = parseWhereQuery(_match.Groups[3].Value);

            var _tableInfo = TableService.GetTableInfo(_tableName, false);

            if (_tableInfo == null)
            {
                throw new QueryException("not found table " + _tableName, QueryExceptionType.NotFoundTable);
            }

            findTargetRow(_tableInfo.Table, _where, _row=>
            {
                applyUpdateValue(_row, _value);
            });

            _tableInfo.Table.Save();
        }

        private static void applyUpdateValue(IRecordContainer _row, string[] _value)
        {
            for (int j = 0; j < _value.Length - 2; j += 3)
            {
                var _variable = JsonConvertor.GetInstance().BuildVariable(_value[j + 2]);

                switch (_value[j])
                {
                    case "=":
                        _row[_value[j + 1]].AsVariable.AsString = _variable.AsVariable.AsString;
                        break;
                    case "-=":
                        _row[_value[j + 1]].AsVariable.AsFloat -= _variable.AsVariable.AsFloat;
                        break;
                    case "+=":
                        _row[_value[j + 1]].AsVariable.AsFloat += _variable.AsVariable.AsFloat;
                        break;
                    case "*=":
                        _row[_value[j + 1]].AsVariable.AsFloat *= _variable.AsVariable.AsFloat;
                        break;
                }
            }
        }

        private static void findTargetRow(IQueryTable _table, string[] _where, Action<IRecordContainer> _applyCallback)
        {
            for (int i = 0; i < _table.Count; i++)
            {
                var _row = _table.RecordAt(i);

                bool _result = true;
                for (int j = 0; j < _where.Length - 2; j += 3)
                {
                    switch (_where[j])
                    {
                        case "=":
                            _result = _result && _row[_where[j+1]].AsVariable.AsString == _where[j+2];
                            break;
                        case "!=":
                            _result = _result && _row[_where[j+1]].AsVariable.AsString != _where[j+2];
                            break;
                    }
                }

                if (_result == true)
                {
                    _applyCallback(_row);
                }
            }
        }

        static private string[] parseWhereQuery(string _whereQuery){
            string[] _whereQueryList;
            List<string> _where = new List<string>();

            if(_whereQuery.Contains("and") || _whereQuery.Contains("AND")){
                _whereQueryList = _whereQuery.Split(new String[]{"and", "AND"}, StringSplitOptions.RemoveEmptyEntries);
            }else{
                _whereQueryList = new string[]{_whereQuery};
            }

            for(int i = 0; i < _whereQueryList.Length; i++){
                if(_whereQueryList[i].Contains("!=")){
                    var _query = _whereQueryList[i].Split(new String[]{"!="}, StringSplitOptions.RemoveEmptyEntries);
                    _where.Add("!=");
                    _where.Add(_query[0].Trim());
                    _where.Add(_query[1].Trim());
                }else if(_whereQueryList[i].Contains("=")){
                    var _query = _whereQueryList[i].Split(new String[]{"="}, StringSplitOptions.RemoveEmptyEntries);
                    _where.Add("=");
                    _where.Add(_query[0].Trim());
                    _where.Add(_query[1].Trim());
                }else{
                    throw new QueryException("Not found == or != in where query", QueryExceptionType.WhereParse);
                }
            }

            return _where.ToArray();
        }

        static private string[] parseUpdateQuery(string _whereQuery){
            string[] _valueQueryList;
            List<string> _value = new List<string>();

            if(_whereQuery.Contains(",")){
                _valueQueryList = _whereQuery.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
            }else{
                _valueQueryList = new string[]{_whereQuery};
            }

            for(int i = 0; i < _valueQueryList.Length; i++){
                if(_valueQueryList[i].Contains("+=")){
                    var _query = _valueQueryList[i].Split(new String[]{"+="}, StringSplitOptions.RemoveEmptyEntries);
                    _value.Add("+=");
                    _value.Add(_query[0].Trim());
                    _value.Add(_query[1].Trim());
                }else if(_valueQueryList[i].Contains("-=")){
                    var _query = _valueQueryList[i].Split(new String[]{"-="}, StringSplitOptions.RemoveEmptyEntries);
                    _value.Add("-=");
                    _value.Add(_query[0].Trim());
                    _value.Add(_query[1].Trim());
                }else if(_valueQueryList[i].Contains("*=")){
                    var _query = _valueQueryList[i].Split(new String[]{"*="}, StringSplitOptions.RemoveEmptyEntries);
                    _value.Add("*=");
                    _value.Add(_query[0].Trim());
                    _value.Add(_query[1].Trim());
                }else if(_valueQueryList[i].Contains("=")){
                    var _query = _valueQueryList[i].Split(new String[]{"="}, StringSplitOptions.RemoveEmptyEntries);
                    _value.Add("=");
                    _value.Add(_query[0].Trim());
                    _value.Add(_query[1].Trim());
                }else{
                    throw new QueryException("Not found = or += or -= or *= in where query", QueryExceptionType.UpdateParse);
                }
            }

            return _value.ToArray();
        }

        static private void queryDelete(string _query){
            
        }

        static private QueryModel GetQueryModel(string _queryId){
            var _result = queryTable.FirstOrDefault(_row=>_row.Id.AsString == _queryId);
            if(_result == null){
                _result = new QueryModel();
                _result.Id.AsString = _queryId;
                _result.ExecCount.AsInt = 0;
                queryTable.Add(_result);
            }

            return _result;
        }
        #endregion
	}
}
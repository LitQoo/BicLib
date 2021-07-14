using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicUtil.Json;
using UnityEngine;
using static BicDB.Storage.FileStorage;

namespace BicDB.Storage
{
    public static class FileStorageUtil
    {
        public static void BuildTable<T>(ITableContainer<T> _table, string _data, Result _result) where T : IRecordContainer, new()
        {
            try
            {
                int _counter = 0;
                JsonConvertor.GetInstance().BuildTableContainer(_table, ref _data, ref _counter);
            }
            catch (Exception _e)
            {
                #if UNITY_EDITOR
                Debug.LogError("[BicDB] Fail load " + _table.Name + "/" + _e.Message + "/" + _e.ToString());
                #endif
                _result.Code = (int)ResultCode.FailedConvertJson;
                _result.Message = "FailedConvertJson some Error BuildTableContainer " + _e.Message;
            }
        }
        public static async Task<string> GetFileDataWithPathListAsync(string _tableName, string _encryptKey){
            var _filename = GetFileName(_tableName);

            string _data = await FileStorageUtil.ReadAndDecryptAsync(GetPath(_filename), _encryptKey);

            if (_data == null || _data == string.Empty)
            {
                if(_tableName == TableService.TABLENAME){
                    //Debug.Log("[BicDB] bicsystem path by PlayerPrefs.GetString");

                    if(PlayerPrefs.HasKey(TableService.TABLENAME) == true){
                        string _path = PlayerPrefs.GetString(TableService.TABLENAME);
                        Debug.Log("PlayerPrefs table path = " + _path);

                        if(_path != string.Empty){
                            _data = await FileStorageUtil.ReadAndDecryptAsync(_path, _encryptKey);
                        }
                    }
                }else{
                    //Debug.Log("[BicDB] path by bicsystem.path");

                    var _tableInfo = TableService.GetTableInfo(_tableName, false);
                    if (_tableInfo != null)
                    {
                        for (int i = _tableInfo.PathList.Count - 1; i >= 0; i--)
                        {
                            _data = await FileStorageUtil.ReadAndDecryptAsync(_tableInfo.PathList[i].AsString, _encryptKey);

                            if (_data != null && _data != string.Empty)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return _data;
        }

        public static string GetFileDataWithPathList(string _tableName, string _encryptKey)
        {
            var _filename = GetFileName(_tableName);

            string _data = FileStorageUtil.ReadAndDecrypt(GetPath(_filename), _encryptKey);

            if (_data == null || _data == string.Empty)
            {
                if(_tableName == TableService.TABLENAME){
                    //Debug.Log("[BicDB] bicsystem path by PlayerPrefs.GetString");

                    if(PlayerPrefs.HasKey(TableService.TABLENAME) == true){
                        string _path = PlayerPrefs.GetString(TableService.TABLENAME);
                        Debug.Log("PlayerPrefs table path = " + _path);

                        if(_path != string.Empty){
                            _data = FileStorageUtil.ReadAndDecrypt(_path, _encryptKey);
                        }
                    }
                }else{
                    //Debug.Log("[BicDB] path by bicsystem.path");

                    var _tableInfo = TableService.GetTableInfo(_tableName, false);
                    if (_tableInfo != null)
                    {
                        for (int i = _tableInfo.PathList.Count - 1; i >= 0; i--)
                        {
                            _data = FileStorageUtil.ReadAndDecrypt(_tableInfo.PathList[i].AsString, _encryptKey);

                            if (_data != null && _data != string.Empty)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return _data;
        }

        public static async Task<Result> LoadByFileAsync<T>(ITableContainer<T> _table, string _encryptKey) where T : IRecordContainer, new (){
            var _filename = GetFileName(_table.Name);
            var _result = new Result((int)ResultCode.Success, GetPath(_filename), 0);
            string _data = null;

            try
            {
                _data = await FileStorageUtil.GetFileDataWithPathListAsync(_table.Name, _encryptKey);
            }
            catch (System.Exception _e)
            {
                Debug.Log("[Exception] " + _e.Message + "/" + _e.ToString());
                _result.Code = (int)ResultCode.FileStream;
                _result.Message = _e.Message;
            }

            if (_result.Code == (int)ResultCode.Success && !string.IsNullOrEmpty(_data) && _data.Length > 10)
            {
                _result.Code = (int)ResultCode.Success;
                _result.HashCode = _data.GetHashCode();
                FileStorageUtil.BuildTable(_table, _data, _result);
            }

            return _result;
        }

        public static void LoadByFile<T>(ITableContainer<T> _table, Action<Result> _callback, string _encryptKey) where T : IRecordContainer, new ()
        {
            var _filename = GetFileName(_table.Name);
            var _result = new Result((int)ResultCode.Success, GetPath(_filename), 0);
            string _data = null;

            try
            {
                _data = FileStorageUtil.GetFileDataWithPathList(_table.Name, _encryptKey);
            }
            catch (System.Exception _e)
            {
                Debug.Log("[Exception] " + _e.Message + "/" + _e.ToString());
                _result.Code = (int)ResultCode.FileStream;
                _result.Message = _e.Message;
            }

            if (_result.Code == (int)ResultCode.Success && !string.IsNullOrEmpty(_data) && _data.Length > 10)
            {
                _result.Code = (int)ResultCode.Success;
                _result.HashCode = _data.GetHashCode();
                FileStorageUtil.BuildTable(_table, _data, _result);
            }

            if (_callback != null)
            {
                _callback(_result);
            }
        }

        #region static
        static public string GetPath(string _fileName){
            return Application.persistentDataPath + "/" + _fileName;
        }

        static System.Object locker = new System.Object();

        static public void Write(string _data, string _fileName, string _key)
        {
#if !WEB_BUILD

            string _path = GetPath(_fileName);
            if (_key != string.Empty)
            {
                _data = AESEncrypt256(_data, _key);
            }

            WriteFile(_data, _path);

#else

            throw new System.Exception ("webbuild do not save to file");

#endif
        }

        public static void WriteFile(string _data, string _path)
        {
            lock(locker){
                using (System.IO.FileStream _file = new System.IO.FileStream(_path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    using (System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file))
                    {
                        _streamWriter.Write(_data);
                        _streamWriter.Flush();
                        _streamWriter.Close();
                        _file.Close();
                    }
                }
            }
        }


        static public async Task WriteAsync(string _data, string _fileName, string _key)
        {
#if !WEB_BUILD

            string _path = GetPath(_fileName);
            if (_key != string.Empty)
            {
                _data = AESEncrypt256(_data, _key);
            }

            await WriteFileAsync(_data, _path);

#else

            throw new System.Exception ("webbuild do not save to file");

#endif
        }


        public static async Task WriteFileAsync(string _data, string _path)
        {
            using (System.IO.FileStream _file = new System.IO.FileStream(_path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (System.IO.StreamWriter _streamWriter = new System.IO.StreamWriter(_file))
                {
                    await _streamWriter.WriteAsync(_data);
                    await _streamWriter.FlushAsync();
                    _streamWriter.Close();
                    _file.Close();
                }
            }
        }

        public static void WriteByte(byte[] _data, string _path)
        {
            using (System.IO.FileStream _file = new System.IO.FileStream(_path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (System.IO.BinaryWriter _writer = new System.IO.BinaryWriter(_file))
                {
                    _writer.Write(_data);
                    _writer.Flush();
                    _writer.Close();
                    _file.Close();
                }
            }
        }

        static public string ReadByTableName(string _tableName, string _key){
            return ReadAndDecrypt(GetPath(GetFileName(_tableName)), _key);
        }
        
        static public async Task<string> ReadFileAsync(string _path){
            string _data = null;

            if (System.IO.File.Exists(_path))
            {
                using(System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read)){
                    using(System.IO.StreamReader _stream = new System.IO.StreamReader(_file)){
                        _data = await _stream.ReadToEndAsync ();
                        _stream.Close();
                        _file.Close();
                    }
                }
            }

            return _data;
        }

        static public string ReadFile(string _path){
            string _data = null;

            lock(locker){
                if (System.IO.File.Exists(_path))
                {
                    using(System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read)){
                        using(System.IO.StreamReader _stream = new System.IO.StreamReader(_file)){
                            _data = _stream.ReadToEnd ();
                            _stream.Close();
                            _file.Close();
                        }
                    }
                }
            }

            return _data;
        }

        static public (string headline, string data) ReadFileHeadLineAndData(string _path){
            string _data = null;
            string _head = null;

            lock(locker){
                try{
                    if (System.IO.File.Exists(_path))
                    {
                        using(System.IO.FileStream _file = new System.IO.FileStream (_path, System.IO.FileMode.Open, System.IO.FileAccess.Read)){
                            using(System.IO.StreamReader _stream = new System.IO.StreamReader(_file)){
                                _head = _stream.ReadLine();
                                _data = _stream.ReadToEnd ();
                                _stream.Close();
                                _file.Close();
                            }
                        }
                    }
                }catch{

                }
            }

            return (_head, _data);
        }

        static public async Task<string> ReadAndDecryptAsync(string _path, string _key){
            #if !WEB_BUILD

            string _data = await ReadFileAsync(_path);

            if(string.IsNullOrEmpty(_data) == false){
                if(_key != string.Empty){
                    try{
                        var _result = AESDecrypt256(_data, _key);
                        return _result;
                    }catch{
                        return _data;
                    }
                }else{
                    return _data;
                }
            }else{
                return _data;
            }
            #else
            return null;
            #endif 
        }

        static public string ReadAndDecrypt(string _path, string _key){
            #if !WEB_BUILD

            string _data = ReadFile(_path);

            if(string.IsNullOrEmpty(_data) == false){
                if(_key != string.Empty){
                    try{
                        var _result = AESDecrypt256(_data, _key);
                        return _result;
                    }catch{
                        return _data;
                    }
                }else{
                    return _data;
                }
            }else{
                return _data;
            }
            #else
            return null;
            #endif 
        }

        static public string AESDecrypt256(String Input, String key)
        {
            if (string.IsNullOrEmpty(Input)) {
                return string.Empty;
            }

            key = key.PadRight(16, '_');

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(Input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }

        static public String AESEncrypt256(String Input, String key)
        {
            if (string.IsNullOrEmpty(Input)) {
                return string.Empty;
            }

            key = key.PadRight(16, '_');

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);        
            aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(Input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        static public string GetFileName(string _tableName){
            return FILE_NAME_PREFIX + _tableName;
        }

        static public void RemoveFile(string _filePath){
            if(System.IO.File.Exists(_filePath) == true){
                System.IO.File.Delete(_filePath);
            }
        } 
        #endregion
    }
}
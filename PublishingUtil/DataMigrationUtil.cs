using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB.Container;
using BicDB.Core;
using BicDB.Storage;
using BicUtil.UI;
using UnityEngine;

namespace BicUtil.DataMigration{
    public class DataMigrationUtil
    {
        
        #region DataMigration

        private string appId;
        private string key;
        private string url;
        private string lastKey;
        private CommonPopup commonPopup;

        public void Start(CommonPopup _commonPopup, string _appId, string _key, string _url){
            this.appId = _appId;
            this.key = _key;
            this.url = _url;
            this.commonPopup = _commonPopup;

            if(commonPopup == null){
                throw new System.Exception("Need setup data migration");
            }

            if(string.IsNullOrEmpty(BicUtil.Crypto.AES256.KEY) == false){
                lastKey = BicUtil.Crypto.AES256.KEY;
            }

            BicUtil.Crypto.AES256.SetKey(key);

            if(TableService.IsSetup == true){
                commonPopup.Confirm("You can transfer data to another device.", "Import Data", import);
                commonPopup.SetTitle("Data Migration");
                commonPopup.SetCloseButton(commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
            }else{
                commonPopup.Select("You can transfer data to another device.", "Export Data", "Import Data", confirmExport, import);
                commonPopup.SetTitle("Data Migration");
                commonPopup.SetCloseButton(commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
            }
        }

        private void finish(){
            if(string.IsNullOrEmpty(lastKey) == false){
                BicUtil.Crypto.AES256.SetKey(lastKey);
            }
        }

        private void confirmExport(){
            commonPopup.Confirm("Do you want\nto back up your data\nto the server?", "Yes", export);
            commonPopup.SetTitle("Data Migration");
            commonPopup.SetBottomButton("Cancel", commonPopup.Close);
            commonPopup.SetCloseButton(commonPopup.Close);
            commonPopup.SetDimmed(commonPopup.Close);
            commonPopup.Open();
        }

        private void import()
        {
            if(TableService.IsSetup == true){
                commonPopup.Input("Input backup code", "Confirm", "", "", restore, commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
            }else{
                commonPopup.Confirm("You can receive data\nonly during the first run\nafter installation.", "Confirm", commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
            }
        }

        private async void restore()
        {
            commonPopup.Dimmed("Data is being recovered");
            commonPopup.Open();

            var _backupId = commonPopup.InputText;
            var _result = await restore(_backupId, appId);


            if(_result == null || _result.ContainsKey("result") == false || _result["result"].AsVariable.AsInt != 0){

                var _msg = "";

                try{
                    _msg = _result["message"].AsVariable.AsString;
                }catch{

                }

                commonPopup.Confirm("Failed Migration\n Check backup code\n"+_msg, "Confirm", commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
                return;  
            }

            if(_result.ContainsKey("appId") == true && _result["appId"].AsVariable.AsString != appId){

                commonPopup.Confirm("Failed Migration\n Check app id\n"+_result["appId"].AsVariable.AsString, "Confirm", commonPopup.Close);
                commonPopup.SetDimmed(commonPopup.Close);
                commonPopup.Open();
                return;
            }

            TableService.Restore(_result["data"].AsVariable.AsString);
            commonPopup.Dimmed("Data migration is complete\nPlease restart the game");
            commonPopup.Open();

            await completeBackup(_backupId, appId);
        }

        private async void export()
        {
            var _backupData = TableService.Backup();
            commonPopup.Dimmed("Data is being sent");
            commonPopup.Open();
            var _result = await backup(TableService.UserId, appId, _backupData);

            if(_result == null || _result["result"].AsVariable.AsInt != 0){
                commonPopup.Confirm("Backup creation failed\nPlease try again\n"+(_result != null ? _result["message"].AsVariable.AsString : ""), "Confirm", commonPopup.Close);  
                commonPopup.SetDimmed(commonPopup.Close); 
                commonPopup.Open(); 
            }else{
                commonPopup.NoButton("Backup complete\n\n" + _result["backupId"].AsVariable.AsString + "\n\nEnter the code above\non the device to be migrated\nBackup data will be deleted\nafter 24 hours.");
                commonPopup.Open();
            }
        }

        private async Task<RecordContainer> backup(string _userId, string _appId, string _data){
            var _record = new RecordContainer();
            Dictionary<string, string> _param = new Dictionary<string, string>();
            _param["userId"] = _userId;
            _param["data"] = _data;
            _param["appId"] = _appId;
            _param["version"] = Application.version;

            var _webparam = new WebStorageParameter(){
                Url=this.url+"/BackupData",
                ShouldEncrypt = true,
                Param = _param,
                CachingLevel = CachingLevel.None,
            };

            var _result = await WebStorage.Instance.SendRecordAsync<RecordContainer>(_record, _webparam);
            return _result.Data;
        }

        private async Task<RecordContainer> restore(string _backupId, string _appId){
            var _record = new RecordContainer();
            Dictionary<string, string> _param = new Dictionary<string, string>();
            _param["backupId"] = _backupId;
            _param["appId"] = _appId;

            var _webparam = new WebStorageParameter(){
                Url=this.url+"/RestoreData",
                ShouldEncrypt = true,
                Param = _param,
                CachingLevel = CachingLevel.None,
            };

            var _result = await WebStorage.Instance.SendRecordAsync<RecordContainer>(_record, _webparam);
            return _result.Data;
        }

        private async Task completeBackup(string _backupId, string _appId){
            var _record = new RecordContainer();
            Dictionary<string, string> _param = new Dictionary<string, string>();
            _param["backupId"] = _backupId;
            _param["appId"] = _appId;

            var _webparam = new WebStorageParameter(){
                Url=this.url+"/CompleteRestore", 
                ShouldEncrypt = true,
                Param = _param,
                CachingLevel = CachingLevel.None,
            };

            var _result = await WebStorage.Instance.SendRecordAsync<RecordContainer>(_record, _webparam);

            //백업카운트 높이고 백업카운트 있는 데이터는 백업못받도록 하기 -> 
            //문제있을경우 help@bigjamgames.com로 backupid를 보내라 하기
        }
        #endregion
    }
}
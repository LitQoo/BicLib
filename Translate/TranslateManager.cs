using System.Collections;
using System.Collections.Generic;
using BicUtil.SingletonBase;
using UnityEngine;
using BicDB.Container;
using BicDB.Variable;
using System.Linq;
using BicDB.Storage;

namespace BicUtil.Translate
{
    public class TranslateManager : MonoBehaviourSoftBase<TranslateManager>
    {   
        #region LinkingObject
        [SerializeField]
        private TextTranslateInfo[] texts;
        [SerializeField]
        private ImageTraslateInfo[] images;
        [SerializeField]
        private SpriteInfo[] sprites;
        [SerializeField]
        private string language;
        [SerializeField]
        private string resourceFilePath;
        #endregion

        #region InstantData
        private TableContainer<TranslateValue> Table;
        #endregion

        #region LifeCycle
        private new void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            if (language == "")
            {
                language = Application.systemLanguage.ToString();
            }
#else
            language = Application.systemLanguage.ToString();
#endif

#if UNITY_WEBGL
            language = "English";
#endif

            this.Table = new TableContainer<TranslateValue>("translate");
            this.Table.SetStorage(ResourceStorage.GetInstance());
            this.Table.Load(_result =>
            {
                if (_result.Code != (int)ResourceStorage.ResultCode.Success)
                {
                    throw new System.Exception("not load TranslateValue " + _result.Message);
                }
            }, new ResourceStorageParameter(this.resourceFilePath));
            
            translateTexts();
            translateImages();
        }
        #endregion

        #region Logic
        private void translateTexts(){
            for (int i = 0; i < texts.Length; i++)
            {
                SetText(texts[i].Id, texts[i].Target);
            }
        }

        private void translateImages(){
            for (int i = 0; i < images.Length; i++)
            {
                SetImage(images[i].Id, images[i].Target);
            }
        }

        public void SetImage(string _id, UnityEngine.UI.Image _image){
            if(_image != null){
                var _spriteId = GetText(_id);
                if(string.IsNullOrEmpty(_spriteId) == false){
                    _image.sprite = getSprite(_spriteId);
                }
            }
        }

        private Sprite getSprite(string _spriteId){
            for(int i = 0; i < sprites.Length; i++){
                if(sprites[i].SpriteId == _spriteId){
                    return sprites[i].Sprite;
                }
            }

            return null;
        }

        public Sprite GetSprite(string _id){
            return getSprite(GetText(_id));
        }

        public void SetText(string _id, UnityEngine.UI.Text _text){
            if(_text != null){
                _text.text = GetText(_id);
            }
        }

        public string GetText(string _id){
            var _value = this.Table.FirstOrDefault(_row=>_row.Id.AsString == _id);

            if(_value == null){
                Debug.LogWarning("[Translate] Not support " + _id);
                return string.Empty;
            }

            if(_value.ContainsKey(language) == true){
                if(string.IsNullOrEmpty(_value[language].AsVariable.AsString) == false){
                    return _value[language].AsVariable.AsString.Replace("\\n", "\n");
                }
            }

            if(_value.ContainsKey("English") == true){
                return _value["English"].AsVariable.AsString.Replace("\\n", "\n");
            }

            Debug.LogWarning("[Translate] Not support " + _id);
            return string.Empty;
        }
        #endregion
    }
    
    [System.Serializable]
    public struct TextTranslateInfo{
        public string Id;
        public UnityEngine.UI.Text Target;
    }

    [System.Serializable]
    public struct ImageTraslateInfo{
        public string Id;
        public UnityEngine.UI.Image Target;
    }


    [System.Serializable]
    public struct SpriteInfo{
        public string SpriteId;
        public Sprite Sprite;
    }

    public class TranslateValue : RecordContainer
    {
        #region Language
        public const string KOREAN = "Korean";
        public const string ENGLISH = "English";
        #endregion
        public StringVariable Id = new StringVariable();
        
        
        public TranslateValue()
        {
            AddManagedColumn("ID", this.Id);
        }
    }
}
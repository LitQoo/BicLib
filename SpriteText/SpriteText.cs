using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BicUtil.SpriteText{
    [ExecuteInEditMode]
    public class SpriteText : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private SpriteAtlas fontAtlas;
        [SerializeField]
        private string text;
        [SerializeField]
        private Color color = Color.white;
        #endregion
        
        #region InstantData
        [SerializeField]
        private List<CharacterObject> characters = new List<CharacterObject>();
        #endregion

        #region Logic
        private void OnValidate() {
            reloadChild();
            SetText(text);
            SetColor(color);
        }

        private void reloadChild()
        {
            this.characters.Clear();

            for(int i = 0; i < this.transform.childCount; i++){
                var _image = this.transform.GetChild(i).GetComponent<Image>();
                var _layout = this.transform.GetChild(i).GetComponent<LayoutElement>();
                var _character = new CharacterObject(_image, _layout);
                this.characters.Add(_character);
            }
        }

        public void SetColor(Color _color){
            this.color = _color;
            for(int i = 0; i < characters.Count; i++){
                characters[i].Image.color = _color;
            }
        }

        public void SetText(string _text){
            for(int i = 0; i < _text.Length; i++){
                CharacterObject _character = null;
                if(characters.Count > i){
                    _character = characters[i];
                }else{
                    var _newCharacter = new GameObject();
                    var _image = _newCharacter.AddComponent<Image>();
                    var _layout = _newCharacter.AddComponent<LayoutElement>();
                    _newCharacter.transform.SetParent(this.transform);
                    _character = new CharacterObject(_image, _layout);
                    characters.Add(_character);
                }

                _character.Image.gameObject.SetActive(true);
                _character.LayoutElement.ignoreLayout = false;
                _character.Image.sprite = getSprite(_text[i]);
                _character.Image.color = this.color;
                _character.Image.SetNativeSize();
            }

            if(_text.Length < characters.Count){
                for(int i = _text.Length; i < characters.Count; i++){
                    characters[i].LayoutElement.ignoreLayout = true;
                    characters[i].LayoutElement.gameObject.SetActive(false);
                }
            }
        }

        private Sprite getSprite(char _char){
            var _sprite = fontAtlas.GetSprite(_char.ToString());

            if(_sprite == null){
                switch(_char){
                    case '.': _sprite = fontAtlas.GetSprite("comma");break;
                }
            }
            
            return _sprite;
        }
        #endregion
    }

    [System.Serializable]
    class CharacterObject{
        public Image Image;
        public LayoutElement LayoutElement;

        public CharacterObject(Image _image, LayoutElement _layout){
            this.Image = _image;
            this.LayoutElement = _layout;
        }
    }
}
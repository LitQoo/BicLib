using System.Collections;
using System.Collections.Generic;
using BicUtil.Translate;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Text))]
public class TranslateText : MonoBehaviour
{
    [SerializeField]
    private string translateId;
    [SerializeField]
    private UnityEngine.UI.Text target;

    private void Awake(){
        setupTarget();
        SetTextByTranslateId(translateId);
    }

    public void SetTextWithFormat(params string[] _text){
        setupTarget();
        this.target.text = string.Format(TranslateManager.Instance.GetText(translateId), _text);
    }

    public void SetTextByTranslateId(string _id){
        setupTarget();
        var _text = TranslateManager.Instance.GetText(_id);
        if(string.IsNullOrEmpty(_text) == false){
            this.target.text = _text;
        }
    }

    private void setupTarget(){
        if(target == null){
            target = GetComponent<UnityEngine.UI.Text>();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BicUtil.Selector{
    public class SelectingTag : MonoBehaviour
    {
        [SerializeField]
        private string[] tags;
        [SerializeField]
        private bool setupOnAwake = false;

        public string[] Tags{get=>this.tags;}

        public bool HasClass(string _tag){
            if(tags == null){
                return false;
            }
            
            return tags.Contains(_tag);        
        }
    }
}
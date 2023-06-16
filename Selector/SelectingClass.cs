using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BicUtil.Selector{
    public class SelectingClass : MonoBehaviour
    {
        [SerializeField]
        private string[] selectingName;

        public bool HasClass(string _className){
            return selectingName.Contains(_className);        
        }
    }
}
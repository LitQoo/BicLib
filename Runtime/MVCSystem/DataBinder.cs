using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicDB.Variable;
using System;
using BicDB;
using BicDB.Container;
using BicUtil.ListValueSelector;

namespace BicUtil.MVCSystem
{
	public class DataBinder<T> : ModelBinder where T : class
	{
        public T Data{
            get=>data;
        }

        private T data;

        public void Bind(T _data){
            if(this.data != null){
                this.ClearBinding();
            }
            
            this.data = _data;
        }

        public new void ClearBinding(){
            base.ClearBinding();
            this.data = null;
        }
    }
}

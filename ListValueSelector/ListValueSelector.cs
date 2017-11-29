using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;

namespace BicUtil.ListValueSelector
{
    public class ListValueSelector<T> where T : class
    {
        private int index = 0;
        private IList<T> list;

        public ObjectContainer<T> Current = new ObjectContainer<T>();

        public ListValueSelector(IList<T> _list){
            list = _list;
        }

        public void SelectNext(){
            SelectAt(index + 1);
        }

        public bool HasNext(){
            return list.Count > index;
        }

        public void SelectPrev(){
            SelectAt(index - 1);
        }

        public bool HasPrev(){
            return index > 0;
        }

        public void SelectFirst(){
            SelectAt(0);
        }

        public void  SelectLast(){
            SelectAt(list.Count - 1);
        }

        public void SelectAt(int _index){
            index = _index;
            if(list.Count > index){
                Current.AsObject = list[index];
            }else{
                UnityEngine.Debug.Log("overload");
                Current.AsObject = null;
            }
        }

        public void SelectFindFirst(Func<T, bool> _finder){
            for(int i = 0; i < list.Count; i++){
                if(_finder(list[i])){
                    SelectAt(i);
                    return;
                }
            }

            Current.AsObject = null;
        }

        public void SelectFindLast(Func<T, bool> _finder){
            for(int i = list.Count - 1; i >= 0; i--){
                if(_finder(list[i])){
                    SelectAt(i);
                    return;
                }
            }

            Current.AsObject = null;
        }
    }
}
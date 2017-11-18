using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;

namespace BicUtil.ListIterator
{
    public class ListIterator<T> where T : class
    {
        private int index = 0;
        private IList<T> list;

        public ObjectContainer<T> Current = new ObjectContainer<T>();

        public ListIterator(IList<T> _list){
            list = _list;
        }

        public void MoveNext(){
            MoveTo(index + 1);
        }

        public bool HasNext(){
            return list.Count > index;
        }

        public void MovePrev(){
            MoveTo(index - 1);
        }

        public bool HasPrev(){
            return index > 0;
        }

        public void MoveFirst(){
            MoveTo(0);
        }

        public void  MoveLast(){
            MoveTo(list.Count - 1);
        }

        public void MoveTo(int _index){
            index = _index;
            Current.AsObject = list[index];
        }

        public void MoveFindFirst(Func<T, bool> _finder){
            for(int i = 0; i < list.Count; i++){
                if(_finder(list[i])){
                    MoveTo(i);
                    return;
                }
            }

            Current.AsObject = null;
        }

        public void MoveFindLast(Func<T, bool> _finder){
            for(int i = list.Count - 1; i >= 0; i--){
                if(_finder(list[i])){
                    MoveTo(i);
                    return;
                }
            }

            Current.AsObject = null;
        }
    }
}
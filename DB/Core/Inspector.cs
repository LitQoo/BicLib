using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicDB.Core
{
    public interface IInspector
    {
        void Track(string _id, object _object);
    }

    public class DummyInspector : IInspector{
        public void Track(string _id, object _object){

        }
    }

    public class Inspector : IInspector
    {
        public Inspector(){
            Debug.Log("BicDB Inspector Ready");
        }

        public Dictionary<string, object> TrackingList = new Dictionary<string, object>();

        public void Track(string _id, object _object){
            TrackingList[_id] = _object;
        }
        
        public void CancelToTrack(string _id){
            TrackingList.Remove(_id);
        } 
    }
}
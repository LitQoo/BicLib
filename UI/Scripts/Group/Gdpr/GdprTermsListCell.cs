using System.Collections;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicUtil.Disabled{
    public class GdprTermsListCell : MonoBehaviour, BicUtil.TableView.ITableCellWithBinder
    {
        #region DI
        [SerializeField]
        private UnityEngine.UI.Text text;
        #endregion

        public void Bind(IRecordContainer _data, string _msg)
        {
            text.text = _data["url"].AsVariable.AsString;
        }

        public void Unbind(IRecordContainer _data, string _msg)
        {
            
        }
    }
}
using System.Linq;
using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using BicUtil.Tween;

namespace BicUtil.TableView
{
    public class CollectionRow : TableRow
    {
        public static float WIDTH = -1;
        #region DI
        [SerializeField]
        private VerticalLayoutGroup[] verticals;
        [SerializeField]
        private RectTransform tableContentRectTransform;
        #endregion

        #region Logic
        public override void InitializeCells(){
            if(cellCount > 0){
                var _tableCell = transform.GetComponentInChildren<TableCell>();
                var _childs = transform.GetComponentsInChildren<TableCell>();
                var _cellCount = _childs.Length;

                for(int i = _cellCount; i < cellCount; i++){
                    var _cell = Instantiate(_tableCell.gameObject);
                    _cell.transform.SetParent(verticals[0].transform);
                    _cell.transform.SetSiblingIndex(_tableCell.transform.GetSiblingIndex());
                }

                Cells.Clear();
            }

            if(Cells.Count <= 0){
                var _childs = transform.GetComponentsInChildren<TableCell>();
                for(int i = 0; i < _childs.Length; i++){
                    var _tableCell = _childs[i];
                    if(_tableCell != null){
                        Cells.Add(_tableCell);
                    }
                }
            }

            cellCount = Cells.Count;
        }

        public override void SetData(int _startCellIndex, Func<int, int, int, IRecordContainer> _cellDataFunc, ITableViewDataSource _dataSource){
            if(WIDTH < 0){
                WIDTH = RectTransformUtility.CalculateRelativeRectTransformBounds(this.tableContentRectTransform).size.x;
            }
            var _cellCount = _dataSource.GetCellCountInRow(this.RowIndex);
            var _arrangeInfo = _dataSource.GetArrangeInfoInRow(this.RowIndex);

            for(int v = 0; v < verticals.Length; v++){
                if(v < _arrangeInfo.VerticalCount){
                    verticals[v].gameObject.SetActive(true);
                    var _rectTransform = ((RectTransform)verticals[v].transform);
                    _rectTransform.sizeDelta = new Vector2(WIDTH / 3f * _arrangeInfo.Data[v][0].x, _rectTransform.sizeDelta.y);
                }else{
                    verticals[v].gameObject.SetActive(false);
                }
            }   

            var _usingCells = new List<TableCell>();
            for(int i = 0; i < Cells.Count; i++){
                if(_cellCount > i){
                    var _model = _cellDataFunc(_startCellIndex + i, RowIndex, i);
                    if(_model != null){
                        Cells[i].gameObject.SetActive(true);
                        Cells[i].Model = _model;
                        _usingCells.Add(Cells[i]);
                    }else{
                        Cells[i].gameObject.SetActive(false);
                    }
                }else{
                    Cells[i].gameObject.SetActive(false);
                }
            }

            int _currnetVerticalIndex = 0;
            int _currnetHorizontalIndex = 0;

            for(int i = 0; i < _usingCells.Count; i++){
                _usingCells[i].transform.SetParent(verticals[_currnetVerticalIndex].transform);

                if(_arrangeInfo.Data[_currnetVerticalIndex].Length - 1 > _currnetHorizontalIndex){
                    _currnetHorizontalIndex++;
                }else{
                    _currnetVerticalIndex++;
                    _currnetHorizontalIndex = 0;
                }
            }

            OnCreateFunction.Invoke(this);
        }
        #endregion
    }
}
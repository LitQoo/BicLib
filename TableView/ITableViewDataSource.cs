using UnityEngine;
using System.Collections;
using BicDB.Container;

namespace BicUtil.TableView
{
    public interface ITableViewDataSource
    {
        int GetNumberOfCellsForTableView();
        float GetHeightForRowInTableView(int _row);
        TableRow GetCellForRowInTableView(int _row);
        IRecordContainer GetCellData(int _index);
        int GetCellCountInRow(int _rowIndex);
        int GetRowCount();
        int GetStartDataIndex(int _rowIndex);
        void ReloadData();
    }
}


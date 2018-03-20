using UnityEngine;
using System.Collections;

namespace BicUtil.TableView
{
    /// <summary>
    /// An interface for a data source to a TableView
    /// </summary>
    public interface ITableViewDataSource
    {
        /// <summary>
        /// Get the number of rows that a certain table should display
        /// </summary>
        int GetNumberOfCellsForTableView(TableView _tableView);
        
        /// <summary>
        /// Get the height of a row of a certain cell in the table view
        /// </summary>
        float GetHeightForRowInTableView(TableView _tableView, int _row);

        /// <summary>
        /// Create a cell for a certain row in a table view.
        /// Callers should use tableView.GetReusableCell to cache objects
        /// </summary>
        TableRow GetCellForRowInTableView(TableView _tableView, int _row);
    }
}


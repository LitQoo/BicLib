using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using BicDB.Container;
using BicDB;
using System;
using BicUtil.Tween;

namespace BicUtil.TableView
{
    /// <summary>
    /// A reusable table for for (vertical) tables. API inspired by Cocoa's UITableView
    /// Hierarchy structure should be :
    /// GameObject + TableView (this) + Mask + Scroll Rect (point to child)
    /// - Child GameObject + Vertical Layout Group
    /// This class should be after Unity's internal UI components in the Script Execution Order
    /// </summary>
	[RequireComponent(typeof(EventControlledScrollRect))]
    public class TableView : MonoBehaviour
    {

        #region Public API
        /// <summary>
        /// The data source that will feed this table view with information. Required.
        /// </summary>

		public void SetDBSource<T>(IList<T> _table) where T : IRecordContainer, new(){
			GetCellDataFunc = (int _row) => {
				return _table[_row];	
			};

			GetTableSizeFunc = () => {
				return _table.Count;
			};

			DataSource = new TableDataSourceAuto();
		}

        [System.Serializable]
        public class CellVisibilityChangeEvent : UnityEvent<int, bool> { }
        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the row index, second param (bool) is whether or not it is visible
        /// </summary>
        public CellVisibilityChangeEvent onCellVisibilityChanged;

        /// <summary>
        /// Get a cell that is no longer in use for reusing
        /// </summary>
        /// <param name="reuseIdentifier">The identifier for the cell type</param>
        /// <returns>A prepared cell if available, null if none</returns>
        public TableCell GetReusableCell(string reuseIdentifier) {
			
            LinkedList<TableCell> cells;
            if (!m_reusableCells.TryGetValue(reuseIdentifier, out cells)) {
                return null;
            }
            if (cells.Count == 0) {
                return null;
            }
            TableCell cell = cells.First.Value;
            cells.RemoveFirst();
            return cell;
        }

		public Dictionary<string, Action<IRecordContainer>> OnClickedActions = new Dictionary<string, Action<IRecordContainer>>();
		public TableCell CreateTableCell(){
			TableCell cell = GetReusableCell(tableCell.reuseIdentifier);

			if (cell == null) {
				cell = (TableCell)GameObject.Instantiate(tableCell);
				cell.name = "CellInstance";
				cell.gameObject.SetActive(true);
				cell.OnClickedActions = OnClickedActions;
			}



			return cell;
		}

		public int GetTableSize(){
			if (GetTableSizeFunc == null) {
				return 0;
			}

			return GetTableSizeFunc();
		}

		public float GetCellHeight(){
			return m_rowHeight;
		}

        public bool isEmpty { get; private set; }

        /// <summary>
        /// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
        /// (number of rows changed, etc)
        /// </summary>
        public void ReloadData() {
            m_cellSizes = new float[m_dataSource.GetNumberOfRowsForTableView(this)];
            this.isEmpty = m_cellSizes.Length == 0;

            if (this.isEmpty) {
                ClearAllRows();
                return;
            }
            m_cumulativeCellSizes = new float[m_cellSizes.Length];
            m_cleanCumulativeIndex = -1;

            for (int i = 0; i < m_cellSizes.Length; i++) {
                m_cellSizes[i] = m_dataSource.GetHeightForRowInTableView(this, i);
                if (i > 0) {
                    m_cellSizes[i] += m_LayoutGroup.spacing;
                }
            }

			if(m_isVertical) {
				m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x, 
					GetCumulativeRowHeight(m_cellSizes.Length - 1) + m_LayoutGroup.padding.top + m_LayoutGroup.padding.bottom);
			} else {
				m_scrollRect.content.sizeDelta = new Vector2(GetCumulativeRowHeight(m_cellSizes.Length - 1) + m_LayoutGroup.padding.left + m_LayoutGroup.padding.right, m_scrollRect.content.sizeDelta.y);
			}
            RecalculateVisibleRowsFromScratch();
            m_requiresReload = false;
        }

        /// <summary>
        /// Get cell at a specific row (if active). Returns null if not.
        /// </summary>
        public TableCell GetCellAtRow(int row)
        {
            TableCell retVal = null;
            m_visibleCells.TryGetValue(row, out retVal);
            return retVal;
        }

        /// <summary>
        /// Get the range of the currently visible rows
        /// </summary>
        public Range visibleRowRange {
            get { return m_visibleCellRange; }
        }

        /// <summary>
        /// Notify the table view that one of its rows changed size
        /// </summary>
        public void NotifyCellDimensionsChanged(int row) {
            float oldHeight = m_cellSizes[row];
            m_cellSizes[row] = m_dataSource.GetHeightForRowInTableView(this, row);
            m_cleanCumulativeIndex = Mathf.Min(m_cleanCumulativeIndex, row - 1);
            if (m_visibleCellRange.Contains(row)) {
                TableCell cell = GetCellAtRow(row);

				if(m_isVertical) {
					cell.GetComponent<LayoutElement>().preferredHeight = m_cellSizes[row];
					if(row > 0) {
						cell.GetComponent<LayoutElement>().preferredHeight -= m_LayoutGroup.spacing;
					}
				} else {
					cell.GetComponent<LayoutElement>().preferredWidth = m_cellSizes[row];
					if(row > 0) {
						cell.GetComponent<LayoutElement>().preferredWidth -= m_LayoutGroup.spacing;
					}
				}
            }


            float heightDelta = m_cellSizes[row] - oldHeight;
            
			if(m_isVertical) {
				m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x,
					m_scrollRect.content.sizeDelta.y + heightDelta);
			} else {
				m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x + heightDelta, m_scrollRect.content.sizeDelta.y);
			}

            m_requiresRefresh = true;
        }

        /// <summary>
        /// Get the maximum scrollable height of the table. scrollY property will never be more than this.
        /// </summary>
        public float scrollableDistance {
            get {
				if(m_isVertical) {
					return m_scrollRect.content.rect.height - (this.transform as RectTransform).rect.height;
				} else {
					return m_scrollRect.content.rect.width - (this.transform as RectTransform).rect.width;
				}
            }
        }

        /// <summary>
        /// Get or set the current scrolling position of the table
        /// </summary>
        public float scrollDistance {
            get {
				return m_scrollDistance;
            }
            set {
                if (this.isEmpty) {
                    return;
                }
                value = Mathf.Clamp(value, 0, GetScrollYForRow(m_cellSizes.Length - 1, true));
                if (m_scrollDistance != value) {
                    m_scrollDistance = value;
                    m_requiresRefresh = true;

					float relativeScroll = value / this.scrollableDistance;

					if(m_isVertical) {
						m_scrollRect.verticalNormalizedPosition = 1 - relativeScroll;
					} else {
						m_scrollRect.horizontalNormalizedPosition = relativeScroll;
					}
                }
            }
        }

        /// <summary>
        /// Get the y that the table would need to scroll to to have a certain row at the top
        /// </summary>
        /// <param name="row">The desired row</param>
        /// <param name="above">Should the top of the table be above the row or below the row?</param>
        /// <returns>The y position to scroll to, can be used with scrollY property</returns>
        public float GetScrollYForRow(int row, bool above) {
            float retVal = GetCumulativeRowHeight(row);
            if (above) {
                retVal -= m_cellSizes[row];
            }
            return retVal;
        }

        #endregion

        #region Private implementation

		[SerializeField]
		private TableCell tableCell;
        private ITableViewDataSource m_dataSource;
        private bool m_requiresReload;

		private HorizontalOrVerticalLayoutGroup m_LayoutGroup;
		private EventControlledScrollRect m_scrollRect;
        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;

		private float[] m_cellSizes;
        //cumulative[i] = sum(rowHeights[j] for 0 <= j <= i)
		private float[] m_cumulativeCellSizes;
        private int m_cleanCumulativeIndex;

        private Dictionary<int, TableCell> m_visibleCells;
		private Range m_visibleCellRange;

        private RectTransform m_reusableCellContainer;
        private Dictionary<string, LinkedList<TableCell>> m_reusableCells;

        private float m_scrollDistance;

        private bool m_requiresRefresh;

		private bool m_isVertical;
		private float m_rowHeight;

		public ITableViewDataSource DataSource
		{
			get { return m_dataSource; }
			set { m_dataSource = value; m_requiresReload = true; }
		}

		private Func<int, IRecordContainer> GetCellDataFunc = null;
		private Func<int> GetTableSizeFunc = null;

        private void ScrollViewValueChanged(Vector2 newScrollValue) {
			if(m_isVertical) {
				float relativeScroll = 1 - newScrollValue.y;
				m_scrollDistance = relativeScroll * scrollableDistance;
			} else {
				float relativeScroll = newScrollValue.x;
				m_scrollDistance = relativeScroll * scrollableDistance;
			}

            m_requiresRefresh = true;
        }

        private void RecalculateVisibleRowsFromScratch() {
            ClearAllRows();
            SetInitialVisibleRows();
        }

        private void ClearAllRows() {
            while (m_visibleCells.Count > 0) {
                HideRow(false);
            }
            m_visibleCellRange = new Range(0, 0);
        }

        void Awake()
        {
			m_isVertical = true;
            isEmpty = true;
			m_scrollRect = GetComponent<EventControlledScrollRect>();
			m_LayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

			if(m_LayoutGroup == null) {
				m_LayoutGroup = GetComponentInChildren<HorizontalLayoutGroup>();
				m_isVertical = false;
			}

			if(m_LayoutGroup == null) {
				throw new System.Exception("m_verticalLayoutGroup is null");
			}

			m_rowHeight = 100;

			if (tableCell != null) {
				tableCell.gameObject.SetActive(false);

				if (m_isVertical) {
					m_rowHeight = tableCell.GetComponent<RectTransform>().sizeDelta.y;
				} else {
					m_rowHeight = tableCell.GetComponent<RectTransform>().sizeDelta.x;
				}
			}

            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_topPadding.transform.SetParent(m_scrollRect.content, false);
            m_bottomPadding = CreateEmptyPaddingElement("Bottom");
            m_bottomPadding.transform.SetParent(m_scrollRect.content, false);
            m_visibleCells = new Dictionary<int, TableCell>();

            m_reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
            m_reusableCellContainer.SetParent(this.transform, false);
            m_reusableCellContainer.gameObject.SetActive(false);
            m_reusableCells = new Dictionary<string, LinkedList<TableCell>>();
        }
        
        void Update()
        {
            if (m_requiresReload) {
                ReloadData();
            }
        }

        void LateUpdate() {
            if (m_requiresRefresh) {
                RefreshVisibleRows();
            }

			if (centerPositionMagnet == true) {
				checkEnableMagnet ();
			}
        }

        void OnEnable() {
            m_scrollRect.onValueChanged.AddListener(ScrollViewValueChanged);
			m_scrollRect.OnBeginDragActions.AddListener (isControlledTrue);
			m_scrollRect.OnEndDragActions.AddListener (isControlledFalse);
        }

        void OnDisable() {
			m_scrollRect.onValueChanged.RemoveListener(ScrollViewValueChanged);
			m_scrollRect.OnBeginDragActions.RemoveListener (isControlledTrue);
			m_scrollRect.OnEndDragActions.RemoveListener (isControlledFalse);
        }

		bool isControlled = false;
		private void isControlledTrue(){
			isControlled = true;
            if(scrollTween != null){
                scrollTween.Cancel();
                scrollTween = null;
            }
		}

		private void isControlledFalse(){
			isControlled = false;
		}


		#region MangetControl
		[SerializeField]
		private bool centerPositionMagnet = false;
		float lastScrollDistance = 0;

		private void checkEnableMagnet(){
			if (isControlled == false) {
				if (scrollDistance >= 0 && scrollDistance <= scrollableDistance) {
					float _gap = lastScrollDistance - scrollDistance;
					if (Mathf.Abs (_gap) < 4f) {
						magnetControl (_gap);
						isControlled = true;
					}
				}
			}

			lastScrollDistance = scrollDistance;
		}

        private TweenModel scrollTween = null;
		private void magnetControl(float _gap){
			var _centerPosition = m_scrollRect.transform.position;
			foreach (var _cell in m_visibleCells) {
				var _cellPosition = _cell.Value.transform.position;
				var _cellHalfSize = m_cellSizes [_cell.Key] / 2f;
				if (_cellPosition.x + _cellHalfSize > _centerPosition.x && _cellPosition.x - _cellHalfSize <= _centerPosition.x) {
					var _targetPosition = scrollDistance - (_centerPosition.x - _cellPosition.x);
					float _targetGap = _targetPosition - scrollDistance;
					var _speed = Mathf.Abs(_targetGap) / 100f;
					scrollTween = BicTween.Value (scrollDistance, _targetPosition, _speed).SetEase(EaseType.OutBack).SubscribeUpdate(_value => {
						scrollDistance = _value.x;
					});
					break;
				}
			}
		}
		#endregion
        
        private Range CalculateCurrentVisibleRowRange()
        {
            float startY = 0;
			float endY = 0;
            
			if (m_isVertical) {
				startY = m_scrollDistance - m_LayoutGroup.padding.top;
				endY = m_scrollDistance + (this.transform as RectTransform).rect.height + m_LayoutGroup.padding.bottom;
			}else if(!m_isVertical) {
				startY = m_scrollDistance - m_LayoutGroup.padding.left;
				endY = m_scrollDistance + (this.transform as RectTransform).rect.width + m_LayoutGroup.padding.right;
			}

			int startIndex = FindIndexOfRowAtY(startY);
            int endIndex = FindIndexOfRowAtY(endY);


			return new Range(startIndex, endIndex - startIndex + 1);
        }

        private void SetInitialVisibleRows()
        {
            Range visibleRows = CalculateCurrentVisibleRowRange();
            for (int i = 0; i < visibleRows.count; i++)
            {
                AddRow(visibleRows.from + i, true);
            }
            m_visibleCellRange = visibleRows;
            UpdatePaddingElements();
        }

        private void AddRow(int row, bool atEnd)
        {
            TableCell newCell = m_dataSource.GetCellForRowInTableView(this, row);

			if (GetCellDataFunc != null) {
				newCell.Model = GetCellDataFunc(row);
			}

            newCell.transform.SetParent(m_scrollRect.content, false);

            LayoutElement layoutElement = newCell.GetComponent<LayoutElement>();
            if (layoutElement == null) {
                layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            }

			if(m_isVertical) {
				layoutElement.preferredHeight = m_cellSizes[row];
				if(row > 0) {
					layoutElement.preferredHeight -= m_LayoutGroup.spacing;
				}
			} else {
				layoutElement.preferredWidth = m_cellSizes[row];
				if(row > 0) {
					layoutElement.preferredWidth -= m_LayoutGroup.spacing;
				}
			}
            
            m_visibleCells[row] = newCell;

            if (atEnd) {
                newCell.transform.SetSiblingIndex(m_scrollRect.content.childCount - 2); //One before bottom padding
            } else {
                newCell.transform.SetSiblingIndex(1); //One after the top padding
            }

			this.onCellVisibilityChanged.Invoke(row, true);

        }

        private void RefreshVisibleRows()
        {
            m_requiresRefresh = false;

            if (this.isEmpty) {
                return;
            }

            Range newVisibleRows = CalculateCurrentVisibleRowRange();

			int oldTo = m_visibleCellRange.Last();
            int newTo = newVisibleRows.Last();

            if (newVisibleRows.from > oldTo || newTo < m_visibleCellRange.from) {
                //We jumped to a completely different segment this frame, destroy all and recreate
				RecalculateVisibleRowsFromScratch();
                return;
            }

            //Remove rows that disappeared to the top
            for (int i = m_visibleCellRange.from; i < newVisibleRows.from; i++)
            {
                HideRow(false);
            }
            //Remove rows that disappeared to the bottom
            for (int i = newTo; i < oldTo; i++)
            {
                HideRow(true);
            }
            //Add rows that appeared on top
            for (int i = m_visibleCellRange.from - 1; i >= newVisibleRows.from; i--) {
                AddRow(i, false);
            }
            //Add rows that appeared on bottom
            for (int i = oldTo + 1; i <= newTo; i++) {
                AddRow(i, true);
            }
            m_visibleCellRange = newVisibleRows;
            UpdatePaddingElements();
        }

        private void UpdatePaddingElements() {
            float hiddenElementsHeightSum = 0;
            for (int i = 0; i < m_visibleCellRange.from; i++) {
                hiddenElementsHeightSum += m_cellSizes[i];
            }

			if(m_isVertical) {
				m_topPadding.preferredHeight = hiddenElementsHeightSum - m_LayoutGroup.padding.top;
				m_topPadding.gameObject.SetActive(m_topPadding.preferredHeight > 0);
			} else {
				m_topPadding.preferredWidth = hiddenElementsHeightSum - m_LayoutGroup.padding.left;
				m_topPadding.gameObject.SetActive(m_topPadding.preferredWidth > 0);
			}


			for (int i = m_visibleCellRange.from; i <= m_visibleCellRange.Last(); i++) {
                hiddenElementsHeightSum += m_cellSizes[i];
            }

			if(m_isVertical) {
				float bottomPaddingHeight = m_scrollRect.content.rect.height - hiddenElementsHeightSum;
				m_bottomPadding.preferredHeight = bottomPaddingHeight - m_LayoutGroup.spacing - m_LayoutGroup.padding.top - m_LayoutGroup.padding.bottom;
				m_bottomPadding.gameObject.SetActive(m_bottomPadding.preferredHeight > 0);
			} else {
				float bottomPaddingHeight = m_scrollRect.content.rect.width - hiddenElementsHeightSum;
				m_bottomPadding.preferredWidth = bottomPaddingHeight - m_LayoutGroup.spacing - m_LayoutGroup.padding.left- m_LayoutGroup.padding.right;
				m_bottomPadding.gameObject.SetActive(m_bottomPadding.preferredWidth > 0);
			}
        }

        private void HideRow(bool last)
        {
            int row = last ? m_visibleCellRange.Last() : m_visibleCellRange.from;
            TableCell removedCell = m_visibleCells[row];
			removedCell.OnRemove();
            StoreCellForReuse(removedCell);
            m_visibleCells.Remove(row);
            m_visibleCellRange.count -= 1;
            if (!last) {
                m_visibleCellRange.from += 1;
            } 
            this.onCellVisibilityChanged.Invoke(row, false);
        }

        private LayoutElement CreateEmptyPaddingElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }

        private int FindIndexOfRowAtY(float y) {
            //TODO : Binary search if inside clean cumulative row height area, else walk until found.
            return FindIndexOfRowAtY(y, 0, m_cumulativeCellSizes.Length - 1);
        }

        private int FindIndexOfRowAtY(float y, int startIndex, int endIndex) {
			if (startIndex >= endIndex) {
                return startIndex;
            }
            int midIndex = (startIndex + endIndex) / 2;

            if (GetCumulativeRowHeight(midIndex) >= y) {
                return FindIndexOfRowAtY(y, startIndex, midIndex);
            } else {
                return FindIndexOfRowAtY(y, midIndex + 1, endIndex);
            }
        }

        private float GetCumulativeRowHeight(int row) {
            while (m_cleanCumulativeIndex < row) {
                m_cleanCumulativeIndex++;
				m_cumulativeCellSizes[m_cleanCumulativeIndex] = m_cellSizes[m_cleanCumulativeIndex];
				 if (m_cleanCumulativeIndex > 0) {
                    m_cumulativeCellSizes[m_cleanCumulativeIndex] += m_cumulativeCellSizes[m_cleanCumulativeIndex - 1];
                } 
            }
            return m_cumulativeCellSizes[row];
        }

        private void StoreCellForReuse(TableCell cell) {
            string reuseIdentifier = cell.reuseIdentifier;
            
            if (string.IsNullOrEmpty(reuseIdentifier)) {
                GameObject.Destroy(cell.gameObject);
                return;
            }

            if (!m_reusableCells.ContainsKey(reuseIdentifier)) {
                m_reusableCells.Add(reuseIdentifier, new LinkedList<TableCell>());
            }

            m_reusableCells[reuseIdentifier].AddLast(cell);
            cell.transform.SetParent(m_reusableCellContainer, false);
        }

        #endregion


        
    }

	internal class TableDataSourceAuto : ITableViewDataSource{
		public int GetNumberOfRowsForTableView(TableView _tableView)
		{
			return _tableView.GetTableSize();
		}

		public float GetHeightForRowInTableView(TableView _tableView, int _row)
		{
			return _tableView.GetCellHeight();
		}

		public TableCell GetCellForRowInTableView(TableView _tableView, int _row)
		{
			var _tableCell = _tableView.CreateTableCell(); // 셀 리턴
			_tableCell.CellIndex = _row;
			return _tableCell;
		}
	}

    internal static class RangeExtensions
    {
        public static int Last(this Range range)
        {
            if (range.count == 0)
            {
                throw new System.InvalidOperationException("Empty range has no to()");
            }
            return (range.from + range.count - 1);
        }

        public static bool Contains(this Range range, int num) {
            return num >= range.from && num < (range.from + range.count);
        }
    }

}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using BicDB.Container;
using BicDB;
using System;
using BicUtil.Tween;
using UnityEngine.Serialization;

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

		public void SetDBSource<T>(IList<T> _table, Func<TableView, IList<T>, int, float> _getRowHeightFunc) where T : IRecordContainer, new(){
            DataSource = new TableDataSourceAuto<T>(this, _table, _getRowHeightFunc);
        }

        public void SetDBSource<T>(IList<T> _table, string _headRowName = "", string _footRowName = "", Func<TableView, IList<T>, int, float> _getRowHeightFunc = null) where T : IRecordContainer, new(){
            var _dataSource = new TableDataSourceAuto<T>(this, _table, _getRowHeightFunc);
            _dataSource.SetHeadRow(_headRowName);
            _dataSource.SetFootRow(_footRowName);
            DataSource = _dataSource;
        }

        public void SetDBSource<T>(IList<T> _table, Func<CollectionRowData[]> _collectionArrangeBuilder, string _headRowName = "", string _footRowName = "", Func<TableView, IList<T>, int, float> _getRowHeightFunc = null) where T : IRecordContainer, new(){
            var _dataSource = new TableDataSourceAuto<T>(this, _table, _getRowHeightFunc);
            _dataSource.ArrangeInfoBuilder = _collectionArrangeBuilder;
            _dataSource.SetHeadRow(_headRowName);
            _dataSource.SetFootRow(_footRowName);
            DataSource = _dataSource;
        }

        [System.Serializable]
        public class RowVisibilityChangeEvent : UnityEvent<int, bool> { }
        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the row index, second param (bool) is whether or not it is visible
        /// </summary>
        public RowVisibilityChangeEvent onRowVisibilityChanged;

        /// <summary>
        /// Get a cell that is no longer in use for reusing
        /// </summary>
        /// <param name="reuseIdentifier">The identifier for the cell type</param>
        /// <returns>A prepared cell if available, null if none</returns>
        public TableRow GetReusableRow(string reuseIdentifier) {
			
            LinkedList<TableRow> _rows;
            if (!m_reusableRows.TryGetValue(reuseIdentifier, out _rows)) {
                return null;
            }

            if (_rows.Count == 0) {
                return null;
            }

            TableRow _row = _rows.First.Value;
            _rows.RemoveFirst();
            return _row;
        }
        
        private Dictionary<string, Action<IRecordContainer>> onClickedCellActions = new Dictionary<string, Action<IRecordContainer>>();
        public void AddClickCellEvent(string _buttonName, Action<IRecordContainer> _action){
            onClickedCellActions[_buttonName] = _action;
        }

		public TableRow CreateTableRow(string _reuseIdentifier){
			TableRow _row = GetReusableRow(_reuseIdentifier);

			if (_row == null) {
                if(string.IsNullOrEmpty(_reuseIdentifier) == false)
                {
                    _row = (TableRow)GameObject.Instantiate(getTableRowForCopy(_reuseIdentifier));
                }
                else
                {
                    _row = (TableRow)GameObject.Instantiate(tableRowDefault);
                }

                if(_row == null){
                    throw new SystemException("[TableView] NotFound TableRow " + _reuseIdentifier);
                }

                _row.transform.SetParent(m_scrollRect.content); 
				_row.BindOnClickedEvent(onClickedCellActions);
				_row.name = "RowInstance";
				_row.gameObject.SetActive(true);
			}

			return _row;
		}

        private TableRow getTableRowForCopy(string _reuseIdentifier)
        {
            for (int i = 0; i < tableRows.Count; i++)
            {
                if (tableRows[i].ReuseIdentifier == _reuseIdentifier)
                {
                    return tableRows[i];
                }
            }

            return null;
        }

        public int GetTableSize(){
			return DataSource.GetNumberOfCellsForTableView();
		}

        [Obsolete("use GetDefaultRowHeight")]
		public float GetRowHeight(){
			return m_defaultRowHeight;
		}

        public float GetRowHeight(string _rowName){
            var _row = getTableRowForCopy(_rowName);
            var _height = _row.GetComponent<RectTransform>().sizeDelta.y;
            return _height; 
        }

        public float GetDefaultRowHeight(){
            return m_defaultRowHeight;
        }



        public void StopScrollMovement(){
            m_scrollRect.StopMovement();
        }

        public bool isEmpty { get; private set; }

        /// <summary>
        /// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
        /// (number of rows changed, etc)
        /// </summary>
        public void ReloadData()
        {
            DataSource.ReloadData();
            m_rowSizes = new float[DataSource.GetRowCount()];
            this.isEmpty = m_rowSizes.Length == 0;

            if (this.isEmpty)
            {
                ClearAllRows();
                return;
            }
            m_cumulativeRowSizes = new float[m_rowSizes.Length];
            m_cleanCumulativeIndex = -1;

            for (int i = 0; i < m_rowSizes.Length; i++)
            {
                m_rowSizes[i] = m_dataSource.GetHeightForRowInTableView(i);
                if (i > 0)
                {
                    m_rowSizes[i] += m_LayoutGroup.spacing;
                }
            }

            RecalculateContentSize();

            RecalculateVisibleRowsFromScratch();
            m_requiresReload = false;

            if(onReloadData != null){
                onReloadData();
            }
            // scrollDistance = 1;
            // scrollDistance = 0; 

        }

        private void RecalculateContentSize()
        {
            if (m_isVertical)
            {
                var _height = GetCumulativeRowHeight(m_rowSizes.Length - 1) + m_LayoutGroup.padding.top + m_LayoutGroup.padding.bottom;
                m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x, _height);
            }
            else
            {
                m_scrollRect.content.sizeDelta = new Vector2(GetCumulativeRowHeight(m_rowSizes.Length - 1) + m_LayoutGroup.padding.left + m_LayoutGroup.padding.right, m_scrollRect.content.sizeDelta.y);
            }
        }

        /// <summary>
        /// Get row at a specific row (if active). Returns null if not.
        /// </summary>
        public TableRow GetRow(int _rowIndex)
        {
            TableRow _result = null;
            m_visibleRows.TryGetValue(_rowIndex, out _result);
            return _result;
        }

        public TableRow GetHeadRow(){
            var _row = this.GetRow(0); 
            
            if(_row == null){
                _row = GetReusableRow(this.m_dataSource.HeadRowName);
            }

            if(_row == null){
                _row = getTableRowForCopy(this.DataSource.HeadRowName);
            }

            return _row;
        }

        public string PrintVisibleRows(){
            var _string = "{";
            foreach(var _value in m_visibleRows){
                _string += _value.Key.ToString() + ":" + _value.Value.Cells.Count.ToString() + "/";
            }
            _string += "}";
            return _string;
        }

        /// <summary>
        /// Get the range of the currently visible rows
        /// </summary>
        public UnityEngine.SocialPlatforms.Range visibleRowRange {
            get { return m_visibleRowRange; }
        }

        public string defaultReusableRowId = "tableRow";

        /// <summary>
        /// Notify the table view that one of its rows changed size
        /// </summary>
        public void NotifyRowDimensionsChanged(int _rowIndex) {
            float oldHeight = m_rowSizes[_rowIndex];
            m_rowSizes[_rowIndex] = m_dataSource.GetHeightForRowInTableView(_rowIndex);
            m_cleanCumulativeIndex = Mathf.Min(m_cleanCumulativeIndex, _rowIndex - 1);
            if (m_visibleRowRange.Contains(_rowIndex)) {
                TableRow _row = GetRow(_rowIndex);

				if(m_isVertical) {
					_row.GetComponent<LayoutElement>().preferredHeight = m_rowSizes[_rowIndex];
					if(_rowIndex > 0) {
						_row.GetComponent<LayoutElement>().preferredHeight -= m_LayoutGroup.spacing;
					}
				} else {
					_row.GetComponent<LayoutElement>().preferredWidth = m_rowSizes[_rowIndex];
					if(_rowIndex > 0) {
						_row.GetComponent<LayoutElement>().preferredWidth -= m_LayoutGroup.spacing;
					}
				}
            }

            float heightDelta = m_rowSizes[_rowIndex] - oldHeight;
            
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
					return Mathf.Max(0, m_scrollRect.content.rect.height - (this.transform as RectTransform).rect.height);
				} else {
					return Mathf.Max(0, m_scrollRect.content.rect.width - (this.transform as RectTransform).rect.width);
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

                if(m_scrollRect.movementType != ScrollRect.MovementType.Unrestricted){
                    value = Mathf.Clamp(value, 0, GetScrollYForRow(m_rowSizes.Length - 1, true));
                }

                if (m_scrollDistance != value) {
                    m_scrollDistance = value;
                    m_requiresRefresh = true;
                    m_requiresCallScrollChagned = true;

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
        /// <param name="_rowIndex">The desired row</param>
        /// <param name="_above">Should the top of the table be above the row or below the row?</param>
        /// <returns>The y position to scroll to, can be used with scrollY property</returns>
        public float GetScrollYForRow(int _rowIndex, bool _above) {
            float retVal = GetCumulativeRowHeight(_rowIndex);
            if (_above) {
                retVal -= m_rowSizes[_rowIndex];
            }
            return retVal;
        }

        public float GetScrollYForCell(int _cellIndex, bool _above){
            int _cellCount = 0;
            int _rowIndex = DataSource.GetRowCount();
            for(int i = 0; i < DataSource.GetRowCount(); i++){
                _cellCount += DataSource.GetCellCountInRow(i);

                if(_cellIndex <= _cellCount){
                    _rowIndex = i;
                    break;
                }
            }

            return GetScrollYForRow(_rowIndex, _above);
        }

        public void ScrollTo<U>(Func<U, bool> _find, out int _rowIndex, int _scrollOffset = 0) where U : class, IRecordContainer{
            var _scrollTargetIndex = this.DataSource.GetRowIndex(_find);
            _rowIndex = _scrollTargetIndex;

            if(_rowIndex >= 0){
                ScrollToRow(_scrollTargetIndex, _scrollOffset);
            }
        }

        public void ScrollToRow(int _rowIndex, float _scrollOffset = 0, bool _above = true){
            var _scroll = Mathf.Max(0, this.GetScrollYForRow(_rowIndex, _above) - _scrollOffset);
            this.scrollDistance = _scroll;
        }

        #endregion

        #region Private implementation
        [FormerlySerializedAs("tableRow")]
		[SerializeField]
		private TableRow tableRowDefault;
        [SerializeField]
        private List<TableRow> tableRows = new List<TableRow>();

        public int CellCountInRowDefault{
            get{
                return tableRowDefault.Cells.Count;
            }
        }
        private ITableViewDataSource m_dataSource;
        private bool m_requiresReload;

		private HorizontalOrVerticalLayoutGroup m_LayoutGroup;
		private EventControlledScrollRect m_scrollRect;
        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;

		private float[] m_rowSizes;
        //cumulative[i] = sum(rowHeights[j] for 0 <= j <= i)
		private float[] m_cumulativeRowSizes;
        private int m_cleanCumulativeIndex;

        private Dictionary<int, TableRow> m_visibleRows;
		private UnityEngine.SocialPlatforms.Range m_visibleRowRange;
        public UnityEngine.SocialPlatforms.Range VisibleRowRange{
            get{ return m_visibleRowRange;}
        }

        private RectTransform m_reusableRowContainer;
        private Dictionary<string, LinkedList<TableRow>> m_reusableRows;

        private float m_scrollDistance;
        private bool m_requiresRefresh;
        private bool m_requiresCallScrollChagned = false;

		private bool m_isVertical;
		private float m_defaultRowHeight;

		public ITableViewDataSource DataSource
		{
			get { return m_dataSource; }
			set { m_dataSource = value;init(); }
		}

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
            while (m_visibleRows.Count > 0) {
                HideRow(false);
            }
            m_visibleRowRange = new UnityEngine.SocialPlatforms.Range(0, 0);
        }

        void Awake()
        {
            init();
        }

        private bool isInit = false;
        private void init()
        {
            if(isInit == true){
                return;
            }

            isInit = true;
            m_isVertical = true;
            isEmpty = true;
            m_scrollRect = GetComponent<EventControlledScrollRect>();
            m_LayoutGroup = m_scrollRect.content.GetComponentInChildren<VerticalLayoutGroup>();

            if (m_LayoutGroup == null)
            {
                m_LayoutGroup = m_scrollRect.content.GetComponentInChildren<HorizontalLayoutGroup>();
                m_isVertical = false;
            }

            if (m_LayoutGroup == null)
            {
                throw new System.Exception("m_verticalLayoutGroup is null");
            }


            initRows();

            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_topPadding.transform.SetParent(m_scrollRect.content, false);
            m_bottomPadding = CreateEmptyPaddingElement("Bottom");
            m_bottomPadding.transform.SetParent(m_scrollRect.content, false);
            m_visibleRows = new Dictionary<int, TableRow>();

            m_reusableRowContainer = new GameObject("ReusableRows", typeof(RectTransform)).GetComponent<RectTransform>();
            m_reusableRowContainer.SetParent(this.transform, false);
            m_reusableRowContainer.gameObject.SetActive(false);
            m_reusableRows = new Dictionary<string, LinkedList<TableRow>>();
        }

        private void initRows()
        {
            m_defaultRowHeight = 100;
            
            if (tableRowDefault != null)
            {
                if (tableRows.Count == 0)
                {
                    tableRows.Add(tableRowDefault);
                }
            }


            if (tableRows.Count > 0)
            {
                if (tableRowDefault == null)
                {
                    tableRowDefault = tableRows[0];
                }

                for (int i = 0; i < tableRows.Count; i++)
                {
                    var _row = tableRows[i];
                    _row.gameObject.SetActive(false);
                    _row.InitializeCells();
                }


                if (m_isVertical)
                {
                    m_defaultRowHeight = tableRowDefault.GetComponent<RectTransform>().sizeDelta.y;
                }
                else
                {
                    m_defaultRowHeight = tableRowDefault.GetComponent<RectTransform>().sizeDelta.x;
                }
            }
        }

        void Update()
        {
            if (m_requiresReload) {
                ReloadData();
            }
            
            if(m_requiresCallScrollChagned){
                m_scrollRect.onValueChanged.Invoke(m_scrollRect.normalizedPosition);
                m_requiresCallScrollChagned = false;
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
            scrollTracker.Cancel();
		}

		private void isControlledFalse(){
			isControlled = false;
		}


		#region MangetControl
		[SerializeField]
		private bool centerPositionMagnet = false;
        [SerializeField]
        private float magnetSpeed = 1000;
        [SerializeField]
        private EaseType magnetEaseType = EaseType.OutBack;

		private void checkEnableMagnet(){
			if (isControlled == false) {
                isControlled = true;
                MagnetControl();
			}
		}

        private TweenTracker scrollTracker = new TweenTracker();
        private RectTransform rectTransform = null;
        public void ScrollToRowCenter(int _index){
            if(rectTransform == null){
                rectTransform = this.GetComponent<RectTransform>();
            }

            var _centerPosition = this.m_isVertical == true ? rectTransform.rect.height : rectTransform.rect.width;
            var _distance = DataSource.GetHeightForRowInTableView(_index);
            var _offset = (_centerPosition - _distance) / 2f;
            var _scroll = this.GetScrollYForRow(_index, true) - _offset;
            this.scrollDistance = _scroll;
        }
		public void MagnetControl(){
            if(m_isVertical == true){
                throw new System.NotImplementedException("not support magnet control for vertical table");
            }

            if(m_visibleRows.Count <= 0){
                BicTween.DelayOneFrame().SubscribeComplete(()=>{
                    BicTween.DelayOneFrame().SubscribeComplete(()=>{
                    isControlled = false;
                    });
                });
                return;
            }

            float _targetPosition = 0f;
            float _speed = 1f;
			var _centerPosition = m_scrollRect.transform.position;
            bool _isFind = false;
            var _selectedRowIndex = -1;
			foreach (var _row in m_visibleRows) {
                var _rowPosition = _row.Value.transform.position;
				var _rowHalfSize = m_rowSizes [_row.Key] / 2f;
                
                if (_rowPosition.x + _rowHalfSize > _centerPosition.x && _rowPosition.x - _rowHalfSize <= _centerPosition.x) {
                    _selectedRowIndex = _row.Key;
                    _targetPosition = scrollDistance - (_centerPosition.x - _rowPosition.x);
                    _isFind = true;
                    break;
                }
			}

            if(_isFind == false){
                try{
                    if(this.scrollDistance <= 0){
                        _targetPosition = scrollDistance - (_centerPosition.x - m_visibleRows[0].transform.position.x);
                        _selectedRowIndex = 0;
                    }else{
                        _targetPosition = scrollDistance - (_centerPosition.x - m_visibleRows[m_rowSizes.Length - 1].transform.position.x);
                        _selectedRowIndex = m_rowSizes.Length - 1;
                    }
                }catch{
                    Debug.LogWarning("[TableView] MagnetControl error");
                    return;
                }
            }

            float _targetGap = _targetPosition - scrollDistance;
            _speed = Mathf.Abs(_targetGap) / magnetSpeed;
            var _tween = BicTween.Value (scrollDistance, _targetPosition, _speed).SubscribeUpdate(_value => {
                scrollDistance = _value.x;
            }).SetTracker(scrollTracker).SubscribeComplete(()=>{
                OnMargnetControl(_selectedRowIndex);
            }).SetEase(magnetEaseType);
		}

        public Action<int> OnMargnetControl = null;

        public void EnableScroll(bool _isEnable){
            this.m_scrollRect.enabled = _isEnable;
        }
		#endregion
        
        private UnityEngine.SocialPlatforms.Range CalculateCurrentVisibleRowRange()
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
            return new UnityEngine.SocialPlatforms.Range(startIndex, endIndex - startIndex + 1);
        }

        private void SetInitialVisibleRows()
        {
            UnityEngine.SocialPlatforms.Range visibleRows = CalculateCurrentVisibleRowRange();

            for (int i = 0; i < visibleRows.count; i++)
            {   
                var _rowIndex = visibleRows.from + i;
                AddRow(_rowIndex, true, m_scrollRect.content);
            }
            m_visibleRowRange = visibleRows;
            UpdatePaddingElements();
        }

        private void AddRow(int _rowIndex, bool _isEnd, RectTransform _parent)
        {
            TableRow newRow = m_dataSource.GetCellForRowInTableView(_rowIndex);
            int _startDataIndex = DataSource.GetStartDataIndex(_rowIndex);

            newRow.transform.SetParent(_parent, false);


			if(m_isVertical) {
				newRow.LayoutElement.preferredHeight = m_rowSizes[_rowIndex];
				if(_rowIndex > 0) {
					newRow.LayoutElement.preferredHeight -= m_LayoutGroup.spacing;
				}
			} else {
				newRow.LayoutElement.preferredWidth = m_rowSizes[_rowIndex];
				if(_rowIndex > 0) {
					newRow.LayoutElement.preferredWidth -= m_LayoutGroup.spacing;
				}
			}
            
            m_visibleRows[_rowIndex] = newRow;

            if (_isEnd) {
                newRow.transform.SetSiblingIndex(m_scrollRect.content.childCount - 2); //One before bottom padding
            } else {
                newRow.transform.SetSiblingIndex(1); //One after the top padding
            }

            newRow.SetData(_startDataIndex, DataSource.GetCellData, DataSource);

			this.onRowVisibilityChanged.Invoke(_rowIndex, true);
        }

        private void RefreshVisibleRows()
        {
            m_requiresRefresh = false;

            if (this.isEmpty) {
                return;
            }

            UnityEngine.SocialPlatforms.Range newVisibleRows = CalculateCurrentVisibleRowRange();

			int oldTo = m_visibleRowRange.Last();
            int newTo = newVisibleRows.Last();

            if (newVisibleRows.from > oldTo || newTo < m_visibleRowRange.from) {
                //We jumped to a completely different segment this frame, destroy all and recreate
				RecalculateVisibleRowsFromScratch();
                return;
            }

            //Remove rows that disappeared to the top
            for (int i = m_visibleRowRange.from; i < newVisibleRows.from; i++)
            {
                HideRow(false);
            }
            //Remove rows that disappeared to the bottom
            for (int i = newTo; i < oldTo; i++)
            {
                HideRow(true);
            }
            //Add rows that appeared on top
            for (int i = m_visibleRowRange.from - 1; i >= newVisibleRows.from; i--) {
                AddRow(i, false, m_scrollRect.content);
            }
            //Add rows that appeared on bottom
            for (int i = oldTo + 1; i <= newTo; i++) {
                AddRow(i, true, m_scrollRect.content);
            }

            m_visibleRowRange = newVisibleRows;
            UpdatePaddingElements();
        }

        private void UpdatePaddingElements() {
            float hiddenElementsHeightSum = 0;
            for (int i = 0; i < m_visibleRowRange.from; i++) {
                hiddenElementsHeightSum += m_rowSizes[i];
            }

			if(m_isVertical) {
				m_topPadding.preferredHeight = hiddenElementsHeightSum;
                m_topPadding.gameObject.SetActive(m_topPadding.preferredHeight > 0);
			} else {
				m_topPadding.preferredWidth = hiddenElementsHeightSum;
				m_topPadding.gameObject.SetActive(m_topPadding.preferredWidth > 0);
			}


			for (int i = m_visibleRowRange.from; i <= m_visibleRowRange.Last(); i++) {
                hiddenElementsHeightSum += m_rowSizes[i];
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

        private void HideRow(bool _last)
        {
            int _rowIndex = _last ? m_visibleRowRange.Last() : m_visibleRowRange.from;
            TableRow removedRow = m_visibleRows[_rowIndex];
			removedRow.OnRemove();
            StoreRowForReuse(removedRow);
            m_visibleRows.Remove(_rowIndex);
            m_visibleRowRange.count -= 1;
            if (!_last) {
                m_visibleRowRange.from += 1;
            } 
            this.onRowVisibilityChanged.Invoke(_rowIndex, false);
        }

        private LayoutElement CreateEmptyPaddingElement(string _name)
        {
            GameObject go = new GameObject(_name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }

        private int FindIndexOfRowAtY(float y) {
            //TODO : Binary search if inside clean cumulative row height area, else walk until found.
            return FindIndexOfRowAtY(y, 0, m_cumulativeRowSizes.Length - 1);
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

        private float GetCumulativeRowHeight(int _rowIndex) {
            while (m_cleanCumulativeIndex < _rowIndex) {
                m_cleanCumulativeIndex++;
				m_cumulativeRowSizes[m_cleanCumulativeIndex] = m_rowSizes[m_cleanCumulativeIndex];
				 if (m_cleanCumulativeIndex > 0) {
                    m_cumulativeRowSizes[m_cleanCumulativeIndex] += m_cumulativeRowSizes[m_cleanCumulativeIndex - 1];
                } 
            }

            return m_cumulativeRowSizes[_rowIndex];
        }

        private void StoreRowForReuse(TableRow _row) {
            string reuseIdentifier = _row.ReuseIdentifier;
            
            if (string.IsNullOrEmpty(reuseIdentifier)) {
                GameObject.Destroy(_row.gameObject);
                return;
            }

            if (!m_reusableRows.ContainsKey(reuseIdentifier)) {
                m_reusableRows.Add(reuseIdentifier, new LinkedList<TableRow>());
            }

            m_reusableRows[reuseIdentifier].AddLast(_row);
            _row.transform.SetParent(m_reusableRowContainer, false);
        }

        public void ChangeRowSize(int _rowIndex, float _size){
            m_rowSizes[_rowIndex] = _size;

            if(m_visibleRowRange.Contains(_rowIndex)){
                if(m_isVertical == true){
                    m_visibleRows[_rowIndex].LayoutElement.preferredHeight = _size;
                }else{
                    m_visibleRows[_rowIndex].LayoutElement.preferredWidth = _size;
                }

                UpdatePaddingElements();
                m_cleanCumulativeIndex = -1;
                RecalculateContentSize();
            }
        }

        #endregion

        #region Event
        private Action onReloadData = null;
        public void SubscribeReloadData(Action _callback){
            onReloadData += _callback;
        }

        public void UnsubscribeReloadData(Action _callback){
            onReloadData -= _callback;
        }

        private Action onDestoryCallback = null;
        private void OnDestroy() {
            if(onDestoryCallback != null){
                onDestoryCallback();
            }
        }

        public void SubscribeDestory(Action _callback){
            onDestoryCallback += _callback;
        }

        public void UnsubscribeDestory(Action _callback){
            onDestoryCallback -= _callback;
        }
        #endregion
    }

    internal static class RangeExtensions
    {
        public static int Last(this UnityEngine.SocialPlatforms.Range range)
        {
            if (range.count == 0)
            {
                throw new System.InvalidOperationException("Empty range has no to()");
            }
            return (range.from + range.count - 1);
        }

        public static bool Contains(this UnityEngine.SocialPlatforms.Range range, int num) {
            return num >= range.from && num < (range.from + range.count);
        }
    }

}

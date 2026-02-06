using System;
using System.Collections.Generic;
using Plugins.MVVM.Runtime.UIComponents.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIS {

    /// <summary>
    /// Load direction
    /// </summary>
    public enum ScrollerDirection {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>
    /// Infinite scroller
    /// </summary>
    public class Scroller : MonoBehaviour {

        /// <summary>
        /// Velocity for scroll to function
        /// </summary>
        public Vector2 SCROLL_VELOCITY = new Vector2(0f, 50f);

        /// <summary>
        /// Delegate for heights
        /// </summary>
        public delegate int HeightItem(int index);

        /// <summary>
        /// Event for get item height
        /// </summary>
        public HeightItem OnHeight;

        /// <summary>
        /// Delegate for widths
        /// </summary>
        public delegate int WidthtItem(int index);

        /// <summary>
        /// Event for get item width
        /// </summary>
        public HeightItem OnWidth;

        /// <summary>
        /// Callback on item fill
        /// </summary>
        public Action<int, GameObject> OnFill = delegate { };

        /// <summary>
        /// Callback to get prefab for specific item index
        /// Returns null to use default Prefab
        /// </summary>
        public Func<int, GameObject> OnGetPrefab = null;

        [Header("Item settings")]
        
        [HideInInspector]
        public GameObject Prefab = null;

        /// <summary>
        /// Enable multi-prefab support
        /// When enabled, OnGetPrefab callback will be used to determine prefab per item
        /// Set automatically by BindVirtualList
        /// </summary>
        [HideInInspector]
        public bool UseMultiplePrefabs = false;

        /// <summary>
        /// Auto-calculate item sizes from prefab RectTransform
        /// When enabled and OnHeight/OnWidth callbacks are null, sizes are extracted from prefabs
        /// Set automatically by BindVirtualList
        /// </summary>
        [HideInInspector]
        public bool AutoCalculateSizes = false;

        [Header("Padding")]
        /// <summary>
        /// Top padding
        /// </summary>
        public int TopPadding = 10;

        /// <summary>
        /// Bottom padding
        /// </summary>
        public int BottomPadding = 10;

        [Header("Padding")]
        /// <summary>
        /// Left padding
        /// </summary>
        public int LeftPadding = 10;

        /// <summary>
        /// Right padding
        /// </summary>
        public int RightPadding = 10;

        /// <summary>
        /// Spacing between items
        /// </summary>
        public int ItemSpacing = 10;


        [Header("Other")]
        /// <summary>
        /// Container for calc width/height if anchors exists
        /// </summary>
        public RectTransform ParentContainer = null;

        /// <summary>
        /// Addon count views
        /// </summary>
        public int AddonViewsCount = 4;


        /// <summary>
        /// Type of scroller: 0 = Vertical, 1 = Horizontal
        /// </summary>
        [HideInInspector]
        public int Type = 0;
        
        /// <summary>
        /// Is this a vertical scroller?
        /// </summary>
        public bool IsVertical => Type == 0;
        
        public ScrollerData Data = new ScrollerData();
        ScrollerInitializer _initializer;
        ScrollerViewPool _viewPool;
        ScrollerLayoutCalculator _layoutCalculator;
        ScrollerUpdateSystem _updateSystem;
        ScrollerNavigation _navigation;
        
        bool _isInitialized = false;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        void Initialize() {
            if (_isInitialized) return;
            _isInitialized = true;
            
            _initializer = new ScrollerInitializer(this);
            _viewPool = new ScrollerViewPool(this);
            _layoutCalculator = new ScrollerLayoutCalculator(this);
            _updateSystem = new ScrollerUpdateSystem(this);
            _navigation = new ScrollerNavigation(this);
            
            Data.Container = (ParentContainer != null) ? ParentContainer.rect : GetComponent<RectTransform>().rect;
            Data.Container.width = Mathf.Abs(Data.Container.width);
            Data.Container.height = Mathf.Abs(Data.Container.height);
            Data.Scroll = GetComponent<ScrollRect>();
            Data.Content = Data.Scroll.viewport.transform.GetChild(0).GetComponent<RectTransform>();
            Data.Heights = new Dictionary<int, int>();
            Data.Widths = new Dictionary<int, int>();
            Data.Positions = new Dictionary<int, float>();
        }

        /// <summary>
        /// Is list has been inited
        /// </summary>
        public bool IsInited {
            get {
                return Data.IsInited;
            }
        }

        /// <summary>
        /// Return list views count
        /// </summary>
        public int ViewsCount {
            get {
                return (Data.Views == null) ? 0 : Data.Views.Length;
            }
        }

        /// <summary>
        /// Get height from prefab's RectTransform
        /// Uses rect.rect.size for accurate measurement regardless of anchor setup
        /// Accounts for localScale to get the actual rendered size
        /// </summary>
        public static int GetPrefabHeight(GameObject prefab)
        {
            if (prefab == null) return 80;
            var rect = prefab.GetComponent<RectTransform>();
            if (rect == null) return 80;
            return Mathf.RoundToInt(Mathf.Abs(rect.rect.height * rect.localScale.y));
        }

        /// <summary>
        /// Get width from prefab's RectTransform
        /// Uses rect.rect.size for accurate measurement regardless of anchor setup
        /// Accounts for localScale to get the actual rendered size
        /// </summary>
        public static int GetPrefabWidth(GameObject prefab)
        {
            if (prefab == null) return 100;
            var rect = prefab.GetComponent<RectTransform>();
            if (rect == null) return 100;
            return Mathf.RoundToInt(Mathf.Abs(rect.rect.width * rect.localScale.x));
        }

        /// <summary>
        /// Current normalized position 0..1
        /// </summary>
        public float NormalizedPosition {
            get {
                return (Type == 0) ? Data.Scroll.verticalNormalizedPosition : Data.Scroll.horizontalNormalizedPosition;
            }
        }
        
        /// <summary>
        /// Main loop to check items positions and heights
        /// </summary>
        void Update() {
            _updateSystem.Update(Type);
        }

        /// <summary>
        /// Scroll to show item by index
        /// </summary>
        /// <param name="index">Item index</param>
        public void ScrollTo(int index) {
            _navigation.ScrollTo(index);
        }

        /// <summary>
        /// Disable all items in list
        /// </summary>
        public void RecycleAll() {
            Data.Count = 0;
            if (Data.Views == null || !Data.IsInited) {
                return;
            }
            for (var i = 0; i < Data.Views.Length; i++) {
                Data.Views[i].SetActive(false);
            }
        }

        /// <summary>
        /// Disable item
        /// </summary>
        /// <param name="index">Index in list data</param>
        public void Recycle(int index) {
            Data.Count--;
            var name = index.ToString();
            if (Data.Count == 0 || !Data.IsInited) {
                RecycleAll();
                return;
            }
            var height = CalcSizesPositions(Data.Count);
            for (var i = 0; i < Data.Views.Length; i++) {
                if (string.CompareOrdinal(Data.Views[i].name, name) == 0) {
                    Data.Views[i].SetActive(false);
                    Data.Content.sizeDelta = new Vector2(Data.Content.sizeDelta.x, height);
                    UpdateVisible();
                    UpdatePositions();
                    break;
                }
            }
        }

        /// <summary>
        /// Update positions for visible items
        /// </summary>
        void UpdatePositions() {
            var pos = Vector2.zero;
            pos.y = 0f;
            for (var i = 0; i < Data.Views.Length; i++) {
                if (i + 1 > Data.Count) {
                    continue;
                }
                var index = int.Parse(Data.Views[i].name);
                if (index < Data.Count) {
                    pos = Data.Rects[i].anchoredPosition;
                    pos.y = Data.Positions[i];
                    pos.x = 0f;
                    Data.Rects[i].anchoredPosition = pos;
                    var size = Data.Rects[i].sizeDelta;
                    if (Type == 0) {
                        size.y = Data.Heights[i];
                    } else {
                        size.x = Data.Widths[i];
                    }
                    Data.Rects[i].sizeDelta = size;
                }
            }
        }

        /// <summary>
        /// Update visible items with new data
        /// </summary>
        public void UpdateVisible() {
            if (!Data.IsInited) {
                return;
            }
            for (var i = 0; i < Data.Views.Length; i++) {
                var showed = i < Data.Count;
                Data.Views[i].SetActive(showed);
                if (i + 1 > Data.Count) {
                    continue;
                }
                var index = int.Parse(Data.Views[i].name);
                if (index < Data.Count) {
                    OnFill(index, Data.Views[i]);
                }
            }
        }

        /// <summary>
        /// Clear views cache
        /// Needed to recreate views after Prefab change
        /// </summary>
        /// <param name="count">Items count</param>
        public void RefreshViews(int count) {
            if (Data.Views == null) {
                return;
            }
            Data.IsInited = false;
            for (var i = Data.Views.Length - 1; i >= 0; i--) {
                Destroy(Data.Views[i]);
            }
            Data.Rects = null;
            Data.Views = null;
            Data.Indexes = null;
            CalcSizesPositions(count);
            CreateViews(true);
        }

        /// <summary>
        /// Get all views in list
        /// </summary>
        /// <returns>Array of views</returns>
        public GameObject[] GetAllViews() {
            return Data.Views;
        }
        
        /// <summary>
        /// Init list
        /// </summary>
        /// <param name="count">Items count</param>
        /// <param name="isOtherSide">Go to bottom or right on init</param>
        public void InitData(int count, bool isOtherSide = false) {
            if (count <= 0) {
                Debug.LogWarning("Can't init empty list!");
                return;
            }
            Initialize();
            Data.IsInited = true;
            _initializer.Init(count, isOtherSide);
        }
        
        /// <summary>
        /// Create views
        /// </summary>
        /// <param name="isForceCreate">Create views anyway</param>
        public void CreateViews(bool isForceCreate = false) {
            _viewPool.CreateViews(isForceCreate, Type);
        }
        
        /// <summary>
        /// Calc all items height and positions
        /// </summary>
        /// <returns>Common content height</returns>
        public float CalcSizesPositions(int count) {
            return _layoutCalculator.CalcSizesPositions(count, Type);
        }

        /// <summary>
        /// Update list after load new items
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        public void ApplyDataTo(int count, int newCount, ScrollerDirection direction) {
            if (!Data.IsInited) {
                return;
            }
            _initializer.ApplyDataTo(count, newCount, direction);
        }
    }
}
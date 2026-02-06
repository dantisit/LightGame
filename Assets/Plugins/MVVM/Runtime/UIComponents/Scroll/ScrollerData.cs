using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MVVM.Runtime.UIComponents.Scroll
{
    public class ScrollerData
    {
        /// <summary>
        /// Scrollrect cache
        /// </summary>
        public ScrollRect Scroll = null;

        /// <summary>
        /// Content rect cache
        /// </summary>
        public RectTransform Content = null;

        /// <summary>
        /// Container rect cache
        /// </summary>
        public Rect Container = new Rect();

        /// <summary>
        /// All rects cache
        /// </summary>
        public RectTransform[] Rects = null;

        /// <summary>
        /// All objects cache
        /// </summary>
        public GameObject[] Views = null;

        /// <summary>
        /// Previous position
        /// </summary>
        public int PreviousPosition = -1;

        /// <summary>
        /// List items count
        /// </summary>
        public int Count = 0;

        /// <summary>
        /// Items heights cache
        /// </summary>
        public Dictionary<int, int> Heights = null;

        /// <summary>
        /// Items widths cache
        /// </summary>
        public Dictionary<int, int> Widths = null;

        /// <summary>
        /// Items positions cache
        /// </summary>
        public Dictionary<int, float> Positions = null;

        /// <summary>
        /// Cache with item indexes
        /// </summary>
        public int[] Indexes = null;

        /// <summary>
        /// Track which prefab each view was created from (for multi-prefab support)
        /// </summary>
        public GameObject[] ViewPrefabs = null;

        /// <summary>
        /// Item height or width for non-different lists
        /// </summary>
        public float OffsetData = 0f;

        /// <summary>
        /// Check if items has different heights/widths
        /// </summary>
        public bool IsComplexList = false;

        /// <summary>
        /// Init list flag
        /// </summary>
        public bool IsInited = false;
    }
}
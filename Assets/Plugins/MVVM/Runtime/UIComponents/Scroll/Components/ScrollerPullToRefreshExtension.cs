using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIS {
    
    /// <summary>
    /// Pull-to-refresh extension for Scroller component
    /// Attach this component to enable pull-to-refresh functionality
    /// </summary>
    [RequireComponent(typeof(Scroller))]
    public class ScrollerPullToRefreshExtension : MonoBehaviour, IDropHandler {
        
        /// <summary>
        /// Callback on pull action
        /// </summary>
        public Action<ScrollerDirection> OnPull = delegate { };
        
        [Header("Labels")]
        /// <summary>
        /// Label font asset
        /// </summary>
        public TMP_FontAsset LabelsFont = null;

        /// <summary>
        /// Label font size
        /// </summary>
        public int FontSize = 30;

        /// <summary>
        /// Label color
        /// </summary>
        public Color FontColor = Color.white;

        /// <summary>
        /// Pull top text label
        /// </summary>
        public string TopPullLabel = "Pull to refresh";

        /// <summary>
        /// Release top text label
        /// </summary>
        public string TopReleaseLabel = "Release to load";

        /// <summary>
        /// Pull bottom text label
        /// </summary>
        public string BottomPullLabel = "Pull to refresh";

        /// <summary>
        /// Release bottom text label
        /// </summary>
        public string BottomReleaseLabel = "Release to load";

        /// <summary>
        /// Pull left text label
        /// </summary>
        public string LeftPullLabel = "Pull to refresh";

        /// <summary>
        /// Release left text label
        /// </summary>
        public string LeftReleaseLabel = "Release to load";

        /// <summary>
        /// Pull right text label
        /// </summary>
        public string RightPullLabel = "Pull to refresh";

        /// <summary>
        /// Release right text label
        /// </summary>
        public string RightReleaseLabel = "Release to load";

        [Header("Directions")]
        /// <summary>
        /// Can we pull from top
        /// </summary>
        public bool IsPullTop = true;

        /// <summary>
        /// Can we pull from bottom
        /// </summary>
        public bool IsPullBottom = true;

        /// <summary>
        /// Can we pull from left
        /// </summary>
        public bool IsPullLeft = true;

        /// <summary>
        /// Can we pull from right
        /// </summary>
        public bool IsPullRight = true;

        [Header("Offsets")]
        /// <summary>
        /// Coefficient when labels should action
        /// </summary>
        public float PullValue = 1.5f;

        /// <summary>
        /// Label position offset
        /// </summary>
        public float LabelOffset = 85f;

        [HideInInspector]
        public TextMeshProUGUI TopLabel = null;

        [HideInInspector]
        public TextMeshProUGUI BottomLabel = null;

        [HideInInspector]
        public TextMeshProUGUI LeftLabel = null;

        [HideInInspector]
        public TextMeshProUGUI RightLabel = null;

        private Scroller _scroller;
        private bool _isCanLoadUp = false;
        private bool _isCanLoadDown = false;
        private bool _isCanLoadLeft = false;
        private bool _isCanLoadRight = false;
        private float _previousScrollPosition = -1f;

        void Awake() {
            _scroller = GetComponent<Scroller>();
            _scroller.Data.Scroll.onValueChanged.AddListener(OnScrollChange);
            CreateLabels();
        }

        void OnDestroy() {
            if (_scroller != null && _scroller.Data.Scroll != null) {
                _scroller.Data.Scroll.onValueChanged.RemoveListener(OnScrollChange);
            }
        }

        /// <summary>
        /// Create labels
        /// </summary>
        private void CreateLabels() {
            var isVertical = _scroller.Type == 0;
            
            if (isVertical) {
                CreateLabel("TopLabel", ref TopLabel, TopPullLabel,
                    new Vector2(0.5f, 1f), new Vector2(0f, 1f), Vector2.one,
                    Vector2.zero, new Vector2(0f, -LabelOffset));
                CreateLabel("BottomLabel", ref BottomLabel, BottomPullLabel,
                    new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1f, 0f),
                    new Vector2(0f, LabelOffset), Vector2.zero);
            } else {
                CreateLabel("LeftLabel", ref LeftLabel, LeftPullLabel,
                    new Vector2(0f, 0.5f), Vector2.zero, new Vector2(0f, 1f),
                    Vector2.zero, new Vector2(-LabelOffset * 2, 0f));
                CreateLabel("RightLabel", ref RightLabel, RightPullLabel,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0f), Vector3.one,
                    new Vector2(LabelOffset * 2, 0f), Vector2.zero);
            }
        }

        private void CreateLabel(string name, ref TextMeshProUGUI label, string text,
            Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMax, Vector2 offsetMin) {
            var labelObj = new GameObject(name);
            labelObj.transform.SetParent(_scroller.Data.Scroll.viewport.transform);
            label = labelObj.AddComponent<TextMeshProUGUI>();
            label.font = LabelsFont;
            label.color = FontColor;
            label.fontSize = FontSize;
            label.transform.localScale = Vector3.one;
            label.alignment = TextAlignmentOptions.Center;
            label.text = text;
            label.transform.position = Vector3.zero;
            var rect = label.GetComponent<RectTransform>();
            rect.pivot = pivot;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMax = offsetMax;
            rect.offsetMin = offsetMin;
            rect.anchoredPosition3D = Vector3.zero;
            labelObj.SetActive(false);
        }

        /// <summary>
        /// Handler on scroller
        /// </summary>
        private void OnScrollChange(Vector2 vector) {
            if (_scroller.Data.Scroll.velocity.magnitude == 0.0f) {
                var pos = (_scroller.Type == 0) ? vector.y : vector.x;
                var index = Mathf.RoundToInt(_scroller.Data.Count * (1.0f - pos));
                _scroller.ScrollTo(index);
            }
            
            HandleScrollChange(_scroller.Type == 0);
        }

        private void HandleScrollChange(bool isVertical) {
            if (_scroller.Data.Views == null) return;

            if (isVertical) {
                _isCanLoadUp = false;
                _isCanLoadDown = false;
            } else {
                _isCanLoadLeft = false;
                _isCanLoadRight = false;
            }

            var normalizedPos = isVertical ? _scroller.Data.Scroll.verticalNormalizedPosition : _scroller.Data.Scroll.horizontalNormalizedPosition;
            var isScrollable = normalizedPos != 1f && normalizedPos != 0f;
            var position = isVertical ? _scroller.Data.Content.anchoredPosition.y : _scroller.Data.Content.anchoredPosition.x;
            
            var z = 0f;
            if (isScrollable) {
                var checkCondition = isVertical ? normalizedPos < 0f : normalizedPos > 1f;
                if (checkCondition) {
                    z = position - _previousScrollPosition;
                } else {
                    _previousScrollPosition = position;
                }
            } else {
                z = position;
            }

            if (isVertical) {
                HandleVerticalPull(position, z);
            } else {
                HandleHorizontalPull(position, z);
            }
        }

        private void HandleVerticalPull(float position, float z) {
            if (position < -LabelOffset && IsPullTop) {
                TopLabel.gameObject.SetActive(true);
                TopLabel.text = TopPullLabel;
                if (position < -LabelOffset * PullValue) {
                    TopLabel.text = TopReleaseLabel;
                    _isCanLoadUp = true;
                }
            } else {
                TopLabel.gameObject.SetActive(false);
            }
            
            if (z > LabelOffset && IsPullBottom) {
                BottomLabel.gameObject.SetActive(true);
                BottomLabel.text = BottomPullLabel;
                if (z > LabelOffset * PullValue) {
                    BottomLabel.text = BottomReleaseLabel;
                    _isCanLoadDown = true;
                }
            } else {
                BottomLabel.gameObject.SetActive(false);
            }
        }

        private void HandleHorizontalPull(float position, float z) {
            if (position > LabelOffset && IsPullLeft) {
                LeftLabel.gameObject.SetActive(true);
                LeftLabel.text = LeftPullLabel;
                if (position > LabelOffset * PullValue) {
                    LeftLabel.text = LeftReleaseLabel;
                    _isCanLoadLeft = true;
                }
            } else {
                LeftLabel.gameObject.SetActive(false);
            }
            
            if (z < -LabelOffset && IsPullRight) {
                RightLabel.gameObject.SetActive(true);
                RightLabel.text = RightPullLabel;
                if (z < -LabelOffset * PullValue) {
                    RightLabel.text = RightReleaseLabel;
                    _isCanLoadRight = true;
                }
            } else {
                RightLabel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handler on scroller drop pull
        /// </summary>
        public void OnDrop(PointerEventData eventData) {
            var isVertical = _scroller.Type == 0;
            
            if (isVertical) {
                if (_isCanLoadUp) {
                    OnPull(ScrollerDirection.Top);
                } else if (_isCanLoadDown) {
                    OnPull(ScrollerDirection.Bottom);
                }
                _isCanLoadUp = false;
                _isCanLoadDown = false;
            } else {
                if (_isCanLoadLeft) {
                    OnPull(ScrollerDirection.Left);
                } else if (_isCanLoadRight) {
                    OnPull(ScrollerDirection.Right);
                }
                _isCanLoadLeft = false;
                _isCanLoadRight = false;
            }
        }
    }
}

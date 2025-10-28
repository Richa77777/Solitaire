using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardSlot _lastParentSlot;
    private int _lastSiblingIndex;

    private Camera _mainCamera;
    private Collider2D _cardCollider;
    private SortingGroup _sortingGroup;

    private List<Transform> _draggedStack = new List<Transform>();
    private List<Vector3> _offsets = new List<Vector3>();

    private Vector3 _dragOffset;
    private CardSlot _pendingDropTarget;

    private const int DragSortingOffset = 1000;


    #region Unity Lifecycle

    private void Awake()
    {
        _cardCollider = GetComponent<Collider2D>();
        _sortingGroup = GetComponent<SortingGroup>();

        _mainCamera = Camera.main;
    }

    #endregion

    #region Initialization

    public void Initialize(CardSlot parentSlot)
    {
        _lastParentSlot = parentSlot;
        _lastSiblingIndex = transform.GetSiblingIndex();
    }

    public void SetSortingGroup(int sortingOrder)
    {
        _sortingGroup.sortingOrder = sortingOrder;
    }

    public void HandleDrop(CardSlot targetSlot)
    {
        _pendingDropTarget = targetSlot;
    }

    #endregion

    #region Dragging

    public void OnBeginDrag(PointerEventData eventData)
    {
        _draggedStack.Clear();
        _offsets.Clear();

        Transform parentTransform = transform.parent;

        if (parentTransform != null)
        {
            int startIndex = transform.GetSiblingIndex();
            _lastSiblingIndex = startIndex;

            CardSlot parentSlot = parentTransform.GetComponent<CardSlot>();

            if (parentSlot != null)
                _lastParentSlot = parentSlot;

            for (int i = startIndex; i < parentTransform.childCount; i++)
                _draggedStack.Add(parentTransform.GetChild(i));
        }
        else
        {
            _draggedStack.Add(transform);
            _lastSiblingIndex = 0;
        }

        for (int i = 0; i < _draggedStack.Count; i++)
            _offsets.Add(_draggedStack[i].position - _draggedStack[0].position);

        for (int i = 0; i < _draggedStack.Count; i++)
        {
            Transform cardTransform = _draggedStack[i];

            Collider2D collider = cardTransform.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            SortingGroup sortingGroup = cardTransform.GetComponent<SortingGroup>();
            if (sortingGroup != null)
                sortingGroup.sortingOrder = DragSortingOffset + i;

            cardTransform.SetParent(null);
        }

        _dragOffset = transform.position - _mainCamera.ScreenToWorldPoint((Vector3)eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedStack.Count == 0)
            return;

        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint((Vector3)eventData.position) + _dragOffset;

        for (int i = 0; i < _draggedStack.Count; i++)
            _draggedStack[i].position = mouseWorldPosition + _offsets[i];
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CardSlot targetSlot = _pendingDropTarget;

        if (targetSlot == null)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (RaycastResult raycastResult in raycastResults)
            {
                CardSlot foundSlot = raycastResult.gameObject.GetComponent<CardSlot>();
                if (foundSlot != null)
                {
                    targetSlot = foundSlot;
                    break;
                }
            }
        }

        if (targetSlot == null)
        {
            ReturnToLastParent();
            CleanupAfterDrag();

            _pendingDropTarget = null;
            _lastParentSlot?.RebuildUI();

            return;
        }

        if (targetSlot.SlotType == CardSlotType.Receiver && _draggedStack.Count > 1)
        {
            ReturnToLastParent();
            CleanupAfterDrag();

            _pendingDropTarget = null;
            _lastParentSlot?.RebuildUI();

            return;
        }

        int insertIndex = targetSlot.transform.childCount;

        for (int i = 0; i < _draggedStack.Count; i++)
        {
            Transform cardTransform = _draggedStack[i];
            cardTransform.SetParent(targetSlot.transform);

            int clampedIndex = Mathf.Clamp(insertIndex + i, 0, targetSlot.transform.childCount - 1);
            cardTransform.SetSiblingIndex(clampedIndex);
        }

        _lastParentSlot = targetSlot;

        CleanupAfterDrag();
        _pendingDropTarget = null;

        _lastParentSlot?.RebuildUI();
        targetSlot?.RebuildUI();
    }

    private void CleanupAfterDrag()
    {
        for (int i = 0; i < _draggedStack.Count; i++)
        {
            Transform cardTransform = _draggedStack[i];
            Collider2D collider = cardTransform.GetComponent<Collider2D>();

            if (collider != null)
                collider.enabled = true;
        }

        _draggedStack.Clear();
        _offsets.Clear();
    }

    #endregion

    #region Helpers

    public void ReturnToLastParent()
    {
        if (_lastParentSlot == null)
        {
            for (int i = 0; i < _draggedStack.Count; i++)
                _draggedStack[i].SetParent(null);

            return;
        }

        int startIndex = Mathf.Clamp(_lastSiblingIndex, 0, _lastParentSlot.transform.childCount);

        for (int i = 0; i < _draggedStack.Count; i++)
        {
            Transform cardTransform = _draggedStack[i];
            cardTransform.SetParent(_lastParentSlot.transform);

            int clampedIndex = Mathf.Clamp(startIndex + i, 0, _lastParentSlot.transform.childCount - 1);
            cardTransform.SetSiblingIndex(clampedIndex);
        }
    }

    public int GetDraggedCount()
    {
        return _draggedStack.Count;
    }

    #endregion
}
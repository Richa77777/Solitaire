using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private CardSlotType _cardSlotType;
    private GameSettings _gameSettings;

    public CardSlotType SlotType => _cardSlotType;

    #region Unity Lifecycle

    private void Awake()
    {
        _gameSettings = Resources.Load<GameSettings>("Configs/GameSettings");
    }

    private void Start()
    {
        RebuildUI();
    }

    private void OnTransformChildrenChanged()
    {
        RebuildUI();
    }

    #endregion

    #region UI

    public void RebuildUI()
    {
        if (_gameSettings == null)
            return;

        List<Card> childCards = new List<Card>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Card cardComponent = transform.GetChild(i).GetComponent<Card>();
            if (cardComponent != null)
                childCards.Add(cardComponent);
        }

        if (_cardSlotType == CardSlotType.Tableau)
        {
            for (int i = 0; i < childCards.Count; i++)
            {
                childCards[i].transform.localPosition = new Vector3(0, -(_gameSettings.TableauOffset * i), 0);
                childCards[i].SetSortingGroup(i);
            }
        }
        else
        {
            for (int i = 0; i < childCards.Count; i++)
            {
                childCards[i].transform.localPosition = Vector3.zero;
                childCards[i].SetSortingGroup(i);
            }
        }
    }

    #endregion

    #region Drop Handling

    public void OnDrop(PointerEventData eventData)
    {
        Card droppedCard = eventData.pointerDrag?.GetComponent<Card>();
        if (droppedCard == null)
            return;

        if (_cardSlotType == CardSlotType.Deck)
        {
            droppedCard.ReturnToLastParent();
            return;
        }

        int draggedCount = droppedCard.GetDraggedCount();

        if (_cardSlotType == CardSlotType.Receiver && draggedCount > 1)
        {
            droppedCard.ReturnToLastParent();
        }
        else
        {
            droppedCard.HandleDrop(this);
        }

        RebuildUI();
    }

    #endregion
}

public enum CardSlotType
{
    Deck,
    Tableau,
    Receiver
}
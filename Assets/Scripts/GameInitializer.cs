using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private int _startDeckSize = 52;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private CardSlot _deck;

    private void Awake()
    {
        for (int i = 0; i < _startDeckSize; i++)
        {
            Card card = Instantiate(_cardPrefab, _deck.transform);
            card.name = $"Card{i + 1}";
            card.transform.SetSiblingIndex(_deck.transform.childCount - 1);
            card.Initialize(_deck);
        }
    }
}

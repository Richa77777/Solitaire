using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Cards Settings")]
    [field: SerializeField] public float TableauOffset { get; private set; } = 0.3f;
}
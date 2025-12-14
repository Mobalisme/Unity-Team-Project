using UnityEngine;

public enum ItemType
{
    Apple,
    Grape,
    Bomb
}

public class FallingItem : MonoBehaviour
{
    public ItemType type = ItemType.Apple;
    public int points = 5;
}

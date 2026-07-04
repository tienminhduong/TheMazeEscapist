using UnityEngine;
using UnityEngine.Events;

public abstract class SpecialTile : MonoBehaviour
{
    public static UnityAction<SpecialTile> OnSpecialTileInstantiated;
    public static UnityAction<SpecialTile> OnSpecialTileInteracted;
    public abstract TileType Type { get; }
    public bool isEffect = false;
    protected virtual void OnInstantiated()
    {
        OnSpecialTileInstantiated?.Invoke(this);
    }

}

public struct SpTileData
{
    public Vector3 Position;
    public TileType Type;
}

public enum TileType
{
    Wall,
    Walkable,
    Portal,
    Trash,
    RecycleBin,
    StudentCard,
    WinPoint,
    Rock,
    Ladder,
    OneWayDoor,
    Slime,
    Item,
}
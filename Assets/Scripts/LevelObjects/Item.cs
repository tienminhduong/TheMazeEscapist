using UnityEngine.Events;

public class Item : SpecialTile
{
    public override TileType Type => TileType.Item;
    public static UnityAction<Item> OnItemInstantiated;
    public static UnityAction<Item> OnItemInteracted;
    void Awake()
    {
        isEffect = true;
    }

    void Start()
    {
        OnItemInstantiated?.Invoke(this);
    }

}

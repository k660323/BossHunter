using Data;
using System;

[Serializable]
public class Item
{
    public ItemData Data;

    public Item() { }

    public Item(ItemData data) => Data = data;

    public int Amount = 1;
}

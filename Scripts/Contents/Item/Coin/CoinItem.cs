using Data;
using System;

[Serializable]
public class CoinItem : Item
{
    public long coinValue;

    public CoinItem(ItemData data) : base(data)
    {

    }

    public CoinItem(ItemData data, long value) : base(data)
    {
        coinValue = value;
    }
}

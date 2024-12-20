using Data;
using System;
using UnityEngine;

[Serializable]
public class PortionItem : CountableItem, IUsableItem
{
    public static Action<Stat, PortionItemData>[] actions = new Action<Stat, PortionItemData>[7]
    {
        (stat, data)=> { return; },
        (stat, data)=> { stat.Hp += (int)data._value; },
        (stat, data)=> { stat.MaxHp += (int)data._value; },
        (stat, data)=> { stat.Mp += (int)data._value; },
        (stat, data)=> { stat.MaxMp += (int)data._value; },
        (stat, data)=> { stat.MoveSpeed += (int)data._value; },
        (stat, data)=> { return; }
    };
    
    public PortionItem(PortionItemData data, int amount = 1) : base(data, amount) { }

    public bool Use(GameObject target = null)
    {
        Amount--;

        if (target.TryGetComponent(out Creature creature))
        {
            Stat stat = creature.GetStat;
            PortionItemData data = Data as PortionItemData;

            actions[(int)data._effectType]?.Invoke(stat, data);

            return true;
        }

        return false;
    }

    protected override CountableItem Clone(int amount)
    {
        return new PortionItem(countableItemData as PortionItemData, amount);
    }
}

using Data;
using System;
using UnityEngine;

[Serializable]
public class CountableItem : Item
{
    public CountableItemData countableItemData;

    // 최대 슬롯 개수
    public int MaxAmount => countableItemData._maxAmount;

    // 수량이 가득 찼는지 여부
    public bool IsMax => Amount >= countableItemData._maxAmount;

    // 개수가 없는지 확인
    public bool IsEmpty => Amount <= 0;

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        countableItemData = data;
        SetAmount(amount);
    }

    // 개수 지정
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    // 개수 추가 및 최대치 초과량 반환 (초과량이 없을 경우 0)
    public int AddAmountAndGetExcess(int amount)
    {
        int totalAmount = Amount + amount;
        SetAmount(totalAmount);

        return (totalAmount > MaxAmount) ? totalAmount - MaxAmount : 0;
    }

    // 개수를 나누어 복제
    public CountableItem SeperateAndClone(int amount)
    {
        // 수량이 한개 이하인 경우 복제 불가
        if (Amount <= 1)
            return null;

        // 가지고있는 수량보다 많이 나눌경우 1 : N - 1으로 나눈다.
        if (amount > Amount - 1)
            amount = Amount - 1;

        Amount -= amount;
        return Clone(amount);
    }

    protected virtual CountableItem Clone(int amount)
    {
        return new CountableItem(countableItemData, amount);
    }
  
}

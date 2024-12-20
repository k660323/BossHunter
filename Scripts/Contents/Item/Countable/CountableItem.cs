using Data;
using System;
using UnityEngine;

[Serializable]
public class CountableItem : Item
{
    public CountableItemData countableItemData;

    // �ִ� ���� ����
    public int MaxAmount => countableItemData._maxAmount;

    // ������ ���� á���� ����
    public bool IsMax => Amount >= countableItemData._maxAmount;

    // ������ ������ Ȯ��
    public bool IsEmpty => Amount <= 0;

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        countableItemData = data;
        SetAmount(amount);
    }

    // ���� ����
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    // ���� �߰� �� �ִ�ġ �ʰ��� ��ȯ (�ʰ����� ���� ��� 0)
    public int AddAmountAndGetExcess(int amount)
    {
        int totalAmount = Amount + amount;
        SetAmount(totalAmount);

        return (totalAmount > MaxAmount) ? totalAmount - MaxAmount : 0;
    }

    // ������ ������ ����
    public CountableItem SeperateAndClone(int amount)
    {
        // ������ �Ѱ� ������ ��� ���� �Ұ�
        if (Amount <= 1)
            return null;

        // �������ִ� �������� ���� ������� 1 : N - 1���� ������.
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

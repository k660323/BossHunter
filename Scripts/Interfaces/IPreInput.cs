using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPreInput
{
    // �� �Է� ���� �� �ִ� �÷���
    public bool CanAddInputBuffer { get; set; }
    // �� �Է� ���� ���� �÷��� ��
    public bool PreInput { get; set; }


}

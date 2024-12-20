using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPreInput
{
    // 선 입력 받을 수 있는 플래그
    public bool CanAddInputBuffer { get; set; }
    // 선 입력 값을 받은 플래그 값
    public bool PreInput { get; set; }


}

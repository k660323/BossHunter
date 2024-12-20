using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager
{
    // 유니티 기본 레이어
    public readonly int Default = 1 << 0;
    public readonly int TransparentFX = 1 << 1;
    public readonly int IgnoreRaycast = 1 << 2;
    public readonly int ObjectToUI = 1 << 3;
    public readonly int Water = 1 << 4;
    public readonly int UI = 1 << 5;

    // 컨텐츠
    public readonly int Monster = 1 << 6;
    public readonly int Ground = 1 << 7;
    public readonly int Block = 1 << 8;
    public readonly int Player = 1 << 9;
    public readonly int Wall = 1 << 10;
    public readonly int WorldItem = 1 << 11;
    public readonly int PlayerDead = 1 << 12;
}

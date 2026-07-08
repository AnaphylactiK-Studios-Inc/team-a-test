using System;
using UnityEngine;

[Serializable]
public class TileDefinition
{
    public TileType type = TileType.Dead;
    [Range(0, 3)] public int rotation;
    public SystemType endpointSystem;
}

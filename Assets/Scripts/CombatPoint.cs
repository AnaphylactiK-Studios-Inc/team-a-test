using System;
using UnityEngine;

public class CombatPoint : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        public Transform anchor;
        public GameObject enemyPrefab;
    }

    public SpawnEntry[] spawns;
}

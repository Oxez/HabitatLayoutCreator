// Author Oxe
// Created at 04.10.2025 10:54

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Habitat/Rule Set")]
public class RuleSet : ScriptableObject
{
    [System.Serializable]
    public struct AdjacencyRule
    {
        public ZoneType A;
        public ZoneType B;
        [Tooltip("true - разнести; false — держать рядом")]
        public bool ShouldSeparate;
        [Tooltip("Для Separate - мин. дистанция, для соседства - макс. дистанция")]
        public float DistanceMeters;
    }

    public List<ZoneSpec>      ZoneSpecs      = new();
    public List<AdjacencyRule> AdjacencyRules = new();

    public ZoneSpec GetSpec(ZoneType t) => ZoneSpecs.Find(z => z.Type == t);
}

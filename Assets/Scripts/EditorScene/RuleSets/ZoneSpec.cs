// Author Oxe
// Created at 04.10.2025 10:53

using UnityEngine;

[CreateAssetMenu(menuName = "Habitat/Zone Spec")]
public class ZoneSpec : ScriptableObject
{
    public ZoneType Type;

    [Header("Минимум площади")]
    [Tooltip("м2 на 1 члена экипажа")]
    public float MinAreaPerCrew = 1.5f;

    [Tooltip("доп. м2 на каждые 100 суток миссии")]
    public float AreaPer100Days = 0.5f;

    [Header("Ширина проходов")]
    [Tooltip("Минимальная ширина прохода в метрах вокруг зоны")]
    public float MinAisleWidth = 0.9f;

    [Header("Нежелательные факторы (0..1)")]
    [Range(0,1)] public float Noise  = 0.5f;
    [Range(0,1)] public float Odor   = 0.5f;
}

// Author Oxe
// Created at 04.10.2025 10:53

using UnityEngine;

[CreateAssetMenu(menuName = "Habitat/Zone Spec")]
public class ZoneSpec : ScriptableObject
{
    public ZoneType Type;

    [Header("Minimum area")]
    [Tooltip("m2 per 1 crew member")]
    public float MinAreaPerCrew = 1.5f;

    [Tooltip("additional m2 for every 100 days of the mission")]
    public float AreaPer100Days = 0.5f;

    [Header("Width of passages")]
    [Tooltip("Minimum width of passage in meters around the area")]
    public float MinAisleWidth = 0.9f;

    [Header("Undesirable factors (0..1)")]
    [Range(0,1)] public float Noise  = 0.5f;
    [Range(0,1)] public float Odor   = 0.5f;
}

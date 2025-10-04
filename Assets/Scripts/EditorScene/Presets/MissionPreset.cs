// Author Oxe
// Created at 04.10.2025 10:54

using UnityEngine;

public enum Destination { Lunar, Transit, Martian }

[CreateAssetMenu(menuName = "Habitat/Mission Preset")]
public class MissionPreset : ScriptableObject
{
    public Destination Destination = Destination.Lunar;
    public int         CrewSize    = 4;
    public int         MissionDays = 180;

    [Header("Доставка/фэринг (упрощённо)")]
    public float MaxFairingDiameterM = 8f;
    public float MaxFairingHeightM = 20f;
    public float MaxMassTonnes     = 30f;
}

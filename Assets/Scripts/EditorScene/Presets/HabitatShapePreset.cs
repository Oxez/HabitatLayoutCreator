// Author Oxe
// Created at 04.10.2025 13:26

using UnityEngine;

public enum HabitatShape { Cylinder, Prism, Inflatable }
public enum StowOrientation { Vertical, Horizontal } // как укладываем при запуске

[CreateAssetMenu(menuName = "Habitat/Shape Preset")]
public class HabitatShapePreset : ScriptableObject
{
    public HabitatShape Shape = HabitatShape.Cylinder;

    [Header("Габариты в метрах")]
    public float Diameter = 6f; // для цилиндра/инфлейтабла (внутренний условный)
    public float Length = 10f;  // длина/высота модуля
    public int   Decks  = 1;

    [Header("Укладка при запуске")]
    public StowOrientation Stow = StowOrientation.Vertical;

    [Header("Редактор грида")]
    public float CellSize = 0.5f; // для EditorGrid
}

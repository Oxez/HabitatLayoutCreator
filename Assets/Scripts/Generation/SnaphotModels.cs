// Author Oxe
// Created at 04.10.2025 19:01


using System;
using System.Collections.Generic;

[Serializable] public class MissionDTO
{
    public int    crewSize;
    public int    missionDays;
    public float  fairingD;
    public float  fairingH;
    public float  maxMass;
    public string destination; // "Lunar"/"Transit"/"Martian" (информативно)
}

[Serializable] public class ShapeDTO
{
    public string shape; // "Cylinder"/"Prism"/"Inflatable"
    public float  diameter;
    public float  length;
    public int    decks;
    public float  cellSize;
    public string stow; // "Vertical"/"Horizontal"
}

[Serializable] public class DeckDTO
{
    public int   cols, rows;
    public int[] tiles; // ZoneType как int, длина = cols*rows
}

[Serializable] public class LayoutSnapshot
{
    public string        name;
    public MissionDTO    mission;
    public ShapeDTO      shape;
    public List<DeckDTO> decks = new();
    public string        createdAtISO;
}

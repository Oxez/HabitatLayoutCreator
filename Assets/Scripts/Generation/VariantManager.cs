// Author Oxe
// Created at 04.10.2025 19:03

using System;
using UnityEngine;

public class VariantManager : MonoBehaviour
{
    public DeckManager        decks;
    public HabitatContext     context;
    public HabitatShapePreset shapePreset;

    public LayoutSnapshot Capture(string name)
    {

        return null;
    }

    public void Apply(LayoutSnapshot snapshot)
    {

    }

    public void SaveNamed(string nameSafe)
    {
        var snap = Capture(nameSafe);
        SnapshotIO.Save(snap, nameSafe);
    }

    public void QuickSave()
    {
        var name = "variant_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        SaveNamed(name);
    }

    public void LoadFromPath(string filePath)
    {
        var snap = SnapshotIO.Load(filePath);
        Apply(snap);
    }
}

// Author Oxe
// Created at 04.10.2025 13:27

using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public HabitatShapePreset preset;
    public GameObject         editorGridPrefab; // пустой GO с компонентом EditorGrid (без плиток)
    public GameObject         tilePrefab;
    public float              deckHeight = 2.5f;

    public System.Action<EditorGrid> OnActiveDeckChanged;

    public int ActiveDeck { get; private set; } = 0;
    public List<EditorGrid> Decks = new();

    public void BuildDecks()
    {
        // очистка
        foreach (var d in Decks) if (d) DestroyImmediate(d.gameObject);
        Decks.Clear();

        int n = Mathf.Max(1, preset.Decks);
        for (int i = 0; i < n; i++)
        {
            var go = Instantiate(editorGridPrefab, transform);
            go.name = $"Deck_{i}";
            go.transform.localPosition = new Vector3(0, i * deckHeight, 0);

            var grid = go.GetComponent<EditorGrid>();
            grid.cellSize  = preset.CellSize;
            grid.lengthM   = preset.Length;
            grid.diameterM = preset.Diameter;
            grid.mask = (preset.Shape == HabitatShape.Prism) ? HabitatMask.Rectangle : HabitatMask.Cylinder;
            grid.tilePrefab = tilePrefab;
            grid.Rebuild();

            Decks.Add(grid);
            go.SetActive(i == ActiveDeck);
        }
    }

    public EditorGrid Current => (ActiveDeck >=0 && ActiveDeck < Decks.Count) ? Decks[ActiveDeck] : null;

    public void SetActiveDeck(int index)
    {
        if (Decks.Count == 0) return;
        index = Mathf.Clamp(index, 0, Decks.Count - 1);
        for (int i=0;i<Decks.Count;i++) Decks[i].gameObject.SetActive(i == index);
        ActiveDeck = index;

        var current = Decks[index];
        OnActiveDeckChanged?.Invoke(current);
    }

    public Dictionary<ZoneType,int> CountTilesAll()
    {
        var dict = new Dictionary<ZoneType,int>();
        foreach (var g in Decks)
            for (int x=0;x<g.cols;x++)
            for (int y=0;y<g.rows;y++)
            {
                var t = g.Tiles[x,y];
                if (t==null || t.Type==ZoneType.None) continue;
                if (!dict.ContainsKey(t.Type)) dict[t.Type]=0;
                dict[t.Type]++;
            }
        return dict;
    }

    public Dictionary<ZoneType, float> AreasAllDecksM2()
    {
        var areas = new Dictionary<ZoneType, float>();
        if (Decks.Count == 0) return areas;

        float cell     = Decks[0].cellSize;
        float cellArea = cell * cell;

        foreach (var g in Decks)
        {
            for (int x = 0; x < g.cols; x++)
            for (int y = 0; y < g.rows; y++)
            {
                var t = g.Tiles[x, y];
                if (t == null || t.Type == ZoneType.None) continue;
                if (!areas.ContainsKey(t.Type)) areas[t.Type] = 0f;
                areas[t.Type] += cellArea;
            }
        }
        return areas;
    }
}

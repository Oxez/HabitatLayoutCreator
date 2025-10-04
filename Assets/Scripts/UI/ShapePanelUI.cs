// Author Oxe
// Created at 04.10.2025 13:29

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapePanelUI : MonoBehaviour
{
    public HabitatShapePreset preset;
    public HabitatShapeView view;
    public DeckManager decks;
    public ZoneBrush brush; // чтобы автоматически переключать активный grid

    [Header("UI")]
    public TMP_Dropdown ddShape;
    public Slider slDiameter;
    public Slider slLength;
    public TMP_InputField inDecks;
    public TMP_Dropdown ddStow;
    public Button btnDeckPrev, btnDeckNext;
    public TextMeshProUGUI txtDeck;

    void Start()
    {
        // заполнение dropdown’ов
        ddShape.ClearOptions();
        ddShape.AddOptions(System.Enum.GetNames(typeof(HabitatShape)).ToList());
        ddShape.value = (int)preset.Shape;
        ddShape.onValueChanged.AddListener(v => { preset.Shape = (HabitatShape)v; RebuildAll(); });

        ddStow.ClearOptions();
        ddStow.AddOptions(System.Enum.GetNames(typeof(StowOrientation)).ToList());
        ddStow.value = (int)preset.Stow;
        ddStow.onValueChanged.AddListener(v => { preset.Stow = (StowOrientation)v; });

        slDiameter.minValue = 3; slDiameter.maxValue = 12; slDiameter.value = preset.Diameter;
        slLength.minValue = 4; slLength.maxValue = 20; slLength.value = preset.Length;

        slDiameter.onValueChanged.AddListener(v => { preset.Diameter = Mathf.Round(v*10f)/10f; RebuildAll(); });
        slLength.onValueChanged.AddListener(v => { preset.Length = Mathf.Round(v*10f)/10f; RebuildAll(); });

        inDecks.text = preset.Decks.ToString();
        inDecks.onEndEdit.AddListener(s => {
            if (int.TryParse(s, out var d)) { preset.Decks = Mathf.Clamp(d,1,5); RebuildAll(); }
            inDecks.text = preset.Decks.ToString();
        });

        btnDeckPrev.onClick.AddListener(() => { decks.SetActiveDeck(decks.ActiveDeck - 1); RefreshDeckLabel(); SyncBrush(); });
        btnDeckNext.onClick.AddListener(() => { decks.SetActiveDeck(decks.ActiveDeck + 1); RefreshDeckLabel(); SyncBrush(); });

        var walls = FindObjectOfType<GridWalls>();
        if (walls) walls.SetGrid(decks.Current);

        ddStow.onValueChanged.AddListener(v => {
            preset.Stow = (StowOrientation)v;
            Debug.Log($"[ShapePanelUI] Stow changed to {preset.Stow}");
        });

        RebuildAll();
    }

    void RebuildAll()
    {
        view.BuildShell();
        decks.BuildDecks();
        RefreshDeckLabel();
        SyncBrush();
    }

    void RefreshDeckLabel()
    {
        txtDeck.text = $"Deck: {decks.ActiveDeck+1}/{decks.Decks.Count}";
    }

    void SyncBrush()
    {
        if (brush != null) brush.grid = decks.Current;
    }
}

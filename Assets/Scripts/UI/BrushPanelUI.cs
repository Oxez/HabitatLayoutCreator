// Author Oxe
// Created at 04.10.2025 18:11


using UnityEngine;
using UnityEngine.UI;

public class BrushPanelUI : MonoBehaviour
{
    public ZoneBrush zoneBrush;

    [Header("UI")]
    public Button Sleep;
    public Button Hygiene;
    public Button ECLSS;
    public Button Galley;
    public Button Storage;
    public Button Exercise;
    public Button Medical;
    public Button Maintenance;
    public Button Recreation;
    public Button Airlock;
    public Button Corridor;
    //public Button btnDeckNext;

    void Start()
    {
        Sleep.onClick.AddListener(() => {
            zoneBrush.SetCurrentBrush(ZoneType.Sleep);
        });

        Hygiene.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Hygiene);
        });

        ECLSS.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.ECLSS);
        });
        
        Galley.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Galley);
        });

        Storage.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Storage);
        });

        Exercise.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Exercise);
        });

        Medical.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Medical);
        });

        Maintenance.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Maintenance);
        });

        Recreation.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Recreation);
        });

        Airlock.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Airlock);
        });

        Corridor.onClick.AddListener(() =>
        {
            zoneBrush.SetCurrentBrush(ZoneType.Corridor);
        });

    }
}

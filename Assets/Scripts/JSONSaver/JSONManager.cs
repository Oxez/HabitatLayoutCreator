//Author bulan
//05.10.2025 3:06
using UnityEngine;
using UnityEngine.UI;
using System.IO;
//Написал JSON на примере кнопки просто подставьте данные которые надо сохранять
public class JSONManager : MonoBehaviour
{
    public Button colorButton;
    private bool _activeButton;
    public Button saveButton;
    private string _savePath;
    private SaveData _data = new SaveData();

    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "data.json");
        
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _data = JsonUtility.FromJson<SaveData>(json);
            _activeButton = _data.IsGreen;
        }
        else
        {
            _activeButton = false;
        }

        // Загружаем сохраненные данные из игры ранее кнопка "Load" бесполезна
        LoadData();
        
        colorButton.onClick.AddListener(ToggleData);
        saveButton.onClick.AddListener((() =>
        {
            SaveButtonState();
        }));
    }

    void ToggleData()
    {
        _activeButton = !_activeButton;
        _data.IsGreen = _activeButton;
        LoadData();
        
    }

    void LoadData()
    {
        Image img = colorButton.GetComponent<Image>();
        img.color = _activeButton ? Color.green : Color.red;
    }

    void SaveButtonState()
    {
        string json = JsonUtility.ToJson(_data, true);
        File.WriteAllText(_savePath, json);
        Debug.Log("Save done");
    }
}


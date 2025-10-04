//Author bulan
//05.10.2025 3:06
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public Button StartBt;
    public Button SettingBt;
    public Button ModelsBt; //Для будущего меню "ModelsMenu"
    public Button DevelopersBt;
    public Button DeveloperMenuExitBt;
    public Button SettingMenuExitBt;
    public GameObject DevelopersMenu;
    public GameObject MainMenu;
    public GameObject SettingMenu;

    private void Start()
    {
            StartBt.onClick.AddListener((() =>
            {
                SceneManager.LoadScene("Editor");
            }));
            DevelopersBt.onClick.AddListener((() =>
            {
                MainMenu.SetActive(false);
                DevelopersMenu.SetActive(true);
            }));
            DeveloperMenuExitBt.onClick.AddListener((() =>
            {
                MainMenu.SetActive(true);
                DevelopersMenu.SetActive(false);
            }));
            SettingBt.onClick.AddListener((() =>
            {
                MainMenu.SetActive(false);
                SettingMenu.SetActive(true);
            }));
            SettingMenuExitBt.onClick.AddListener((() =>
            {
                MainMenu.SetActive(true);
                SettingMenu.SetActive(false);
            }));
    }
}

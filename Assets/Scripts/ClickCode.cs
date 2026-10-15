using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ClickCode : MonoBehaviour
{
    [SerializeField] private GameObject MainManu;
    [SerializeField] private GameObject LevelManu;
    [SerializeField] private GameObject SetingManu;
    [SerializeField] private GameObject HUD;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnLevelClick()
    {
        MainManu.SetActive(false);
        LevelManu.SetActive(true);
    }

    public void OnQuitClick()
    {
        Application.Quit();             
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnSetingClick()
    {
        MainManu.SetActive(false);
        SetingManu.SetActive(true);
    }
    public void OnLevel1Click()
    {
        LevelManu.SetActive(false);
        HUD.SetActive(true);
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    public GameObject infoPanel;
    public TMP_Text enemyNameText;
    public TMP_Text descriptionText;
    public TMP_Text hpText;
    public Slider healthSlider;

    private EnemyHealthDisplay selectedEnemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        infoPanel.SetActive(false);
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.wholeNumbers = false;
    }

    public void SelectEnemy(EnemyHealthDisplay enemy)
    {
        selectedEnemy = enemy;
        RefreshedDisplay();
    }

    void LateUpdate()
    {
        RefreshedDisplay();
    }

    private void RefreshedDisplay()
    {
        if (selectedEnemy == null)
        {
            infoPanel.SetActive(false);
            return;
        }

        infoPanel.SetActive(true);
        enemyNameText.text = selectedEnemy.enemyName;
        descriptionText.text = selectedEnemy.discription;
  
        hpText.text = $"HP: {Mathf.CeilToInt(selectedEnemy.CurrentHealth)} / {Mathf.CeilToInt(selectedEnemy.MaxHealth)}"; // show text in dispaly of current enemy. 
        healthSlider.value = selectedEnemy.CurrentHealth / selectedEnemy.MaxHealth; // Show enemy health bar on slider
    }
}
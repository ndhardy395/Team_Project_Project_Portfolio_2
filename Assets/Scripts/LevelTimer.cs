using UnityEngine;
using TMPro;
public class LevelTimer : MonoBehaviour
{
    float timer = 0;
    private TextMeshProUGUI curText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curText = this.transform.Find("background/Timer Text").gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timer+= Time.deltaTime;
        UpdateTimerText(timer);
    }

    void UpdateTimerText(float curTime)
    {
        int minutes = Mathf.FloorToInt(curTime / 60);
        int seconds = Mathf.FloorToInt(curTime % 60);

        curText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}

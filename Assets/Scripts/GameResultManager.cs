
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultManager : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private WaveSpawner waveSpawner;

    [SerializeField] private GameObject[] hideOnGameEnd;


    private bool gameEnded;

    private void Awake()
    {
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void WinGame()
    {
        // Destroyed base cannot win, even if final enemy left.

        if(gameEnded || baseHealth == null || !baseHealth.IsAlive)
        {
            return;
        }
        EndGame(winPanel);

        // Saves stars and best wave for the level select (does nothing if the level wasn't started from it).
        LevelSession.RecordWin(waveSpawner != null ? waveSpawner.CurrentWaveNumber : 0);
    }

    public void LoseGame()
    {

        if (gameEnded)
        {
            return;
        }
            EndGame(gameOverPanel);

            LevelSession.RecordLoss(waveSpawner != null ? waveSpawner.CurrentWaveNumber : 0);

            return;
        
    }


    private void EndGame(GameObject panel)
    {
        if(panel == null)
        {
            Debug.LogError("Assign the win panel and game over panel.", this);
            return;
        }

        gameEnded = true;

        if(ClickCode.Instance != null)
        {
            ClickCode.Instance.HideGamePlayUI();
        }

        if(hideOnGameEnd != null)
        {
            foreach(GameObject hudObject in hideOnGameEnd)
            {
                // Keep the resul panel and its parents enabled.
                if(hudObject != null && hudObject != panel && !panel.transform.IsChildOf(hudObject.transform))
                {
                    hudObject.SetActive(false);
                }
            }
        }

        if(waveSpawner != null)
        {
            waveSpawner.StopAllCoroutines();
        }

        Canvas resultCanvas = panel.GetComponentInParent<Canvas>(true);

        if(resultCanvas != null)
        {
            resultCanvas.gameObject.SetActive(true);
        }

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        if(!panel.activeInHierarchy)
        {
            Debug.LogError("Result panel is hidden becasue a parent is inactive. " + "Place it under a seperate, active canvas outside the gameplay Hud. ", panel);
        }
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
       if(ClickCode.Instance != null)
        {
            ClickCode.Instance.RestoreStartButton();

            Time.timeScale = 0f;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void LeaveToMainMenu()
    {
        if (ClickCode.Instance == null)
        {
            Debug.LogError("ClickCode is missing. Start the game from the UI scene.");
            return;
        }

        ClickCode.Instance.ReturnToMainMenu();
    }

    public void ContinueToLevelSelect()
    {
        if(ClickCode.Instance == null)
        {
            return;
        }
        ClickCode.Instance.ReturnToLevelSelect();
    }
}

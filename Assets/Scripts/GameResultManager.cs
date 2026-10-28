using System.Reflection.Metadata.Ecma335;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultManager : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private WaveSpawner waveSpawner;


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
 
    }

    public void LoseGame()
    {
        if(gameEnded)
        {
            return;
        }
        EndGame(gameOverPanel);
    }


    private void EndGame(GameObject panel)
    {
        gameEnded = true;

        // stop pending waves and spawn groups.

        waveSpawner.StopAllCoroutines();

        panel.SetActive(true);

        panel.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
}

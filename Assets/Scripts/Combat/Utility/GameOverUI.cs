using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("Pannello Game Over")]
    public GameObject pannelloGameOver;

    // Inizializza il singleton e nasconde il pannello game over
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        pannelloGameOver.SetActive(false);
    }

    // Mostra il pannello game over, sblocca il cursore e ferma il tempo
    public void Mostra()
    {
        pannelloGameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Blocca il tempo — nessun input, nessuna fisica
        Time.timeScale = 0f;
        GameState.inputBloccato = true;
    }

    // Riavvia la scena corrente ripristinando il tempo e lo stato dell'input
    public void RicaricaScena()
    {
        Time.timeScale = 1f;
        GameState.inputBloccato = false;
        pannelloGameOver.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

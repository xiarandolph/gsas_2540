using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shmup
{

public class CanvasManager : MonoBehaviour
{
    // children GameObjects
    Text scoreDisplay;
    Text highScoreDisplay;
    Text waveDisplay;
    Text enemyCountDisplay;
    public GameObject nextWaveButton;
    HealthBarController timer;
    GameObject startScreen;
    GameObject pauseScreen;
    GameObject endScreen;
    Text endScreenDisplay;

    enum Screen {NONE=0, START, PAUSE, END};

    // game variables
    Screen current; // keep track of the current screen for some game logic
    int highScore;

    void Awake()
    {
        scoreDisplay = transform.Find("Score Label").GetComponent<Text>();
        highScoreDisplay = transform.Find("Hi Score Label").GetComponent<Text>();
        waveDisplay = transform.Find("Wave Label").GetComponent<Text>();
        enemyCountDisplay = transform.Find("Enemies Label").GetComponent<Text>();
        nextWaveButton = transform.Find("Next Wave Button").gameObject;
        timer = transform.Find("Timer").GetComponent<HealthBarController>();
        startScreen = transform.Find("Start Screen").gameObject;
        pauseScreen = transform.Find("Pause Screen").gameObject;
        endScreen = transform.Find("End Screen").gameObject;
        endScreenDisplay = endScreen.transform.Find("Text").GetComponent<Text>();

        highScore = PlayerPrefs.GetInt("Shmup High Score", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        highScoreDisplay.text = String.Format("High Score: {0:d}", highScore);
        UpdateScore(0);
    }

    // Make sure the timeScale is reset to avoid breaking other scenes
    void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }

    public void UpdateScore(int score)
    {
        scoreDisplay.text = String.Format("Score: {0:d}", score);
        if (score > highScore)
        {
            // note: high score will be updated even if player exits mid game
            PlayerPrefs.SetInt("Shmup High Score", highScore = score);
            highScoreDisplay.text = String.Format("High Score: {0:d}", score);
        }

        // update end screen display as well
        endScreenDisplay.text = "Game Over!\n";
        endScreenDisplay.text += scoreDisplay.text + "\n";
        endScreenDisplay.text += highScoreDisplay.text + "\n";
    }

    void Update()
    {
        // toggle pause screen only if no other screen is currently displayed
        if (Input.GetKeyDown("escape") && (current == Screen.NONE || current == Screen.PAUSE))
            DisplayScreen((current == Screen.NONE) ? Screen.PAUSE : Screen.NONE);
    }

    // overload as public method for other classes / buttons to access with int rather than enum
    public void DisplayScreen(int screen)
    {
        DisplayScreen((Screen) screen);
    }

    void DisplayScreen(Screen screen)
    {
        current = screen;

        // use timeScale for pausing / resuming gameplay
        Time.timeScale = (screen == Screen.NONE) ? 1.0f : 0.0f;

        startScreen.SetActive(screen == Screen.START);
        pauseScreen.SetActive(screen == Screen.PAUSE);
        endScreen.SetActive(screen == Screen.END);
    }

    public void UpdateWave(int wave)
    {
        waveDisplay.text = String.Format("Wave {0:d}", wave);
    }

    public void UpdateTimer(float percentage)
    {
        timer.UpdateBar(percentage);
    }

    public void UpdateEnemies(int enemies)
    {
        enemyCountDisplay.text = String.Format("Remaining Enemies: {0:d}", enemies);
    }

    public void UpdateLives(int num)
    {
        for (int i = 3; i > num; --i)
            transform.Find(String.Format("Heart {0:d}", i)).gameObject.SetActive(false);

        for (int i = num; i > 0; --i)
            transform.Find(String.Format("Heart {0:d}", i)).gameObject.SetActive(true);
    }
}


} // namespace Shmup
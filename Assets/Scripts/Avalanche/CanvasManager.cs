using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Avalanche
{

// CanvasManager controls the HUD and separate "screens"
// The game will be paused whenever any screens are displayed
// Also manages the HighScore through PlayerPrefs
public class CanvasManager : MonoBehaviour
{
    // children GameObjects
    Text scoreDisplay;
    Text highScoreDisplay;
    GameObject startScreen;
    GameObject pauseScreen;
    GameObject endScreen;
    Text endScreenDisplay;

    enum Screen {NONE=0, START, PAUSE, END};

    Screen current; // keep track of the current screen for some game logic
    int highScore;  // for reduced calls to PlayerPrefs

    // Awake finds referenced GameObjects & components
    void Awake()
    {
        // Transform.Find looks for child objects
        scoreDisplay = transform.Find("Score Display").GetComponent<Text>();
        highScoreDisplay = transform.Find("Hi Score Display").GetComponent<Text>();
        startScreen = transform.Find("Start Screen").gameObject;
        pauseScreen = transform.Find("Pause Screen").gameObject;
        endScreen = transform.Find("End Screen").gameObject;
        endScreenDisplay = endScreen.transform.Find("Text").GetComponent<Text>();

        highScore = PlayerPrefs.GetInt("Avalanche High Score", 0);
    }

    void Start()
    {
        highScoreDisplay.text = String.Format("High Score: {0:d}", highScore);
        DisplayScreen(Screen.START);
    }

    // Make sure the timeScale is reset to avoid breaking other scenes
    void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }

    // score is the only thing actively changing throughout screens
    public void UpdateHUD(int score)
    {
        scoreDisplay.text = String.Format("Score: {0:d}", score);
        if (score > highScore)
        {
            // note: high score will be updated even if player exits mid game
            PlayerPrefs.SetInt("Avalanche High Score", highScore = score);
            highScoreDisplay.text = String.Format("High Score: {0:d}", score);
        }

        // update end screen display as well
        endScreenDisplay.text = "Game Over!\n";
        endScreenDisplay.text += String.Format("Score: {0:d}\n", score);
        endScreenDisplay.text += String.Format("High Score: {0:d}\n", highScore);
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

    void Update()
    {
        // toggle pause screen only if no other screen is currently displayed
        if (Input.GetKeyDown("escape") && (current == Screen.NONE || current == Screen.PAUSE))
            DisplayScreen((current == Screen.NONE) ? Screen.PAUSE : Screen.NONE);
    }

    // StartGame is called by "Start" on Start Screen or "Restart" on End Screen
    public void StartGame()
    {
        DisplayScreen(Screen.NONE);
        GameObject.Find("Game Manager").SendMessage("InitializeGame");
    }
}

} // namespace Avalanche
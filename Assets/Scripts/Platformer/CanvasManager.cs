using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{

// CanvasManager controls the HUD and separate "screens"
// The game will be paused whenever any screens are displayed
// Also manages the HighScore through PlayerPrefs
public class CanvasManager : MonoBehaviour
{
    // children GameObjects
    GameObject pauseScreen;

    enum Screen {NONE=0, PAUSE};

    Screen current; // keep track of the current screen for some game logic

    // Awake finds referenced GameObjects & components
    void Awake()
    {
        // Transform.Find looks for child objects
        pauseScreen = transform.Find("Pause Screen").gameObject;
    }

    void Start()
    {
        DisplayScreen(Screen.NONE);
    }

    // Make sure the timeScale is reset to avoid breaking other scenes
    void OnDestroy()
    {
        Time.timeScale = 1.0f;
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

        pauseScreen.SetActive(screen == Screen.PAUSE);
    }

    void Update()
    {
        // toggle pause screen only if no other screen is currently displayed
        if (Input.GetKeyDown("escape") && (current == Screen.NONE || current == Screen.PAUSE))
            DisplayScreen((current == Screen.NONE) ? Screen.PAUSE : Screen.NONE);
    }
}

} // namespace Platformer
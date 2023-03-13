using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure
{

// LogController is meant to be placed on the in-game display log for other classes to call
public class LogController : MonoBehaviour
{

    Text log;

    void Start()
    {
        log = GetComponent<Text>();
    }

    // Clear empties the log of all text
    public void Clear()
    {
        log.text = "";
    }

    // Prints to the in game log
    public void Print(string str)
    {
        log.text += String.Format("\n{0}", str);
        Canvas.ForceUpdateCanvases();   // needed to get accurate line count

        // if there are too many lines, remove oldest log message
        string[] lines = log.text.Split('\n');
        if (lines.Length > log.cachedTextGenerator.lineCount)
            log.text = log.text.Substring(lines[0].Length).Trim();
    }
}

} // namespace Adventure
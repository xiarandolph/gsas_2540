using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Avalanche
{

// GameManager initializes the game (starting positions and borders)
public class GameManager : MonoBehaviour
{
    // referenced GameObjects
    CanvasManager canvas;
    Camera mainCamera;
    Rigidbody2D player;
    [SerializeField] GameObject enemyPrefab = null; // use for resetting game / multiple enemies
    [SerializeField] Collider2D collectionField;    // for drops that are missed

    // game variables
    Rigidbody2D enemy;                              // TODO: Consider list of enemies if desired

    // Awake finds referenced GameObjects & components
    void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<CanvasManager>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();

        // default to "Bottom Border" if not set ...
        if (collectionField == null) collectionField = GameObject.Find("Bottom Border").GetComponent<Collider2D>();
    }

    void Start()
    {
        InitializeGame();
    }

    // Initialize positions for relevant objects and initialize game variables
    void InitializeGame()
    {
        // Destroy any previously existing enemies to reset game logic
        if (enemy != null) GameObject.Destroy(enemy.gameObject);
        enemy = Instantiate(enemyPrefab, null).GetComponent<Rigidbody2D>();

        // use camera to get world size to initialize positions
        player.position = mainCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.1f));
        enemy.position = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.9f, 0.0f));
        collectionField.transform.position = mainCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.0f));

        player.GetComponent<PlayerController>().Restart();
    }

}

} // namespace Avalanche
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Avalanche
{

// PlayerController updates the attached Rigidbody to follow the mouse and
// manages current score and lives left
public class PlayerController : MonoBehaviour
{
    // referenced GameObjects/Components
    [SerializeField] GameObject lifePrefab = null;
    Camera cam;
    Rigidbody2D rb;
    AudioSource catchAudio;
    AudioSource missAudio;

    // game variables
    [SerializeField] int startingLives = 3;
    private int _score = 0;
    private int _lives;

    private GameObject[] lifeContainers;

    // variable properties
    public int score
    {
        get { return _score; }
        set {
            _score = value;
            GameObject.Find("Canvas").SendMessage("UpdateHUD", score);
            if (score > 0) catchAudio.Play();
        }
    }

    public int lives
    {
        get { return _lives; }
        set {
            _lives = value;

            if (_lives < startingLives)
            {
                lifeContainers[_lives].SetActive(false);
                missAudio.Play();
            }

            // show end screen
            if (_lives == 0) GameObject.Find("Canvas").SendMessage("DisplayScreen", 3);
        }
    }

    // Awake finds referenced GameObjects & components
    void Awake()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();

        // transform.Find searches children
        catchAudio = transform.Find("Catch Audio").GetComponent<AudioSource>();
        missAudio = transform.Find("Miss Audio").GetComponent<AudioSource>();
    }

    void Start()
    {
        // lives are instantiated as child objects staggered above eachother
        lifeContainers = new GameObject[startingLives];
        for (int i = 0; i < startingLives; ++i)
        {
            lifeContainers[i] = Instantiate(lifePrefab, transform);
            lifeContainers[i].transform.position += new Vector3(0, 0.5f*i, 0);
        }

        Restart();
    }

    // Restart re-activates all life containers and set score appropriately
    public void Restart()
    {
        for (int i = 0; i < startingLives; ++i)
            lifeContainers[i].SetActive(true);
        lives = startingLives;
        score = 0;
    }

    void Update()
    {
        // move directly to mouse, note that the rb has y position locked
        rb.MovePosition(cam.ScreenToWorldPoint(Input.mousePosition));
    }
}

} // namespace Avalanche
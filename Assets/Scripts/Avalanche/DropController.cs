using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Avalanche
{

// DropController keeps a list of all instantiated drops.
// When attached to a Drop, it also checks for collisions and applies the
// appropriate game logic for the collision.
public class DropController : MonoBehaviour
{
    // referenced GameObjects
    AudioSource dropAudio;

    // game variables
    private static List<DropController> drops;  // list of all instantiated drops
    private static float resumeTime = -1;       // keeps track of when drops are allowed to spawn
    [SerializeField] float pauseTime = 2.0f;

    // Treat drop list as a Singleton.
    void Awake()
    {
        if (drops != null) return;
        if (resumeTime > 0) return;
        resumeTime = 0;
        drops = new List<DropController>();
    }

    // Keep track of the Drop within the list and rely on physics to have the Drop
    // simply fall straight down.
    void Start()
    {
        // if drops are currently paused, don't spawn
        if (Time.time < resumeTime)
        {
            Destroy(this.gameObject);
            return;
        }

        drops.Add(this);

        // not in awake because object might not be created yet ...
        dropAudio = transform.Find("Drop Audio").GetComponent<AudioSource>();
        dropAudio.Play();
        GetComponent<Rigidbody2D>().velocity = Vector2.down;
    }

    // Necessary to clean up list (especially when scene is destroyed w/ drops still existing)
    void OnDestroy()
    {
        drops.Remove(this);
    }

    // DestroyAll will destroy all existing drops.
    void DestroyAll()
    {
        foreach (DropController drop in drops)
            GameObject.Destroy(drop.gameObject);
        drops.Clear();
    }

    // OnCollisionEnter2D will destroy the attached object and perform necessary game logic.
    void OnCollisionEnter2D(Collision2D col)
    {
        // Destroy object and remove from drop list
        GameObject.Destroy(this.gameObject);

        PlayerController player = GameObject.Find("Player").GetComponent<PlayerController>();

        if (col.collider.tag == "Player")
            player.score++;
        else
        {
            player.lives--;
            DestroyAll();

            // prevent any drops from spawning for "pauseTime" seconds
            resumeTime = Time.time + pauseTime;
        }
    }
}

} // namespace Avalanche
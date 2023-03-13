using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class GameManager : MonoBehaviour
{
    // referenced gameObjects
    [SerializeField] GameObject enemyPrefab = null;
    [SerializeField] GameObject boulderPrefab = null;
    Camera cam;
    Rigidbody2D player;
    CanvasManager canvas;
    HealthBarController timer;

    // logic variables, modified in functions
    float smoothTime = 0.3f;                // SmoothDamp: rough time it takes camera to follow player
    Vector3 camVelocity = Vector3.zero;     // used with SmoothDamp
    Coroutine waveCoroutine = null;
    List<Entity> entities;                  // to destroy all remaining entities

    // game variables
    int lives = 3;
    int score = 0;
    int wave = 0;
    bool spawning = false;
    int remainingEnemies = 0;

    void Awake()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        canvas = GameObject.Find("Canvas").GetComponent<CanvasManager>();
    }

    void Start()
    {
        canvas.DisplayScreen(1);    // display start screen
    }

    public void StartGame()
    {
        canvas.DisplayScreen(0);    // no screen
        lives = 3;
        canvas.UpdateScore(score = 0);
        wave = 0;
        canvas.UpdateWave(1);
        canvas.UpdateTimer(0.0f);
        entities = new List<Entity>();
        canvas.UpdateLives(lives);
        player.GetComponent<PlayerController>().Respawn();
    }

    void FixedUpdate()
    {
        // make camera follow player, use smoothdamp for slow follow behind
        Vector3 pos = new Vector3(player.position.x, player.position.y, cam.transform.position.z);
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, pos, ref camVelocity, smoothTime);
    }

    // EnemyKilled is to be called when an enemy dies
    // Updates player score and the remaining number of enemies
    public void EnemyKilled(Entity entity)
    {
        // boulders are not counted and don't raise score
        if (entity is EnemyController)
        {
            canvas.UpdateEnemies(--remainingEnemies);
            canvas.UpdateScore(++score);
        }

        entities.Remove(entity);

        

        if (!spawning && remainingEnemies == 0)
            canvas.nextWaveButton.SetActive(true);
    }

    // PlayerKilled is to be called when the player dies
    public void PlayerKilled()
    {
        spawning = false;
        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);
        foreach (Entity entity in entities)
        {
            Destroy(entity.gameObject);
        }
        entities.Clear();
        ProjectileController.DestroyAll();
        canvas.UpdateEnemies(remainingEnemies = 0);
        --wave; // because ComputerNextWave increments wave

        if (--lives > 0)
            player.GetComponent<PlayerController>().Respawn();
        canvas.UpdateLives(lives);

        // end the game
        if (lives == 0)
        {
            canvas.DisplayScreen(3);
        }
    }

    // ComputeNextWave does logic for determining what enemies spawn
    public void ComputeNextWave()
    {
        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);
    
        int numEnemies = 5 + 3*wave;
        float rate = Mathf.Max(1.5f - 0.1f*wave, 0.2f);
        float time = 20.0f + wave * 2.0f;

        waveCoroutine = StartCoroutine(StartWave(++wave, numEnemies, rate, time));
    }

    // SpawnBoulders will continously spawn boulders offscreen
    // which roll towards the player as long as there are enemies remaining
    IEnumerator SpawnBoulders(int currentWave)
    {
        // get world distance to corner from center of screen to spawn off screen
        float dist = (cam.ViewportToWorldPoint(Vector2.one) -
            cam.ViewportToWorldPoint(Vector2.zero)).magnitude / 2;
        GameObject obj;
        Vector2 dir;
        
        // prevent two spawnings at the same time which might occur from
        // next wave button
        while (wave == currentWave && (spawning || remainingEnemies > 0))
        {
            // choose random direction to spawn boulder at
            dir = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector2.up;

            obj = Instantiate(boulderPrefab, player.position + dir * dist, Quaternion.identity);
            entities.Add(obj.GetComponent<Entity>());

            // aim somewhat in the direction of the player
            dir = (player.position + 2*player.velocity) - (Vector2) obj.transform.position;
            dir = Quaternion.Euler(0, 0, Random.Range(-10, 10)) * dir.normalized;
            
            obj.GetComponent<Rigidbody2D>().velocity = 7 * dir;

            // spawn next boulder random amount of time later
            yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        }
    }

    // Start wave number "wave" which will have a total of numEnemies
    // each spawned with rate seconds in between, then time seconds before 
    // the next wave begins.
    // time is only accurate up to the tenth's place.
    IEnumerator StartWave(int wave, int numEnemies, float rate, float time)
    {
        canvas.UpdateWave(wave);
        canvas.nextWaveButton.SetActive(false);
        StartCoroutine(SpawnWave(numEnemies, rate));
        StartCoroutine(SpawnBoulders(wave));
        // update timer bar to reflect when wave will finish spawning
        for (int i = 0; i < 10 * numEnemies * rate; ++i)
        {
            canvas.UpdateTimer( i / (10.0f * numEnemies * rate));
            yield return new WaitForSeconds(0.1f);
        }

        // update timer bar to reflect wave time running out
        for (int i = (int) (10*time) - 1; i >= 0; --i)
        {
            canvas.UpdateTimer( i / (10 * time));
            yield return new WaitForSeconds(0.1f);
        }

        waveCoroutine = null;
        ComputeNextWave();
    }

    // ValidSpawnLocation returns true if there are no colliders within bounds
    // specified by pos and extents
    bool ValidSpawnLocation(Vector2 pos, Vector2 extents)
    {
        return Physics2D.OverlapArea(pos - extents, pos + extents) == null;
    }

    // SpawnWave spawns a total of num enemies, one every rate seconds
    IEnumerator SpawnWave(int num, float rate)
    {
        spawning = true;
        Vector2 extents = 0.4f * Vector2.one;
        Vector2 dir;
        float dist;
        GameObject obj;

        for (int i = 0; i < num; ++i)
        {
            // not spawning anymore for some reason such as player dying
            if (!spawning) yield break;

            dir = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector2.up;
            dist = Random.Range(6.0f, 15.0f);

            // keep trying to find a valid spawning location 
            while (!ValidSpawnLocation(player.position + dir * dist, extents))
            {
                dir = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector2.up;
                yield return null;
            }

            obj = Instantiate(enemyPrefab, player.position + dir * dist, Quaternion.identity);
            entities.Add(obj.GetComponent<Entity>());

            ++remainingEnemies;
            canvas.UpdateEnemies(remainingEnemies);
            yield return new WaitForSeconds(rate);
        }
        spawning = false;
    }
}

} // namespace Shmup

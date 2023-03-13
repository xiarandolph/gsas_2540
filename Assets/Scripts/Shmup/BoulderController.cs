using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class BoulderController : Entity
{
    // referenced game objects
    [SerializeField] GameObject powerupPrefab = null;
    static GameManager gm = null;
    Rigidbody2D rb;
    Animator animator;
    Renderer render; // not named renderer because of Component.renderer
    AudioSource deathAudio;

    // game variables
    [SerializeField] float collisionDamage = 3.0f;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        if (gm == null)
            gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        render = GetComponent<Renderer>();
        deathAudio = transform.Find("Death Audio").GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine("DestroyIfInvisible");
    }

    // DestroyIfInvisible checks if the boulder is off screen for
    // 2 seconds, if so then it will destroy itself
    IEnumerator DestroyIfInvisible()
    {
        while (true)
        {
            if (render.isVisible)
                yield return new WaitForSeconds(0.3f);
            else
            {
                yield return new WaitForSeconds(2.0f);
                if (!render.isVisible)
                {
                    gm.EnemyKilled(this);
                    Destroy(this.gameObject);
                }
            }
            yield return new WaitForSeconds(0.3f); 
        }
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        Entity entity = col.gameObject.GetComponent<Entity>();
        
        if (entity != null)
        {
            entity.Damage(collisionDamage);
        }
    }

    public override void Die()
    {
        // disable future collisions to prevent repeated "dying"
        GetComponent<Collider2D>().isTrigger = true;

        health = 0;
        healthBar.UpdateBar(0);
        animator.Play("death-explosion");
        deathAudio.Play();

        rb.velocity = Vector2.zero;

        Instantiate(powerupPrefab, rb.position, Quaternion.identity);

        gm.EnemyKilled(this);
    }
}


} // namespace Shmup


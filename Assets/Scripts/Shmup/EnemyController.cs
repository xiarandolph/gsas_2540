using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class EnemyController : Entity
{
    // referenced gameObjects
    [SerializeField] GameObject projectilePrefab = null;
    static Rigidbody2D player = null;   // make static since all enemies will use this
    static GameManager gm = null;
    Rigidbody2D rb;
    Animator animator;
    AudioSource deathAudio;

    // logic variables, modified by functions
    bool firing = false;

    // game variables
    [SerializeField] float collisionDamage = 2.5f;
    [SerializeField] float followDistance = 5.0f;
    [SerializeField] float steeringSpeed = 5.0f;

    [SerializeField] float projSpeed = 9.0f;
    [SerializeField] float projDamage = 1.0f;
    [SerializeField] float fireTime = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        if (gm == null)
            gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        deathAudio = transform.Find("Death Audio").GetComponent<AudioSource>();
    }

    void Start()
    {
        // randomize variables
        followDistance *= Random.Range(0.8f, 1.5f);
        moveSpeed *= Random.Range(0.9f, 1.5f);
        fireTime *= Random.Range(0.8f, 1.3f);

        StartCoroutine("TrackPlayer");
    }

    void FixedUpdate()
    {
        // rotate towards player
        Vector2 delta = player.position - rb.position;
        rb.SetRotation(Vector2.SignedAngle(Vector2.up, delta));

        // shoot if within range
        if (delta.magnitude < 1.5f * followDistance)
            StartCoroutine("FireProjectile");
    }

    IEnumerator FireProjectile()
    {
        if (firing == true) yield break;
        firing = true;

        // direction the enemy is currently facing
        Vector2 dir = Quaternion.Euler(0, 0, rb.rotation) * Vector2.up;

        GameObject obj = Instantiate(projectilePrefab, transform.position, transform.rotation);

        // enemies can't shoot eachother
        obj.layer = LayerMask.NameToLayer("Enemy");

        // set projectile color, velocity and damage
        obj.GetComponent<SpriteRenderer>().color = Color.red;
        obj.GetComponent<Rigidbody2D>().velocity = dir.normalized * projSpeed;
        ProjectileController proj = obj.GetComponent<ProjectileController>();
        proj.damage = projDamage;

        // cooldown before enemy can fire again
        yield return new WaitForSeconds(fireTime);
        firing = false;
    }

    // TrackPlayer: determine player location and try to follow at set distance
    IEnumerator TrackPlayer()
    {
        Vector2 desiredPos;
        Vector2 desiredVelocity;
        Vector2 steeringForce;
        float distance;

        while (true)
        {
            // track player from a set distance away
            desiredPos = rb.position - player.position;
            desiredPos = player.position + desiredPos.normalized * followDistance;
            distance = (desiredPos - rb.position).magnitude;

            // calculate necessary velocity to desired position
            desiredVelocity = moveSpeed * (desiredPos - rb.position).normalized;
            if (distance < 1.0f)
                desiredVelocity *= distance;    // slow down when reaching desired point

            // get necessary steering force to reach desired velocity
            steeringForce = desiredVelocity - rb.velocity;
            steeringForce += AvoidCollisions(0.3f);
            steeringForce = Vector2.ClampMagnitude(steeringForce, steeringSpeed);

            rb.velocity += steeringForce;
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, moveSpeed);

            yield return null;
        }
    }

    // AvoidCollisions returns a Force which will keep current entity away from others
    // currently used to make enemies stay slightly away from each other so they don't all
    // pile up into one big area even though they can't collide with eachother
    Vector2 AvoidCollisions(float radius)
    {
        Collider2D[] nearby;
        Rigidbody2D col_rb;
        Vector2 force = Vector2.zero;
        Vector2 delta;

        nearby = Physics2D.OverlapCircleAll(rb.position, radius);

        foreach (Collider2D col in nearby)
        {
            col_rb = col.GetComponent<Rigidbody2D>();
            // projectiles are colliders without rigidbodies
            if (col_rb != null)
            {
                delta = rb.position - (Vector2) col_rb.position;
                force += delta;
            }
        }

        return force.normalized;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Entity entity = col.gameObject.GetComponent<Entity>();
        
        if (entity != null)
        {
            entity.Damage(collisionDamage);
            Die();
        }
    }

    public override void Die()
    {
        if (GetComponent<Collider2D>().isTrigger == true)
            return;

        // disable future collisions to prevent repeated "dying"
        GetComponent<Collider2D>().isTrigger = true;

        healthBar.UpdateBar(0);
        animator.Play("death-explosion");
        deathAudio.Play();

        gm.EnemyKilled(this);
    }
}


} // namespace Shmup


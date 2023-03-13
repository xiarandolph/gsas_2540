using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class PlayerController : Entity
{
    // referenced gameobjs / components
    GameManager gm;
    Camera cam;
    Rigidbody2D rb;
    [SerializeField] GameObject projectilePrefab = null;

    // logic variables, modified in functions
    bool firing = false;                            // prevent spamming fire
    bool respawning = false;
    int[] buffs;

    // game variables
    [SerializeField] float projSpeed = 9.0f;
    [SerializeField] float projDamage = 1.0f;
    [SerializeField] float fireTime = 0.3f;         // seconds between each fire

    // Find relevant game objects and components
    protected override void Awake()
    {
        base.Awake();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        buffs = new int[(int) BuffType.TOTAL];
    }

    void FixedUpdate()
    {
        // fire projectile in direction player is firing
        if (Input.GetMouseButton(0))
            StartCoroutine(FireProjectile(Quaternion.Euler(0, 0, rb.rotation) * Vector2.up));

        // use joysticks / keys for movement
        Vector2 dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.velocity = moveSpeed * dir.normalized;

        // rotate towards mouse
        Vector2 delta = (Vector2) cam.ScreenToWorldPoint(Input.mousePosition) - rb.position;
        rb.SetRotation(Vector2.SignedAngle(Vector2.up, delta));
    }

    enum BuffType {DAMAGE=0, REGEN, RAPIDFIRE, PROJSPEED, MAXHEALTH, TOTAL};

    // GrantRandomBuff will randomly buff one of the player's stats
    public void GrantRandomBuff()
    {
        BuffType buff = (BuffType) Random.Range(0, (int) BuffType.TOTAL);
        StartCoroutine(ApplyBuff(buff));
    }

    IEnumerator ApplyBuff(BuffType buff)
    {
        // keep track of what buffs are applied in order to remove them later
        buffs[(int) buff]++;
        switch (buff)
        {
            case BuffType.DAMAGE:
                projDamage *= 1.5f;
                yield return new WaitForSeconds(20.0f);
                projDamage /= 1.5f;
                break;
            case BuffType.REGEN:
                StartCoroutine(Regenerate(10.0f, 30.0f));
                break;
            case BuffType.RAPIDFIRE:
                fireTime *= 0.3f;
                yield return new WaitForSeconds(20.0f);
                fireTime /= 0.3f;
                break;
            case BuffType.PROJSPEED:
                projSpeed *= 1.5f;
                yield return new WaitForSeconds(20.0f);
                projSpeed /= 1.5f;
                break;
            case BuffType.MAXHEALTH:
                maxHealth *= 1.1f;
                health *= 1.1f;
                buffs[(int) buff]++; // permanent buff, don't decrement when finished
                break;
            default:
                Debug.Log("Unknown buff type");
                break;
        }
        buffs[(int) buff]--;
        yield break;
    }

    public void ClearBuffs()
    {
        StopAllCoroutines();
        for (int i = 0; i < (int) BuffType.TOTAL; ++i)
        {
            for (int j = 0; j < buffs[i]; j++)
            {
                switch ((BuffType) i)
                {
                    case BuffType.DAMAGE:
                        projDamage /= 1.5f;
                        break;
                    case BuffType.REGEN: // stopped coroutine already
                        break;
                    case BuffType.RAPIDFIRE:
                        fireTime /= 0.3f;
                        break;
                    case BuffType.PROJSPEED:
                        projSpeed /= 1.5f;
                        break;
                    case BuffType.MAXHEALTH:
                        maxHealth /= 1.1f;
                        break;
                    default:
                        Debug.Log("Error in ClearBuffs");
                        break;
                }
            }
            buffs[i] = 0;
        }
    }

    public void Respawn()
    {
        ClearBuffs();
        firing = false;
        StartCoroutine("RespawnCoroutine");
    }

    IEnumerator RespawnCoroutine()
    {
        respawning = true;
        rb.MovePosition(Vector2.zero);

        StartCoroutine(Regenerate(maxHealth, 3.0f));
        yield return new WaitForSeconds(3.0f);

        respawning = false;
        
        gm.ComputeNextWave();
    }

    // Regenerate player health by total amount over total time
    IEnumerator Regenerate(float amount, float time)
    {
        for (int i = 0; i < (int) (10 * time); ++i)
        {
            Damage(-amount / (10 * time));
            yield return new WaitForSeconds(0.1f);
        }
    }

    // FireProjectile instantiates a projectile in direction dir
    IEnumerator FireProjectile(Vector2 dir)
    {
        if (firing == true) yield break;
        firing = true;

        GameObject obj = Instantiate(projectilePrefab, transform.position, transform.rotation);

        // make sure player can't shoot itself
        Physics2D.IgnoreCollision(obj.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        // set projectile color, velocity and damage
        obj.GetComponent<SpriteRenderer>().color = Color.black;
        obj.GetComponent<Rigidbody2D>().velocity = dir.normalized * projSpeed;
        ProjectileController proj = obj.GetComponent<ProjectileController>();
        proj.damage = projDamage;

        // cooldown before player can fire again
        yield return new WaitForSeconds(fireTime);
        firing = false;
    }

    public override void Die()
    {
        if (!respawning)
            gm.PlayerKilled();
        
        // set even if not necessarily true to prevent multiple "deaths"
        respawning = true;
    }
}

} // namespace Shmup


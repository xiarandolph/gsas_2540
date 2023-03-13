using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class ProjectileController : MonoBehaviour
{
    public float damage = 1.0f;
    static List<ProjectileController> projectiles = null;

    void Awake()
    {
        if (projectiles == null)
            projectiles = new List<ProjectileController>();
    }

    void Start()
    {
        projectiles.Add(this);
    }

    void OnDestroy()
    {
        projectiles.Remove(this);
    }

    public static void DestroyAll()
    {
        if (projectiles != null)
            foreach (ProjectileController proj in projectiles)
                Destroy(proj.gameObject);
    }
    
    // OnBecameInvisible: after projectile leaves screen, destroy 
    void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

    // OnCollisionEnter2D will apply the relevant damage to the entity it collides with
    void OnCollisionEnter2D(Collision2D col)
    {
        Entity entity = col.gameObject.GetComponent<Entity>();
        if (entity != null)
        {
            entity.Damage(damage);
            Destroy(this.gameObject);
        }
    }
}

}   // namespace Shmup


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

// Entity is a damageable entity
public abstract class Entity : MonoBehaviour
{
    // referenced game objects
    protected HealthBarController healthBar;

    // game variables
    [SerializeField] protected float maxHealth = 10.0f;
    protected float health;
    [SerializeField] protected float moveSpeed = 3.0f;

    protected virtual void Awake()
    {
        health = maxHealth;
        healthBar = transform.Find("Healthbar").GetComponent<HealthBarController>();
    }

    public virtual void Damage(float damage)
    {
        health = Mathf.Min(Mathf.Max(0, health - damage), maxHealth);
        healthBar.UpdateBar(health / maxHealth);

        if (health == 0)
            Die();
    }

    public abstract void Die();
}

} // namespace Shmup

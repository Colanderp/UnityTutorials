using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health;

    public UnityEvent onDamage;
    public UnityEvent onDeath;
    bool dead;
    bool invincible;
    
    public virtual void Start()
    {
        Heal();
    }
    public virtual void Update()
    {
        //Update
    }

    public void AddCallOnDamage(UnityAction call)
    {
        onDamage.AddListener(call);
    }
    public void AddCallOnDeath(UnityAction call)
    {
        onDeath.AddListener(call);
    }

    public bool isDead()
    {
        return dead;
    }

    public virtual void Heal()
    {
        invincible = (maxHealth <= 0);
        health = maxHealth;
        dead = false;
    }

    public virtual bool Damage(float dmg)
    {
        health = Mathf.Clamp(health - dmg, 0, maxHealth);
        onDamage.Invoke();
        if (invincible) return false;
        bool justDied = (!dead && health <= 0);
        if (justDied)
        {
            onDeath.Invoke();
            dead = true;
        }

        return justDied; 
    }
}

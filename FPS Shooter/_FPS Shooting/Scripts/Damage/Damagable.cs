using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damagable : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health;

    UnityEvent onDamage;

    public virtual void Start()
    {
        Heal();
        onDamage = new UnityEvent();
    }
    public virtual void Update()
    {
        //Update
    }

    public void AddCallOnDamage(UnityAction call)
    {
        onDamage.AddListener(call);
    }

    public virtual void Heal()
    {
        health = maxHealth;
    }

    public virtual bool Damage(float dmg)
    {
        health = Mathf.Clamp(health - dmg, 0, maxHealth);
        onDamage.Invoke();

        return (health <= 0);
    }
}

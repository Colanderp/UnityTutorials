using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDamagable : Damagable
{
    public float BackUpTime = 2f;
    Animator ani;
    bool dead;

    float hurt = 0f;
    float previousHealth;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        previousHealth = health;

        dead = false;
        ani = GetComponent<Animator>();
        AddCallOnDamage(CheckIfDead);
    }

    public override void Update()
    {
        hurt = Mathf.Lerp(hurt, (dead) ? 1f : 0f, Time.deltaTime * 4f);
        ani.SetFloat("hurt", hurt);
    }

    void CheckIfDead()
    {
        if (dead) return;
        if (health <= 0)
        {
            StartCoroutine(getBackUp());
            IEnumerator getBackUp()
            {
                hurt = 1f;
                dead = true;
                yield return new WaitForSeconds(BackUpTime);

                Heal();
                dead = false;
            }
        }
        else
        {
            float dmgDone = (previousHealth - health);
            hurt = dmgDone / (maxHealth * 2);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateLean : MonoBehaviour
{
    public float lerpSpeed = 0.125f;

    Vector2 lean;
    Vector2 actualLean;
    Animator ani;

    private void Start()
    {
        ani = GetComponent<Animator>();
    }

    void Update()
    {
        if (Mathf.Abs(lean.x - actualLean.x) > 0.02f)
            actualLean.x = Mathf.Lerp(actualLean.x, lean.x, lerpSpeed);
        if (Mathf.Abs(lean.y - actualLean.y) > 0.02f)
            actualLean.y = Mathf.Lerp(actualLean.y, lean.y, lerpSpeed);
        ani.SetFloat("x", actualLean.x);
        ani.SetFloat("y", actualLean.y);
    }

    public void SetLean(Vector2 set)
    {
        if (lean == set) return;
        lean = set;
    }
}

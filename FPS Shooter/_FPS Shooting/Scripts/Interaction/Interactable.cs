using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Range(1f, 10f)]
    public float interactRange = 3f;
    public string description;
    public UnityEvent onInteract;

    public virtual void Start()
    {

    }

    public virtual void Interact()
    {
        onInteract.Invoke();
    }
}

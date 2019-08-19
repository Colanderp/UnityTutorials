using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 input
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxis("Horizontal");
            i.y = Input.GetAxis("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public Vector2 down
    {
        get { return _down; }
    }

    public Vector2 raw
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxisRaw("Horizontal");
            i.y = Input.GetAxisRaw("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public bool run
    {
        get { return Input.GetKey(KeyCode.LeftShift); }
    }

    public bool crouch
    {
        get { return Input.GetKeyDown(KeyCode.C); }
    }

    public bool crouching
    {
        get { return Input.GetKey(KeyCode.C); }
    }

    private Vector2 previous;
    private Vector2 _down;

    private int jumpTimer;
    private bool jump;

    void Start()
    {
        jumpTimer = -1;
    }

    void Update()
    {
        _down = Vector2.zero;
        if (raw.x != previous.x)
        {
            previous.x = raw.x;
            if (previous.x != 0)
                _down.x = previous.x;
        }
        if (raw.y != previous.y)
        {
            previous.y = raw.y;
            if (previous.y != 0)
                _down.y = previous.y;
        }
    }

    public void FixedUpdate()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            jump = false;
            jumpTimer++;
        }
        else if (jumpTimer > 0)
            jump = true;
    }

    public bool Jump()
    {
        return jump;
    }

    public void ResetJump()
    {
        jumpTimer = -1;
    }
}

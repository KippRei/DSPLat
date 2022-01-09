using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PC : MonoBehaviour
{
    public Rigidbody2D playerChar;
    public float jumpSpeed = 5;
    public float runSpeed = 5;
    public float airSpeed = 2;
    private bool standingOnPlatform = false;
    private string playerMovement = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //CheckInput();
        switch (playerMovement)
        {
            case "Left": 
                playerChar.velocity = new Vector2(-runSpeed, playerChar.velocity.y);
                break;

            case "Right":
                playerChar.velocity = new Vector2(runSpeed, playerChar.velocity.y);
                break;
        }
    }

   /* void OnJump(InputValue value)
    {
        if (standingOnPlatform == true)
        {
            playerChar.AddForce(transform.up * jumpSpeed, ForceMode2D.Impulse);
        }
    }

    void OnLeft(InputValue value)
    {
        if (value.Get<float>() > .5)
        {
           // if (standingOnPlatform == true)
            {
                playerMovement = "Left";
            }
          *//*  else
            {
                StopAllCoroutines();
                playerChar.velocity = new Vector2(-airSpeed, playerChar.velocity.y);
            }*//*
        }
        else
        {
            playerMovement = "";
            StartCoroutine(ReleaseVelocity("Left"));
        }
    }

    void OnRight(InputValue value)
    {
        if (value.Get<float>() > .5)
        {
           // if (standingOnPlatform == true)
            {
                playerMovement = "Right";
            }
        }
        else
        {
            playerMovement = "";
            StartCoroutine(ReleaseVelocity("Right"));
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name == "floor")
        {
            standingOnPlatform = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "floor")
        {
            standingOnPlatform = false;
        }
    }

    // TODO: Make this better!! I feel like I'm instantiating way too many Vec2's!
    IEnumerator ReleaseVelocity(string dirMoving)
    {
        if (dirMoving == "Right")
        {
            while(playerChar.velocity.x > 0)
            {
                Vector2 velo = playerChar.velocity;
                playerChar.velocity = new Vector2(velo.x - .5f, velo.y);
                yield return new WaitForSeconds(.1f);
            }
            Vector2 vel = playerChar.velocity;
            playerChar.velocity = new Vector2(0f, 0f);
        }
        else if (dirMoving == "Left")
        {
            while (playerChar.velocity.x < 0)
            {
                Vector2 velo = playerChar.velocity;
                playerChar.velocity = new Vector2(velo.x + .5f, velo.y);
                yield return new WaitForSeconds(.1f);
            }
            Vector2 vel = playerChar.velocity;
            playerChar.velocity = new Vector2(0f, vel.y);
        }
    }*/
}

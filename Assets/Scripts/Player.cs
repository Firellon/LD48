using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D body;

    private float horizontal;
    private float vertical;

    public float runSpeed = 20.0f;

    private void Start ()
    {
        body = GetComponent<Rigidbody2D>(); 
    }

    private void Update ()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical"); 
    }

    private void FixedUpdate()
    {
        body.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }

    private void OnCollisionEnter2D(Collision2D hit){
        if(hit.gameObject.CompareTag("Wall"))
        {
            body.velocity = Vector2.zero;
        }
    } 
}

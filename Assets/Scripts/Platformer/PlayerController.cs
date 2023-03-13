using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{

public class PlayerController : MonoBehaviour
{
    // referenced game objects / components
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Animator animator;
    Collider2D _collider;

    // game parameters
    [SerializeField] float movementSpeed = 3.0f;
    [SerializeField] float jumpForce = 5.0f;

    // game logic variables
    bool grounded = true;
    bool hugging = false;   // hugging a wall

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        // using force to allow for redirection midair
        // force only apply if grounded,
        // in same direction as current direction (linear drag causes player to stop otherwise),
        // or after peak of a jump (player already falling (so wall jump isn't disturbed)
        if (grounded || horizontal * rb.velocity.x >= 0 || rb.velocity.y <= 0)
            rb.AddForce(Vector2.right * horizontal, ForceMode2D.Impulse);

        // make sure the player is limited in max horizontal speed
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -movementSpeed, movementSpeed), rb.velocity.y);

        // check if grounded using raycast
        int layerMask = LayerMask.GetMask("Border", "Platform");   // to not hit players, enitities, etc
        Vector2 contact = new Vector2(rb.position.x, _collider.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(contact, -Vector2.up, 0.01f, layerMask);

        grounded = hit.collider != null;

        // check if hugging a wall for wall jump
        contact = new Vector2(_collider.bounds.min.x, rb.position.y);
        Vector2 dir = Vector2.left;
        if (horizontal > 0)
        {
            contact.x = _collider.bounds.max.x;
            dir = Vector2.right;
        }
        hit = Physics2D.Raycast(contact, dir, 0.01f, layerMask);

        hugging = (hit.collider != null) && horizontal != 0;

        // jump
        if (Input.GetAxis("Jump") > 0 && grounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            grounded = false;
            animator.SetTrigger("jump");
        }

        // wall jump
        if (Input.GetAxis("Jump") > 0 && hugging && !grounded && rb.velocity.y < 0)
        {
            rb.AddForce((1.5f * Vector2.up - dir).normalized * 1.3f * jumpForce, ForceMode2D.Impulse);
            hugging = false;
            animator.SetTrigger("jump");
        }

        // update animator
        animator.SetFloat("xVel", Mathf.Abs(horizontal));
        animator.SetFloat("yVel", rb.velocity.y);
        animator.SetBool("grounded", grounded);
        animator.SetBool("hugging", hugging);

        if (rb.velocity.x != 0) // don't flip direction if standing still
            sprite.flipX = rb.velocity.x < 0;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        // fall through platform
        if (col.gameObject.layer == LayerMask.NameToLayer("Platform") && Input.GetAxis("Vertical") < 0)
        {
            col.collider.enabled = false;
            StartCoroutine(DelayedEnable(col.collider));
        }
    }

    IEnumerator DelayedEnable(Collider2D col)
    {
        yield return new WaitForSeconds(0.2f);
        col.enabled = true;
    }

}

}   // namespace Platformer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController : MonoBehaviour
{
    [SerializeField] float speed = 9f;
    [SerializeField] float walkAcceleration = 75f;
    [SerializeField] float airAcceleration = 30f;
    [SerializeField] float groundDeceleration = 70f;

    [SerializeField] float jumpHeight = 4f;
    [SerializeField] float airGravity = 1f;
    [SerializeField] float fallSpeed = 1f;

    [SerializeField] float jumpBufferTime = 1f;
    private bool jumpBuffered = false;
    private float bufferedTime;

    private BoxCollider2D boxCollider;
    public Animator sheepAnim;
    public SpriteRenderer sheepSprite;

    private bool isWalking = false,
                 isGrounded = false,
                 isFlipped = false,
                 isJumping = false;

    private Vector2 velocity;
    private Vector2 defaultPosition;

    private void Awake() {      
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Start() {
        defaultPosition = gameObject.transform.position;
    }

    void Update() {
        // reset position if sheep is offscreen
        if(!sheepSprite.isVisible) {
            transform.position = defaultPosition;
        }
        // make movement framerate-independent
        transform.Translate(velocity * Time.deltaTime);


        // solve overlaps
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        foreach (Collider2D hit in hits) {
	        if (hit == boxCollider)
	            continue;

	        ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

	        if (colliderDistance.isOverlapped) {
		        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
	        }
        }


        // handle velocity
        float moveInput = Input.GetAxisRaw("Horizontal");
        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float deceleration = isGrounded ? groundDeceleration : 0;

        if (moveInput != 0) {
	        velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
            isWalking = true;
        } else {
	        velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            isWalking = false;
        }
        
        if (moveInput > 0) {
            isFlipped = false;
        } else if (moveInput < 0) {
            isFlipped = true;
        }

        if (isGrounded) {
	        velocity.y = 0;
        } else if (velocity.y > 0 & Input.GetButton("Jump")) {
            velocity.y += airGravity * Physics2D.gravity.y * Time.deltaTime;
        } else {
            velocity.y += airGravity * fallSpeed * Physics2D.gravity.y * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) Jump(false);


        // configure the animations
        sheepAnim.SetBool("isWalking", isWalking);
        sheepAnim.SetBool("isGrounded", isGrounded);
        sheepAnim.SetBool("isJumping", isJumping);
        sheepSprite.flipX = isFlipped;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ground")) {
            isGrounded = true;
            isJumping = false;
            if (jumpBuffered & (Time.time - bufferedTime < jumpBufferTime)) Jump(true);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Ground")) {
            isGrounded = false;
        }
    }

    void Jump(bool wasBuffered) {
        if (isGrounded) {
            sheepAnim.SetTrigger("jump");
		    velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
            isGrounded = jumpBuffered = false;
            isJumping = true;
	    } else if (!wasBuffered) {
            jumpBuffered = true;
            bufferedTime = Time.time;
        }
    }
}
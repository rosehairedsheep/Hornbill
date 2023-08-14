using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController : MonoBehaviour
{

    [SerializeField] float speed = 9;
    [SerializeField] float walkAcceleration = 75;
    [SerializeField] float airAcceleration = 30;
    [SerializeField] float groundDeceleration = 70;
    [SerializeField] float jumpHeight = 4;

    private BoxCollider2D boxCollider;

    public Animator sheepAnim;
    public SpriteRenderer sheepSprite;

    private bool isWalking = false,
                 isGrounded = false,
                 isFlipped = false;

    private Vector2 velocity;

    private void Awake()
    {      
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // start is called before the first frame update
    void Start()
    {
        
    }

    // update is called once per frame
    void Update()
    {
        // make movement framerate-independent
        transform.Translate(velocity * Time.deltaTime);

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
	        if (Input.GetButtonDown("Jump")) {
		        velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
	        }
        }

        velocity.y += Physics2D.gravity.y * Time.deltaTime;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        isGrounded = false;

        foreach (Collider2D hit in hits) {
	        if (hit == boxCollider)
	            continue;

	        ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

	        if (colliderDistance.isOverlapped) {
		        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0) {
	                isGrounded = true;
                }
	        }
        }

        sheepAnim.SetBool("isWalking", isWalking);
        sheepAnim.SetBool("isGrounded", isGrounded);
        sheepSprite.flipX = isFlipped;
    }
}
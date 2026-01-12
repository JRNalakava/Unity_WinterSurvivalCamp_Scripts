using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick; // Drag your UI Joystick here later
    private Rigidbody rb;
    private Animator animator;

    // Physics Polish State
    private bool isAttacking = false;
    private float defaultMass;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) defaultMass = rb.mass;

        // If you don't have animations yet, this line might error. 
        // Comment it out if you haven't set up the Animator.
        animator = GetComponentInChildren<Animator>(); 
    }

    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
        if (rb != null)
        {
            // "Mass Spike" to prevent pushing
            rb.mass = attacking ? 2000f : defaultMass;
            
            // Stop sliding instantly
            if (attacking) rb.velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        // 1. Input Lock during attack
        if (isAttacking) 
        {
             // Force stop velocity to prevent sliding
             rb.velocity = Vector3.zero;
             // Keep animation as "Idle" (or whatever attack implies)
             if(animator != null) animator.SetBool("IsMoving", false);
             return; 
        }

        // Get Input (Keyboard OR Joystick)
        float moveX = Input.GetAxis("Horizontal") + (joystick != null ? joystick.Horizontal : 0);
        float moveZ = Input.GetAxis("Vertical") + (joystick != null ? joystick.Vertical : 0);

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;

        // Move
        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.z * moveSpeed);

        // ... (Your existing movement code) ...

        // Rotate to face direction
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
            
            // --- NEW: TELL ANIMATOR TO RUN ---
            if(animator != null) 
            {
                animator.SetBool("IsMoving", true); // Synty usually uses "IsMoving" or "Speed"
                // If "IsMoving" doesn't work, try animator.SetFloat("Speed", movement.magnitude);
            }
        }
        else
        {
            // --- NEW: TELL ANIMATOR TO STOP ---
            if(animator != null) animator.SetBool("IsMoving", false);
        }
    }
}
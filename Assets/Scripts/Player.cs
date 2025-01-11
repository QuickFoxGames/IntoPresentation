using UnityEngine;
[RequireComponent (typeof(Rigidbody))] // if the coresponding gameobject does NOT already have a Rigidbody component, one is added
public class Player : MonoBehaviour
{
    [SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_crouchSpeed;
    [SerializeField] private float m_runSpeed;
    [SerializeField] private float m_moveAcceleration;
    [SerializeField] private float m_jumpHeight;
    [SerializeField] private float m_mouseSensitivity;
    [SerializeField] private float m_camUpperBounds;
    [SerializeField] private float m_camLowerBounds;
    [SerializeField] private Transform m_camTransform;
    [SerializeField] private float m_groundCheckDistance;
    [SerializeField] private float m_groundCheckRadius;
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private PhysicsMaterial m_physicsMaterial;
    [SerializeField] private Gun m_gun;

    private bool m_isGrounded = true;
    private float m_xRot, m_yRot;
    private float m_currentSpeed;
    private Vector3 m_groundNormal;

    // inputs //
    private bool m_jumpInput;
    private bool m_sprintInput;
    private bool m_crouchInput;
    private bool m_shootInput;
    private float m_verticalInput;
    private float m_horizontalInput;
    private float m_mouseX;
    private float m_mouseY;
    // inputs //

    private Rigidbody m_rb;
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        // inputs // Reads and assigns user input to each predefined input variable
        m_jumpInput = Input.GetKey(KeyCode.Space);
        m_sprintInput = Input.GetKey(KeyCode.LeftShift);
        m_crouchInput = Input.GetKey(KeyCode.LeftControl);
        m_shootInput = Input.GetKey(KeyCode.Mouse0);
        m_verticalInput = Input.GetAxisRaw("Vertical");
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_mouseX = Input.GetAxisRaw("Mouse X");
        m_mouseY = Input.GetAxisRaw("Mouse Y");
        // input //
        if (m_shootInput) m_gun.Shoot();

        UpdateCamera();

        if (m_sprintInput) m_currentSpeed = m_runSpeed;
        else if (m_crouchInput) m_currentSpeed = m_crouchSpeed;
        else m_currentSpeed = m_walkSpeed;
        m_currentSpeed = m_sprintInput ? m_runSpeed : m_crouchInput ? m_crouchSpeed : m_walkSpeed;

        CheckGround();
    }
    private void FixedUpdate()
    {
        Movement();
        if (m_jumpInput && m_isGrounded) Jump(); // runs the jump function if the player is pressing the jump key and the player is on the ground
    }
    /// <summary>
    /// Calculates and applies movement forces to the player.
    /// Utilizes moveForce, and a frictionForce
    /// </summary>
    private void Movement()
    {
        // The user input direction is projected onto the current ground plane then normalized and multipled by the current speed
        Vector3 finalVelocity = m_currentSpeed * Vector3.ProjectOnPlane(m_verticalInput * transform.forward + m_horizontalInput * transform.right, m_groundNormal).normalized;

        // Calculates the player velocity purpendicular to the ground plane
        Vector3 flatVelocity = Vector3.ProjectOnPlane(m_rb.linearVelocity, m_groundNormal);
        // calculates the moveForce required to move the player to the desired speed
        Vector3 moveForce = m_rb.mass * m_moveAcceleration * (finalVelocity - flatVelocity);
        Vector3 frictionForce = Vector3.zero;
        if (m_isGrounded)
        { // If the player is on the ground calculate the friction force based on the friction coefficient of the interacting services
            frictionForce = m_physicsMaterial.dynamicFriction * m_rb.mass * Physics.gravity.magnitude * moveForce.normalized;
        }
        m_rb.AddForce(moveForce + frictionForce);
    }
    /// <summary>
    /// Calculates the jump velocity of the player required to reach the specified jump height
    /// </summary>
    private void Jump()
    {
        Vector3 jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * m_jumpHeight) * Vector3.up;

        // sets the vertical component of the players velocity to zero
        m_rb.linearVelocity = new Vector3(m_rb.linearVelocity.x, 0f, m_rb.linearVelocity.z);

        // adds the calculated force to the player
        m_rb.AddForce(m_rb.mass * jumpVelocity, ForceMode.Impulse);
    }
    /// <summary>
    /// Checks if the player is on the ground.
    /// Casts a ray starting from slightly above the base of the player, down to the nearest ground plane.
    /// Saves the normal vector of the ground plane into m_groundNormal
    /// </summary>
    private void CheckGround()
    {
        // The origin is set as 1x the ground distance above the player poistion
        Vector3 origin = new(transform.position.x, transform.position.y + m_groundCheckDistance, transform.position.z);

        // We run a SphereCastAll and store the hit data in an array (hits)
        RaycastHit[] hits = Physics.SphereCastAll(origin, m_groundCheckRadius, Vector3.down, m_groundCheckDistance, m_groundMask);

        if (hits.Length > 1) // if there is more than one hit we loop through all hit normals and calculate the average
        {
            m_isGrounded = true;
            Vector3 temp = Vector3.zero;
            for (int i = 0; i < hits.Length; i++)
            {
                temp += hits[i].normal;
            }
            m_groundNormal = temp / hits.Length;
        }
        else if (hits.Length == 1) // if there is only one hit then we use that single hit normal as our ground normal
        {
            m_isGrounded = true;
            m_groundNormal = hits[0].normal;
        }
        else m_isGrounded = false;

        // The ray is cast from the origin, down along Vector3.down, with the max distance of the ray being set to 2x the ground distance
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f * m_groundCheckDistance, m_groundMask))
        {
            m_isGrounded = true;
            m_groundNormal = hit.normal;
        }
        else m_isGrounded = false;
    }
    /// <summary>
    /// Updates the camera rotation based on the culmulative x and y positions of the mouse
    /// </summary>
    private void UpdateCamera()
    {
        m_xRot -= m_mouseY * m_mouseSensitivity * Time.fixedDeltaTime; // fixedDeltaTime is used to ensure frame rate independant mouse control
        m_yRot += m_mouseX * m_mouseSensitivity * Time.fixedDeltaTime;

        if (m_xRot > m_camUpperBounds) m_xRot = m_camUpperBounds;
        if (m_xRot < m_camLowerBounds) m_xRot = m_camLowerBounds;

        m_camTransform.localRotation = Quaternion.Euler(m_xRot, 0f, 0f); // applys the x rotation to the local axis of the camera
        transform.rotation = Quaternion.Euler(0f, m_yRot, 0f); // applys the y rotation to the entire players global axis
    }

    // Displays debug information on the screen
    void OnGUI()
    {
        GUIStyle debugStyle = new(GUI.skin.label)
        {
            fontSize = 20,
            padding = new RectOffset(15, 15, 10, 10)
        };
        string debugMessage = $"vertin: {m_verticalInput:F2}, horzin: {m_horizontalInput:F2}, jump: {m_jumpInput}, sprint: {m_sprintInput}, crouch: {m_crouchInput}\nfullVelocity: {m_rb.linearVelocity.magnitude:F1}";
        GUILayout.Label(debugMessage, debugStyle);
    }


    // What we covered in class //

    /*[SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_runSpeed;
    [SerializeField] private float m_crouchSpeed;
    [SerializeField] private float m_acceleration;
    [SerializeField] private float m_jumpHeight;

    [Header("Inputs")]
    [SerializeField] private float m_verticalInput;
    [SerializeField] private float m_horizontalInput;
    [SerializeField] private float m_mouseX;
    [SerializeField] private float m_mouseY;
    [SerializeField] private bool m_jumpInput;
    [SerializeField] private bool m_sprintInput;
    [SerializeField] private bool m_crouchInput;
    [SerializeField] private bool m_shootInput;

    private Rigidbody m_rb;
    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        // inputs //
        m_verticalInput = Input.GetAxisRaw("Vertical");
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_mouseX = Input.GetAxisRaw("Mouse X");
        m_mouseY = Input.GetAxisRaw("Mouse Y");
        m_jumpInput = Input.GetKey(KeyCode.Space);
        m_sprintInput = Input.GetKey(KeyCode.LeftShift);
        m_crouchInput = Input.GetKey(KeyCode.LeftControl);
        m_shootInput = Input.GetKey(KeyCode.Mouse0);

        CheckGround();
    }
    private void FixedUpdate()
    {
        Movement();
        if (m_jumpInput && isGrounded) Jump();
    }
    private void Movement()
    {
        Vector3 finalVelocity = m_walkSpeed * (m_verticalInput * transform.forward + m_horizontalInput * transform.right).normalized;
        Vector3 currentVelocity = m_rb.linearVelocity;
        Vector3 moveForce = m_rb.mass * m_acceleration * (finalVelocity - currentVelocity);

        m_rb.AddForce(moveForce);
    }
    private void Jump()
    {
        Vector3 jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * 20f) * Vector3.up;
        m_rb.linearVelocity = new(m_rb.linearVelocity.x, 0f, m_rb.linearVelocity.z);
        m_rb.AddForce(m_rb.mass * jumpVelocity, ForceMode.Impulse);
    }
    [SerializeField] private LayerMask groundLayers;
    private bool isGrounded = true;
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 0.125f, transform.position.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 0.125f, groundLayers))
        {
            isGrounded = true;
        }else isGrounded = false;
    }*/
}
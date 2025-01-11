using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float m_maxHp;
    [SerializeField] private float m_currentSpeed;
    [SerializeField] private float m_moveAcceleration;
    [SerializeField] private float m_attackDistance;
    [SerializeField] private float m_attackRate;
    [SerializeField] private PhysicsMaterial m_physicsMaterial;
    [SerializeField] private float m_groundCheckDistance;
    [SerializeField] private LayerMask m_groundMask;
    private bool m_canAttack = true;
    private float m_currentHp;
    private Vector3 m_directionToPlayer;
    private Vector3 m_groundNormal;
    private Transform m_playerTransform;
    private Rigidbody m_rb;
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_playerTransform = FindFirstObjectByType<Player>().transform; // costly function should only be used in Start() or Awake()
        m_currentHp = m_maxHp;
    }
    void Update()
    {
        CheckGround();

        m_directionToPlayer = m_playerTransform.position - transform.position;

        if (m_directionToPlayer.magnitude <= m_attackDistance && m_canAttack) Attack();
    }
    private void FixedUpdate()
    {
        Movement();
    }
    private void CheckGround() // simplified ground check
    {
        // The origin is set as 1x the ground distance above the player poistion
        Vector3 origin = new(transform.position.x, transform.position.y + m_groundCheckDistance, transform.position.z);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, m_groundCheckDistance, m_groundMask)) m_groundNormal = hit.normal;
        else m_groundNormal = Vector3.up;
    }
    /// <summary>
    /// Calculates and applies movement forces to the player.
    /// Utilizes moveForce, and a frictionForce
    /// </summary>
    private void Movement()
    {
        Vector3 moveDirection = m_currentSpeed * Vector3.ProjectOnPlane(m_directionToPlayer.normalized, m_groundNormal);
        Vector3 moveForce = m_rb.mass * m_moveAcceleration * (moveDirection - m_rb.linearVelocity); 

        // Since the enemy can NOT jump, we can assume the enemy will always be on the ground
        Vector3 frictionForce = m_physicsMaterial.dynamicFriction * m_rb.mass * Physics.gravity.magnitude * moveForce.normalized;
        m_rb.AddForce(moveForce + frictionForce);
    }
    private void Attack()
    {
        Debug.Log("Attacked Player");
        StartCoroutine(DelayAttack());
    }
    private IEnumerator DelayAttack()
    {
        m_canAttack = false;
        yield return new WaitForSeconds(1 / (m_attackRate / 60));
        m_canAttack = true;
    }
    private void OnTriggerEnter(Collider other)
    { // When an object with a trigger collides with the attached enemy colliders m_currentHp is decremented by 10f
        Debug.Log("Enemy got hit");
        m_currentHp -= 10f;
        if (m_currentHp <= 0f) gameObject.SetActive(false); // When the value of m_currentHp is less than or equal to zero we disable the Enemy object
    }
}
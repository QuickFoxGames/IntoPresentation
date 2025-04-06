using UnityEngine;
using ViperPhysics;

public class VPlayer : MonoBehaviour
{
    [SerializeField] private Transform t;
    [SerializeField] private Transform t2;
    [SerializeField] private Transform ground;
    private RigidBody rb;
    private RigidBody rb2;
    private RigidBody groundBody;
    private RigidBody[] bodies;
    void Start()
    {
        rb = new RigidBody()
        {
            m_useGravity = true,
            m_isKinematic = false,
            m_mass = 3.0f,
            m_oldPosition = Vector3.zero,
            m_linearVelocity = Vector3.zero,
            m_oldLinearVelocity = Vector3.zero,
            m_linearAcceleration = Vector3.zero,
            m_OldLinearAcceleration = Vector3.zero,
            m_linearJerk = Vector3.zero,
            m_transform = t,
            m_collider = new ViperPhysics.BoxCollider ()
            {
                m_height = 1.0f,
                m_length = 1.0f,
                m_width = 1.0f,
            },
            collidedThisFrame = new()
        };
        rb2 = new RigidBody()
        {
            m_useGravity = true,
            m_isKinematic = false,
            m_mass = 3.0f,
            m_oldPosition = Vector3.zero,
            m_linearVelocity = Vector3.zero,
            m_oldLinearVelocity = Vector3.zero,
            m_linearAcceleration = Vector3.zero,
            m_OldLinearAcceleration = Vector3.zero,
            m_linearJerk = Vector3.zero,
            m_transform = t2,
            m_collider = new ViperPhysics.BoxCollider()
            {
                m_height = 1.0f,
                m_length = 1.0f,
                m_width = 1.0f,
            },
            collidedThisFrame = new()
        };
        groundBody = new RigidBody()
        {
            m_useGravity = false,
            m_isKinematic = true,
            m_mass = 0.0f,
            m_oldPosition = Vector3.zero,
            m_linearVelocity = Vector3.zero,
            m_oldLinearVelocity = Vector3.zero,
            m_linearAcceleration = Vector3.zero,
            m_OldLinearAcceleration = Vector3.zero,
            m_linearJerk = Vector3.zero,
            m_transform = ground,
            m_collider = new ViperPhysics.PlaneCollider()
            {
                m_length = 10.0f,
                m_width = 10.0f,
                m_normal = ground.transform.up,
            },
            collidedThisFrame = new()
        };
        bodies = new[] {rb, rb2, groundBody};
    }
    void Update()
    {
        VPhysx.RunUpdate(bodies);
        if (Input.GetKey(KeyCode.Mouse0)) VPhysx.AddForce(rb, Vector3.up * 0.1f, VPhysx.ForceType.Continuous);
    }
    private void FixedUpdate()
    {
        VPhysx.RunFixedUpdate(bodies);
    }
}
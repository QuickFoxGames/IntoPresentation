using System.Collections.Generic;
using UnityEngine;
namespace ViperPhysics {
    public static class VPhysx
    {
        public enum ForceType { Continuous, Instant }
        public static Vector3 Gravity = 9.81f * Vector3.down;
        public static void RunUpdate(RigidBody[] bodies)
        {
            UpdateCollisions(bodies);
        }
        public static void RunFixedUpdate(RigidBody[] bodies)
        {
            foreach (var body in bodies)
            {
                UpdateRigidBody(body);
            }
        }
        private static void UpdateRigidBody(RigidBody rb)
        {
            if (rb.m_useGravity) AddForce(rb, Gravity, ForceType.Continuous);
            CalculateForces(rb);
            ApplyForces(rb);
        }
        public static void AddForce(RigidBody rb, Vector3 force, ForceType forceType) // f = ma => a = f/m
        {
            Vector3 a = force / rb.m_mass;
            switch (forceType)
            {
                case ForceType.Continuous:
                    rb.m_linearAcceleration += a * Time.fixedDeltaTime;
                    rb.m_linearVelocity += rb.m_linearAcceleration;
                    rb.m_transform.position += rb.m_linearVelocity;
                    break;

                case ForceType.Instant:
                    rb.m_linearAcceleration += a;
                    rb.m_linearVelocity += rb.m_linearAcceleration;
                    rb.m_transform.position += rb.m_linearVelocity;
                    break;
            }
        }
        public static void HandleCollision(RigidBody rb1, RigidBody rb2)
        {
            // v1 = ((m1 - m2)v1 + 2m2v2) / (m1 + m2)
            float totalMass = rb1.m_mass + rb2.m_mass;
            Vector3 v1 = rb1.m_isKinematic ? Vector3.zero : ((rb1.m_mass - rb2.m_mass) * rb1.m_linearVelocity + 2f * rb2.m_mass * rb2.m_linearVelocity) / totalMass;
            Vector3 v2 = rb2.m_isKinematic ? Vector3.zero : ((rb2.m_mass - rb1.m_mass) * rb2.m_linearVelocity + 2f * rb1.m_mass * rb1.m_linearVelocity) / totalMass;

            if (!rb1.m_isKinematic)
            {
                rb1.m_linearVelocity += v1;
                rb1.m_transform.position += rb1.m_linearVelocity;
            }
            if (!rb2.m_isKinematic)
            {
                rb2.m_linearVelocity += v2;
                rb2.m_transform.position += rb2.m_linearVelocity;
            }

            /*bool isBodyAStatic = rb1.m_collider.ColliderType == ColliderType.Plane;
            bool isBodyBStatic = rb2.m_collider.ColliderType == ColliderType.Plane;


            if (rb1.m_collider.CheckCollision(rb1, rb2, out Vector3 collisionNormal, out float penetrationDepth))
            {
                if (isBodyAStatic)
                {
                    // Only move the dynamic body (bodyB) upwards
                    rb2.m_transform.position += collisionNormal * penetrationDepth;
                }
                else if (isBodyBStatic)
                {
                    // Only move the dynamic body (bodyA) upwards
                    rb1.m_transform.position -= collisionNormal * penetrationDepth;
                }
                else
                {
                    // Both objects are dynamic, split the correction
                    rb1.m_transform.position -= collisionNormal * (penetrationDepth * 0.5f);
                    rb2.m_transform.position += collisionNormal * (penetrationDepth * 0.5f);
                }
            }*/
            rb1.collidedThisFrame.Add(rb2);
            rb2.collidedThisFrame.Add(rb1);

            Debug.Log($"HandleCollision ran on {rb1.m_transform.name} and {rb2.m_transform.name}");
        }
        #region Collider Handling
        public static void UpdateCollisions(RigidBody[] bodies)
        {
            int bodyCount = bodies.Length;

            for (int i = 0; i < bodyCount; i++)
            {
                for (int j = i + 1; j < bodyCount; j++)
                {
                    if (bodies[i].collidedThisFrame.Contains(bodies[j]) || bodies[j].collidedThisFrame.Contains(bodies[i]))
                        continue;  // Skip already processed collision

                    if (bodies[i].m_collider != null && bodies[j].m_collider != null)
                    {
                        if (bodies[i].m_collider.CheckCollision(bodies[i], bodies[j], out Vector3 collisionNormal, out float penetrationDepth))
                        {
                            // Separate objects based on collision resolution
                            bodies[i].m_transform.position -= collisionNormal * (penetrationDepth * 0.5f);
                            bodies[j].m_transform.position += collisionNormal * (penetrationDepth * 0.5f);

                            HandleCollision(bodies[i], bodies[j]);
                        }
                    }
                }
            }

            // Clear collision records for the next frame
            foreach (var body in bodies)
            {
                body.collidedThisFrame.Clear();
            }
        }
        #endregion
        #region Base
        private static void ApplyForces(RigidBody rb)
        {
            rb.m_linearJerk += rb.m_linearSnap;
            rb.m_linearAcceleration += rb.m_linearJerk;
            rb.m_linearVelocity += rb.m_linearAcceleration;
            rb.m_transform.position += rb.m_linearVelocity;
        }
        private static void CalculateForces(RigidBody rb, bool velocity = true, bool acceleration = true, bool jerk = true, bool snap = true)
        {
            if (velocity) CalculateVelocity(rb);
            if (acceleration) CalculateAcceleration(rb);
            if (jerk) CalculateJerk(rb);
            if (snap) CalculateSnap(rb);
        }
        private static void CalculateVelocity(RigidBody rb)
        {
            rb.m_linearVelocity = (rb.m_transform.position - rb.m_oldPosition) / Time.fixedDeltaTime;
        }
        private static void CalculateAcceleration(RigidBody rb)
        {
            rb.m_linearAcceleration = (rb.m_linearVelocity - rb.m_oldLinearVelocity) / Time.fixedDeltaTime;
        }
        private static void CalculateJerk(RigidBody rb)
        {
            rb.m_linearJerk = (rb.m_linearAcceleration - rb.m_OldLinearAcceleration) / Time.fixedDeltaTime;
        }
        private static void CalculateSnap(RigidBody rb)
        {
            rb.m_linearSnap = (rb.m_linearJerk - rb.m_oldLinearJerk) / Time.fixedDeltaTime;
        }
        #endregion
    }
    public struct RigidBody
    {
        public bool m_useGravity;
        public bool m_isKinematic;

        public float m_mass;

        public Vector3 m_oldPosition;           // p += v

        public Vector3 m_linearVelocity;        // v = d/t
        public Vector3 m_oldLinearVelocity;

        public Vector3 m_linearAcceleration;    // a = v/t
        public Vector3 m_OldLinearAcceleration;

        public Vector3 m_linearJerk;            // j = a/t
        public Vector3 m_oldLinearJerk;

        public Vector3 m_linearSnap;            // s = j/t

        public Transform m_transform;

        public ICollider m_collider;

        public List<RigidBody> collidedThisFrame;
    }
    #region Colliders
    public enum ColliderType
    {
        None,
        Sphere,
        Box,
        Plane
    }
    public interface ICollider
    {
        ColliderType ColliderType { get; }
        bool CheckCollision(RigidBody a, RigidBody b, out Vector3 collisionNormal, out float penetrationDepth);
    }
    public struct SphereCollider : ICollider
    {
        public float m_radius;
        public ColliderType ColliderType => ColliderType.Sphere;

        public readonly bool CheckCollision(RigidBody a, RigidBody b, out Vector3 collisionNormal, out float penetrationDepth)
        {
            penetrationDepth = 0;
            collisionNormal = Vector3.zero;

            if (b.m_collider is SphereCollider sphereB)
            {
                float combinedRadius = m_radius + sphereB.m_radius;
                float distance = Vector3.Distance(a.m_transform.position, b.m_transform.position);

                if (distance <= combinedRadius)
                {
                    collisionNormal = (b.m_transform.position - a.m_transform.position).normalized;
                    penetrationDepth = combinedRadius - distance;
                    return true;
                }
            }
            return false;
        }
    }
    public struct BoxCollider : ICollider
    {
        public float m_length;
        public float m_width;
        public float m_height;
        public ColliderType ColliderType => ColliderType.Box;

        public readonly Vector3 GetMin(Vector3 position)
        {
            return position - new Vector3(m_length / 2, m_width / 2, m_height / 2);
        }

        public readonly Vector3 GetMax(Vector3 position)
        {
            return position + new Vector3(m_length / 2, m_width / 2, m_height / 2);
        }

        public readonly bool CheckCollision(RigidBody a, RigidBody b, out Vector3 collisionNormal, out float penetrationDepth)
        {
            penetrationDepth = 0;
            collisionNormal = Vector3.zero;

            if (b.m_collider is BoxCollider boxB)
            {
                Vector3 minA = GetMin(a.m_transform.position);
                Vector3 maxA = GetMax(a.m_transform.position);

                Vector3 minB = boxB.GetMin(b.m_transform.position);
                Vector3 maxB = boxB.GetMax(b.m_transform.position);

                if (minA.x <= maxB.x && maxA.x >= minB.x &&
                    minA.y <= maxB.y && maxA.y >= minB.y &&
                    minA.z <= maxB.z && maxA.z >= minB.z)
                {
                    collisionNormal = (b.m_transform.position - a.m_transform.position).normalized;
                    penetrationDepth = 1.0f; // Placeholder value
                    return true;
                }
            }
            return false;
        }
    }
    public struct PlaneCollider : ICollider
    {
        public float m_length;
        public float m_width;
        public Vector3 m_normal;
        public ColliderType ColliderType => ColliderType.Plane;

        public readonly bool CheckCollision(RigidBody a, RigidBody b, out Vector3 collisionNormal, out float penetrationDepth)
        {
            penetrationDepth = 0;
            collisionNormal = m_normal;

            float distance = Vector3.Dot(a.m_transform.position, m_normal);
            float halfHeight = a.m_collider is SphereCollider sphere ? sphere.m_radius : 0;

            if (distance <= halfHeight)
            {
                penetrationDepth = halfHeight - distance;
                return true;
            }
            return false;
        }
    }
    #endregion
}
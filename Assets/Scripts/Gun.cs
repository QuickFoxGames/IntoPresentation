using System.Collections;
using UnityEngine;
public class Gun : MonoBehaviour
{
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private float m_fireRate;
    [SerializeField] private Rigidbody m_bulletPrefab;
    [SerializeField] private Transform m_bulletSpawn;

    private bool m_canShoot = true;
    private float m_timeBetwwenShot;
    private void Awake()
    {
        m_timeBetwwenShot = 1f / (m_fireRate / 60f); // converts the rounds per min value of m_fireRate into a delay between shots in seconds
        // m_firerate => rounds fired per min
        // m_fireRate / 60f => rounds fired per second
        // 1f / (m_firerate / 60f) => seconds between each shot
    }
    /// <summary>
    /// Handles shooting the gun.
    /// Spawns a bullet, applies a bulletForce to move the bullet towards the target.
    /// Runs the ShotDelay coroutine
    /// </summary>
    public void Shoot()
    {
        if (m_canShoot)
        {
            Rigidbody b = Instantiate(m_bulletPrefab, m_bulletSpawn.position, m_bulletSpawn.rotation);
            Destroy(b.gameObject, 3f);
            Vector3 bulletforce = b.mass * m_bulletSpeed * m_bulletSpawn.forward;
            b.AddForce(bulletforce, ForceMode.Impulse);
            StartCoroutine(ShotDelay());
        }
    }
    /// <summary>
    /// Sets the gun's m_canShoot variable to false then waits the specified time and sets the variable to true
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShotDelay()
    {
        m_canShoot = false;
        yield return new WaitForSeconds(m_timeBetwwenShot);
        m_canShoot = true;
    }
}
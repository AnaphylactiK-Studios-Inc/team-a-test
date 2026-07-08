using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Firing")]
    public bool isFiring;
    public float fireRate = 10f; // shots per second

    [Header("Damage")]
    [SerializeField] float shieldDamage = 10f;
    [SerializeField] float bodyDamage = 10f;

    [Header("Raycast")]
    [SerializeField] float range = 100f;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] Camera _cam;

    float _nextFireTime;

    void Update()
    {
        if (isFiring && Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Fire()
    {
        Camera cam = _cam != null ? _cam : Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitLayers))
            return;

        if (hit.collider.TryGetComponent(out EnemyShield shield))
        {
            shield.TakeDamage(shieldDamage);
            return;
        }

        if (hit.collider.TryGetComponent(out EnemyBody body))
            body.TakeDamage(bodyDamage);
    }
}

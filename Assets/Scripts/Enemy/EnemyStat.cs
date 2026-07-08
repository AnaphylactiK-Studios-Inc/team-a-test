using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStat : MonoBehaviour
{
    public static event Action<EnemyStat> OnAnyEnemyDied;
    public static event Action<EnemyStat> OnAnyEnemySpawned;


    [Header("Health")]
    public float health = 50f;
    public float maxHealth = 50f;


    [Header("Death Sequence")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject bloodSplat;
    [SerializeField] private string killTriggerName = "Kill";
    [SerializeField, Min(0f)] private float killAnimationHoldDuration = 0.35f;
    [SerializeField, Min(0f)] private float knockUpHeight = 0.8f;
    [SerializeField, Min(0f)] private float knockBackDistance = 1.2f;
    [SerializeField, Min(0.01f)] private float knockUpDuration = 0.18f;
    [SerializeField, Min(0.01f)] private float sinkDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float sinkDistance = 3.5f;
    [SerializeField, Min(0f)] private float destroyDelayAfterSink = 0f;

    [Header("Death VFX")]
    [SerializeField] private GameObject deathImpactSprayPrefab;
    [SerializeField] private Transform deathImpactSpawnPoint;
    [SerializeField, Min(0.05f)] private float deathImpactLifetime = 3f;
    [SerializeField] private string deathImpactPoolTag;

    [Header("Loot Pool")]
    [SerializeField] private string lootPoolTag;

    private bool isDead;
    private Vector3 deathKnockbackDirection;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        bloodSplat?.SetActive(false);
    }

    public void Start()
    {
        health = maxHealth;
        isDead = false;

        // hide bloodsplat
        bloodSplat?.SetActive(false);

        OnAnyEnemySpawned?.Invoke(this);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        // EnemyMovement movement = GetComponent<EnemyMovement>();
        // if (movement != null)
        // {
        //     movement.enabled = true;
        //     movement.ResetState();
        // }

        // EnemyGun[] guns = GetComponentsInChildren<EnemyGun>(true);
        // for (int i = 0; i < guns.Length; i++)
        // {
        //     if (guns[i] != null)
        //     {
        //         guns[i].enabled = true;
        //         guns[i].ResetState();
        //     }
        // }

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] == null) continue;
            rigidbodies[i].isKinematic = false;
            rigidbodies[i].linearVelocity = Vector3.zero;
            rigidbodies[i].angularVelocity = Vector3.zero;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = true;
            }
        }

        if (animator != null && !string.IsNullOrWhiteSpace(killTriggerName))
        {
            animator.ResetTrigger(killTriggerName);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Destroy(gameObject);
        // SpawnDeathImpactSpray();
        // StartCoroutine(PlayDeathSequenceAndDestroy());
    }

    private void SpawnDeathImpactSpray()
    {
        Transform spawnRef = deathImpactSpawnPoint != null ? deathImpactSpawnPoint : transform;
        Vector3 position = spawnRef.position;

        Vector3 facing = deathKnockbackDirection.sqrMagnitude > 0.0001f
            ? deathKnockbackDirection
            : transform.forward;
        facing.y = 0f;

        Quaternion rotation = facing.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(facing.normalized, Vector3.up)
            : Quaternion.identity;

        // if (!string.IsNullOrEmpty(deathImpactPoolTag) && ObjectPooler.Instance != null)
        // {
        //     ObjectPooler.Instance.SpawnFromPool(deathImpactPoolTag, position, rotation);
        //     return;
        // }

        if (deathImpactSprayPrefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(deathImpactSprayPrefab, position, rotation);
        Destroy(instance, Mathf.Max(0.05f, deathImpactLifetime));
    }

    private void SetDeathKnockbackDirection(Vector3 hitDirection)
    {
        Vector3 planarDirection = hitDirection;
        planarDirection.y = 0f;

        if (planarDirection.sqrMagnitude < 0.0001f)
        {
            planarDirection = transform.forward;
            planarDirection.y = 0f;
        }

        deathKnockbackDirection = planarDirection.sqrMagnitude > 0.0001f
            ? planarDirection.normalized
            : Vector3.zero;
    }

    private IEnumerator PlayDeathSequenceAndDestroy()
    {
        DisableCombatAndMovement();
        TriggerKillAnimation();

        if (killAnimationHoldDuration > 0f)
        {
            yield return new WaitForSeconds(killAnimationHoldDuration);
        }

        Vector3 basePosition = transform.position;
        if (knockUpHeight > 0f && knockUpDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < knockUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / knockUpDuration);
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                Vector3 horizontalOffset = deathKnockbackDirection * (knockBackDistance * eased);
                transform.position = basePosition + horizontalOffset + (Vector3.up * (knockUpHeight * eased));
                yield return null;
            }
        }

        Vector3 startSinkPosition = transform.position;
        Vector3 endSinkPosition = startSinkPosition + (Vector3.down * sinkDistance);
        float sinkElapsed = 0f;
        while (sinkElapsed < sinkDuration)
        {
            sinkElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(sinkElapsed / sinkDuration);
            transform.position = Vector3.Lerp(startSinkPosition, endSinkPosition, t);
            yield return null;
        }

        transform.position = endSinkPosition;

        if (destroyDelayAfterSink > 0f)
        {
            yield return new WaitForSeconds(destroyDelayAfterSink);
        }

        gameObject.SetActive(false);
    }

    private void TriggerKillAnimation()
    {
        if (animator == null || string.IsNullOrWhiteSpace(killTriggerName))
        {
            return;
        }

        animator.ResetTrigger(killTriggerName);
        animator.SetTrigger(killTriggerName);
    }

    private void DisableCombatAndMovement()
    {
        // EnemyMovement movement = GetComponent<EnemyMovement>();
        // if (movement != null)
        // {
        //     movement.enabled = false;
        // }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
            }

            agent.enabled = false;
        }

        // EnemyGun[] guns = GetComponentsInChildren<EnemyGun>(true);
        // for (int i = 0; i < guns.Length; i++)
        // {
        //     if (guns[i] != null)
        //     {
        //         guns[i].enabled = false;
        //     }
        // }

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] == null)
            {
                continue;
            }

            rigidbodies[i].linearVelocity = Vector3.zero;
            rigidbodies[i].angularVelocity = Vector3.zero;
            rigidbodies[i].isKinematic = true;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }
    }
}

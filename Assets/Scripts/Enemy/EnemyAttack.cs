using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackInterval = 2f;
    [SerializeField] float damageAmount = 10f;

    float _attackTimer;

    void Update() => _attackTimer += Time.deltaTime;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerBody playerBody = other.GetComponentInParent<PlayerBody>();
        if (playerBody == null || _attackTimer < attackInterval) return;

        playerBody.TakeDamage(damageAmount);
        _attackTimer = 0f;
    }
}

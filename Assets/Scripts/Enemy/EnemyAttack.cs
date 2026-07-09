using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // if player is in range, attack every few seconds
    public float attackInterval = 2f;
    private float attackTimer = 0f;
    [SerializeField] private float damageAmount = 10f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBody playerBody = other.GetComponentInParent<PlayerBody>();
            if (playerBody != null && attackTimer >= attackInterval)
            {
                playerBody.TakeDamage(damageAmount);
                attackTimer = 0f;
            }
        }
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
    }
}

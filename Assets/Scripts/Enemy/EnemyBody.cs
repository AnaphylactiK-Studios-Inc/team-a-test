using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBody : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] Image healthBarFill;
    float _health;

    public event Action OnDeath;

    void Awake() => _health = maxHealth;

    public void TakeDamage(float amount)
    {
        _health = Mathf.Max(0f, _health - amount);

        if (healthBarFill != null)
            healthBarFill.fillAmount = _health / maxHealth;

        if (_health <= 0f)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}

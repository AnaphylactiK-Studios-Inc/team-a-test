using System.Collections;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float respawnDelay = 3f;

    float _health;
    Transform _visual;
    Collider _collider;
    Vector3 _originalScale;
    bool _broken;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _visual = BuildVisualChild();
        _health = maxHealth;
        _originalScale = _visual.localScale;
    }

    Transform BuildVisualChild()
    {
        var child = new GameObject("Visual").transform;
        child.SetParent(transform, false);

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (mf != null)
        {
            var childMf = child.gameObject.AddComponent<MeshFilter>();
            childMf.sharedMesh = mf.sharedMesh;
            Destroy(mf);
        }

        if (mr != null)
        {
            var childMr = child.gameObject.AddComponent<MeshRenderer>();
            childMr.sharedMaterials = mr.sharedMaterials;
            Destroy(mr);
        }

        return child;
    }

    public void TakeDamage(float amount)
    {
        if (_broken) return;

        _health = Mathf.Max(0f, _health - amount);
        _visual.localScale = _originalScale * (_health / maxHealth);

        if (_health <= 0f)
            StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        _broken = true;
        _visual.localScale = Vector3.zero;
        if (_collider != null) _collider.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        _health = maxHealth;
        _visual.localScale = _originalScale;
        if (_collider != null) _collider.enabled = true;
        _broken = false;
    }
}

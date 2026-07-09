using System.Collections;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] CombatPoint[] combatPoints;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] float travelSpeed = 8f;

    int _aliveCount;

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        foreach (CombatPoint point in combatPoints)
        {
            yield return StartCoroutine(TravelTo(point.transform.position));

            _aliveCount = point.spawns.Length;

            if (_aliveCount == 0)
                continue;

            foreach (var entry in point.spawns)
            {
                if (entry.anchor == null || entry.enemyPrefab == null) { _aliveCount--; continue; }
                var go = Instantiate(entry.enemyPrefab, entry.anchor.position, entry.anchor.rotation);
                var body = go.GetComponent<EnemyBody>();
                if (body != null)
                    body.OnDeath += OnEnemyDied;
                else
                    _aliveCount--;
            }

            yield return new WaitUntil(() => _aliveCount <= 0);
        }
    }

    void OnEnemyDied() => _aliveCount--;

    IEnumerator TravelTo(Vector3 target)
    {
        playerMovement.moveLocked = true;

        CharacterController cc = playerMovement.controller;
        cc.enabled = false;

        Vector3 start = playerMovement.transform.position;
        target = new Vector3(target.x, start.y, target.z); // keep player's Y

        float dist = Vector3.Distance(start, target);
        float elapsed = 0f;
        float duration = dist / Mathf.Max(travelSpeed, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerMovement.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        playerMovement.transform.position = target;
        cc.enabled = true;
        playerMovement.moveLocked = false;
    }
}

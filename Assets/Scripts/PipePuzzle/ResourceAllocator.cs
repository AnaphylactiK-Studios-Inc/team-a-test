using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceAllocator : MonoBehaviour
{
    [Header("Pipe Grids")]
    [SerializeField] PipeGrid resourceA;
    [SerializeField] PipeGrid resourceB;

    [Header("Gun Systems")]
    [SerializeField] Gun shieldBusterGun;
    [SerializeField] Gun mainGun;

    [Header("Player")]
    [SerializeField] PlayerBody playerBody;

    [Header("Heal / Shield Ticks")]
    [SerializeField] float tickInterval = 1f;
    [SerializeField] float baseHealAmount = 5f;
    [SerializeField] float baseShieldAmount = 10f;

    SystemType _aTarget = SystemType.None;
    SystemType _bTarget = SystemType.None;
    readonly Dictionary<SystemType, int> _powerLevels = new();

    float _baseShieldBusterRate;
    float _baseGunRate;
    Coroutine _healCoroutine;
    Coroutine _shieldCoroutine;

    void Start()
    {
        if (shieldBusterGun) _baseShieldBusterRate = shieldBusterGun.fireRate;
        if (mainGun) _baseGunRate = mainGun.fireRate;
    }

    void OnEnable()
    {
        if (resourceA) resourceA.OnPoweredSystemsChanged += OnResourceAChanged;
        if (resourceB) resourceB.OnPoweredSystemsChanged += OnResourceBChanged;
    }

    void OnDisable()
    {
        if (resourceA) resourceA.OnPoweredSystemsChanged -= OnResourceAChanged;
        if (resourceB) resourceB.OnPoweredSystemsChanged -= OnResourceBChanged;
    }

    void OnResourceAChanged(SystemType[] systems)
    {
        _aTarget = systems.Length > 0 ? systems[0] : SystemType.None;
        Recalculate();
    }

    void OnResourceBChanged(SystemType[] systems)
    {
        _bTarget = systems.Length > 0 ? systems[0] : SystemType.None;
        Recalculate();
    }

    void Recalculate()
    {
        foreach (SystemType sys in Enum.GetValues(typeof(SystemType)))
        {
            if (sys == SystemType.None) continue;
            int power = (_aTarget == sys ? 1 : 0) + (_bTarget == sys ? 1 : 0);
            if (_powerLevels.TryGetValue(sys, out int prev) && prev == power) continue;
            _powerLevels[sys] = power;
            ApplyPower(sys, power);
        }
    }

    void ApplyPower(SystemType system, int power)
    {
        switch (system)
        {
            case SystemType.ShieldBuster:
                if (shieldBusterGun != null)
                {
                    shieldBusterGun.isFiring = power > 0;
                    if (power > 0) shieldBusterGun.fireRate = _baseShieldBusterRate * power;
                }
                break;

            case SystemType.Gun:
                if (mainGun != null)
                {
                    mainGun.isFiring = power > 0;
                    if (power > 0) mainGun.fireRate = _baseGunRate * power;
                }
                break;

            case SystemType.Shield:
                if (_shieldCoroutine != null) { StopCoroutine(_shieldCoroutine); _shieldCoroutine = null; }
                if (power > 0) _shieldCoroutine = StartCoroutine(ShieldTick(baseShieldAmount * power));
                break;

            case SystemType.Healing:
                if (_healCoroutine != null) { StopCoroutine(_healCoroutine); _healCoroutine = null; }
                if (power > 0) _healCoroutine = StartCoroutine(HealTick(baseHealAmount * power));
                break;
        }
    }

    IEnumerator ShieldTick(float amount)
    {
        var wait = new WaitForSeconds(tickInterval);
        while (true)
        {
            playerBody?.ReplenishShield(amount);
            yield return wait;
        }
    }

    IEnumerator HealTick(float amount)
    {
        var wait = new WaitForSeconds(tickInterval);
        while (true)
        {
            playerBody?.Heal(amount);
            yield return wait;
        }
    }
}

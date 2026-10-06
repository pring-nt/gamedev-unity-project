using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public PlayerData data;

    public UnityEvent<float> onDamaged;
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDeath;

    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    private readonly List<float> speedModifiers = new List<float>();

    public float SpeedMultiplier
    {
        get
        {
            float total = 1f;
            foreach (float modifier in speedModifiers)
                total *= modifier;

            return Mathf.Clamp(total, data.minSpeedMultiplier, data.maxSpeedMultiplier);
        }
    }

    private void Awake()
    {
        CurrentHealth = data.maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        onDamaged?.Invoke(amount);
        onHealthChanged?.Invoke(CurrentHealth);

        if (IsDead)
            onDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, data.maxHealth);
        onHealthChanged?.Invoke(CurrentHealth);
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        StartCoroutine(SpeedModifierRoutine(multiplier, duration));
    }

    private IEnumerator SpeedModifierRoutine(float multiplier, float duration)
    {
        speedModifiers.Add(multiplier);
        yield return new WaitForSeconds(duration);
        speedModifiers.Remove(multiplier);
    }

    public void ResetStats()
    {
        StopAllCoroutines();
        speedModifiers.Clear();

        CurrentHealth = data.maxHealth;
        onHealthChanged?.Invoke(CurrentHealth);
    }
}
using System;
using Light_and_controller.Scripts.Components;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable, IHealable
{
    [SerializeField] private int health;
    
    public Action<int> OnTakeDamage;
    public Action<int> OnHeal;
    
    public int Health
    {
        get => health;
        protected set => health = value;
    }

    public int MaxHealth { get; set; }

    private void Awake()
    {
        MaxHealth = Health;
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        OnTakeDamage?.Invoke(amount);
        if(Health <= 0) Die();
    }

    public void Heal(int amount)
    {
        Health += amount;
        OnHeal?.Invoke(amount);
    }

    public virtual void Die() {}
}

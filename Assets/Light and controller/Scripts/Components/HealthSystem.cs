using System;
using Light_and_controller.Scripts.Components;
using Light_and_controller.Scripts.Systems;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable, IHealable
{
    [SerializeField] private int health;
    [SerializeField] private float invincibilityDuration = 1f;
    
    public Action<int> OnTakeDamage;
    public Action<int> OnHeal;
    
    private float invincibilityTimer = 0f;
    
    public int Health
    {
        get => health;
        protected set => health = value;
    }

    public int MaxHealth { get; set; }
    
    public bool IsInvincible => invincibilityTimer > 0f;

    protected virtual void Awake()
    {
        MaxHealth = Health;
    }

    protected virtual void Update()
    {
        if(invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    protected virtual void OnEnable()
    {
        EventBus.Subscribe<DamageEvent>(gameObject, OnDamageEvent);
        EventBus.Subscribe<HealEvent>(gameObject, OnHealEvent);
    }

    protected virtual void OnDisable()
    {
        EventBus.Unsubscribe<DamageEvent>(gameObject, OnDamageEvent);
        EventBus.Unsubscribe<HealEvent>(gameObject, OnHealEvent);
    }

    private void OnDamageEvent(DamageEvent evt)
    {
        TakeDamage(evt.Amount);
    }

    private void OnHealEvent(HealEvent evt)
    {
        Heal(evt.Amount);
    }

    public virtual void TakeDamage(int amount)
    {
        if(IsInvincible) return;
        
        Health -= amount;
        OnTakeDamage?.Invoke(amount);
        
        if(Health <= 0)
        {
            Die();
        }
        else
        {
            invincibilityTimer = invincibilityDuration;
        }
    }

    public virtual void Heal(int amount)
    {
        Health += amount;
        OnHeal?.Invoke(amount);
    }

    public virtual void Die() {}
}

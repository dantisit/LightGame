using Light_and_controller.Scripts.Components;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable, IHealable
{
    [SerializeField] private float health;
    
    public float Health
    {
        get => health;
        set => health = value;
    }
    
    public void TakeDamage(float amount)
    {
        Health -= amount;
    }

    public void Heal(float amount)
    {
        Health += amount;
    }
}

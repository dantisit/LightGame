using System;
using System.Collections.Generic;
using System.Linq;
using Light_and_controller.Scripts.Events;
using Light_and_controller.Scripts.Systems;
using UnityEngine;

namespace Light_and_controller.Scripts.UI
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private List<HealthSectorView> healthSectors;

        private void OnEnable()
        {
            SetHealth(healthSystem.Health);
            healthSystem.OnHeal += AddHealth;
            healthSystem.OnTakeDamage += RemoveHealth;
            EventBus.Subscribe<PendingHealEvent>(player, OnHealPending);
            EventBus.Subscribe<PendingDamageEvent>(player, OnTakeDamagePending);
        }

        private void OnDisable()
        {
            healthSystem.OnHeal -= AddHealth;
            healthSystem.OnTakeDamage -= RemoveHealth;
            EventBus.Unsubscribe<PendingHealEvent>(player, OnHealPending);
            EventBus.Unsubscribe<PendingDamageEvent>(player, OnTakeDamagePending);
        }

        private void OnHealPending(PendingHealEvent data)
        {
            data.Cancel += OnHealPendingCancel;
            healthSectors.Where(x => !x.IsFilled)
                .Take(data.Amount)
                .ToList()
                .ForEach(x => x.OnHealPending(data.Duration));
        }

        private void OnHealPendingCancel()
        {
            healthSectors.ForEach(x => x.OnHealPendingCancel());
        }

        private void OnTakeDamagePending(PendingDamageEvent data)
        {
            data.Cancel += OnTakeDamagePendingCancel;
            healthSectors.Where(x => x.IsFilled)
                .TakeLast(data.Amount)
                .ToList()
                .ForEach(x => x.OnTakeDamagePending(data.Duration));
        }
        
        private void OnTakeDamagePendingCancel()
        {
            healthSectors.ForEach(x => x.OnTakeDamagePendingCancel());
        }

        private void AddHealth(int value)
        {
            healthSectors.Where(x => !x.IsFilled)
                .Take(value)
                .ToList()
                .ForEach(x => x.OnHeal());
        }

        private void RemoveHealth(int value)
        {
            healthSectors.Where(x => x.IsFilled)
                .TakeLast(value)
                .ToList()
                .ForEach(x =>
                {
                    x.OnDamage();
                });
        }
        
        private void SetHealth(int value)
        {
            healthSectors.ForEach(x => x.OnDamage());
            healthSectors.Take(Math.Min(value, healthSectors.Count))
                .ToList()
                .ForEach(x => x.OnHeal());
        }
    }
}
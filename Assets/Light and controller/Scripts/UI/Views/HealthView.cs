using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Light_and_controller.Scripts.UI
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private List<GameObject> healthSectors;

        private void OnEnable()
        {
            SetHealth(healthSystem.Health);
            healthSystem.OnHeal += AddHealth;
            healthSystem.OnTakeDamage += RemoveHealth;
            // Start heal - duration to heal
            // Stop heal
            // Start damage - duration to damaage
            // Stop damage
        }

        private void OnDisable()
        {
            healthSystem.OnHeal -= AddHealth;
            healthSystem.OnTakeDamage -= RemoveHealth;
        }

        private void AddHealth(int value)
        {
            healthSectors.Where(x => !x.activeSelf)
                .Take(value)
                .ToList()
                .ForEach(x => x.SetActive(true));
        }

        private void RemoveHealth(int value)
        {
            healthSectors.Where(x => x.activeSelf)
                .TakeLast(value)
                .ToList()
                .ForEach(x => x.SetActive(false));
        }
        
        private void SetHealth(int value)
        {
            healthSectors.ForEach(x => x.SetActive(false));
            healthSectors.Take(Math.Min(value, healthSectors.Count))
                .ToList()
                .ForEach(x => x.SetActive(true));
        }
    }
}
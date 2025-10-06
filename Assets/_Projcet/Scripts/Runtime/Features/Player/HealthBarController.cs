using UnityEngine;
using UnityEngine.UI;

/*
 * Author: Parker Wittenmyer
 * Date: 10-05-2025
 * */

public class HealthBarController : MonoBehaviour
{
    public Slider healthSlider; // ref for actual slider
    public float maxHealth = 100; // max health var
    private float currentHealth; // current health var

    void Start()
    {
        SetMaxHealth(maxHealth); // sets both max health and current health to max at the start
    }

    public void SetMaxHealth(float health) // sets both max health and current health to max
    {
        healthSlider.maxValue = health;
        currentHealth = health;
        healthSlider.value = currentHealth;
    }

    public void SetHealth(float health) // sets current health
    {
        currentHealth = health;
        healthSlider.value = currentHealth;
    }

    public void TakeDamage(float damage) // generic take damage method
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SetHealth(currentHealth);
    }

    public void Heal(float amount) // generic heal method
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SetHealth(currentHealth);
    }
}
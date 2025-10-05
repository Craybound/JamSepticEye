/*
 *  Author: Parker Wittenmyer
 *  Date: 10-3-2025
 */

using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 100; // Generic health amount
    private int currentHealth; // current health varible

    void Start()
    {
        currentHealth = maxHealth; // set current health to max health on start
    }

    public void Damage(int damageAmount)
    {
        currentHealth -= damageAmount; // lose health equal to damage dealt
        if (currentHealth <= 0)
        {
            Die(); // call death method if health hits zero
        }
        Debug.Log(gameObject.name + " Health: " + currentHealth);
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount; // add health equal to damage healed
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // set current health to max health if overhealed
        }
        Debug.Log(gameObject.name + " Health: " + currentHealth);
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        Destroy(gameObject); // destroy object if it's health hits zero
    }
}
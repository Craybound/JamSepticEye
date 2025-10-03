/*
 *  Author: Parker Wittenmyer
 *  Date: 10-3-2025
 */

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform target; // target var for object targeting
    public Vector3 targetOffset = new Vector3(.5f, 0f, .5f); // offset for object targeting
    private NavMeshAgent agent; // NavMeshAgent var for movement
    public int damage = 10; // Generic damage to deal
    public float attackRange = 2f; // Generic attack range
    public float attackSpeed = 1f; // Generic attck speed / high number means slower attack speed
    public float attackTimer = 1f; // Timer for next attack
    HealthSystem healthSys; // HealthSystem var for damage

    void Start()
    {
        SetTarget();
        healthSys = target.GetComponent<HealthSystem>();
    }

    void Update()
    {
        Debug.LogWarning(Vector3.Distance(transform.position, target.position));
        if (target != null && Vector3.Distance(transform.position, target.position) >= attackRange)
        {
            agent.SetDestination(target.position); // sets the target to the player's position
        }
        else if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            Attack();
        }
    }

    private void SetTarget()
    {
        agent = GetComponent<NavMeshAgent>(); // sets the agent var to the NavMeshAgent component
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // finds the player via tag
            if (playerObject != null)
            {
                target = playerObject.transform; // sets player target to the previously found player tag
            }
            else
            {
                Debug.LogWarning("Player target not assigned and no object with 'Player' tag found.");
            }
        }
    }

    private void Attack()
    {
        if (Time.time > attackTimer)
        {
            healthSys.Damage(damage);
            attackTimer = Time.time + attackSpeed;
        }
    }
}
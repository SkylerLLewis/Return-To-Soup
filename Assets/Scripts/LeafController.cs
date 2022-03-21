using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    float health = 5, maxHealth;
    void Awake () {
        gameObject.layer = 6; // Plants

        maxHealth = health;
    }

    public bool Eaten(float dmg) {
        health -= dmg;
        if (health <= 0) {
            Die();
            return true;
        }
        return false;
    }

    void Die() {
        Destroy(transform.gameObject);
    }
}

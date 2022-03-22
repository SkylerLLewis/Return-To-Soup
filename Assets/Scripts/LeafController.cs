using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    float health = 10, maxHealth;
    int id;
    PlantController parent;

    void Awake () {
        gameObject.layer = 6; // Plants

        parent = transform.parent.GetComponent<PlantController>();

        maxHealth = health;
    }

    public void SetStats(int leafID, Color color) {
        if (transform.position.y > 0) { Destroy(transform.gameObject); }
        id = leafID;
        if (color.g != 1) {
            transform.gameObject.GetComponent<MeshRenderer>().material.color = color;
        }
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
        parent.UpdateLeaves(id);
        Destroy(transform.gameObject);
    }

    public void DisplayStats() {
        parent.DisplayStats();
    }
}

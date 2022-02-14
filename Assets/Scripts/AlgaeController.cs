using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

public class AlgaeController : MonoBehaviour
{
    Rigidbody rb;
    GameObject baby, plants;
    //int tick, reproduceThreshold = 100;
    float reproduceTimer;
    public int reproduceCount;
    public int health;
    float diameter = 0.75f;
    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        baby = Resources.Load("Prefabs/Algae") as GameObject;
        plants = GameObject.Find("Plants");

        //tick = Random.Range(0, 9);
        //reproduceCount = Random.Range(0, reproduceThreshold);
        reproduceTimer = 20f;
        health = 50;
        InvokeRepeating("Reproduce", Random.Range(0f, reproduceTimer), reproduceTimer);
    }

    void Update() {
        
    }

    private void FixedUpdate() {
        // Flotation
        /*if (transform.position.y <= 0) {
            rb.AddForce(Vector3.up*10);//, ForceMode.Acceleration);
        } else if (transform.position.y < 0.1f) {
            rb.AddForce(new Vector3(0, -100*(transform.position.y-0.1f), 0));
        }*/

        /*if (tick == 10) {
            if (transform.position.y > 0.3f) {
                Eaten();
            }
            if (reproduceCount >= reproduceThreshold) {
                if (Random.Range(0,4) < 1 && transform.position.y < 0.3f){// && transform.position.y > 0) {
                    Reproduce();
                }
                reproduceCount = 0;
            }
            reproduceCount++;
            tick = 0;
        }
        tick++;*/
    }

    void Reproduce() {
        // only a 1 in four chance of bebe
        if (Random.Range(0,4) > 0){ return; }
        Vector3 pos = FindSpot();
        if (pos == Vector3.zero) { return; } // no room
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            plants.transform);
        clone.name = clone.name.Split('(')[0];
    }

    Vector3 FindSpot() {
        Vector3 direction = Vector3.zero;
        Vector3 probe1 = new Vector3();
        Vector3 probe2 = new Vector3();
        int sentinel = 0;
        float angle = Random.Range(0f, 2f) * Mathf.PI;
        float[] fork = {angle + Mathf.PI/6, angle - Mathf.PI/6};
        do {
            if (sentinel > 0) {
                angle += Mathf.PI/3; // iterate around the circle
                fork[0] += Mathf.PI/3;
                fork[1] += Mathf.PI/3;
                //Utilities.TraceLine(transform.position, transform.position+(direction*1.5f), Color.red);
            }
            probe1.x = diameter*Mathf.Cos(fork[0]);
            probe1.z = diameter*Mathf.Sin(fork[0]);
            probe2.x = diameter*Mathf.Cos(fork[1]);
            probe2.z = diameter*Mathf.Sin(fork[1]);
            sentinel++;
            if (sentinel == 6) {break;}
        } while (
            Physics.Raycast(transform.position, probe1, diameter*1.5f) ||
            Physics.Raycast(transform.position, probe2, diameter*1.5f)
        );

        if (sentinel != 6) {
            direction.x = diameter*Mathf.Cos(angle);
            direction.z = diameter*Mathf.Sin(angle);
            return transform.position + direction;
        } else {
            return Vector3.zero;
        }
    }

    public bool Eaten() {
        health--;
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


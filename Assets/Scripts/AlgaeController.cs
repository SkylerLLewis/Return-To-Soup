using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

public class AlgaeController : MonoBehaviour
{
    Rigidbody rb;
    GameObject baby, plants, staticPlants;
    //int tick, reproduceThreshold = 100;
    float reproduceTimer;
    public int reproduceCount;
    float growTime;
    public int health;
    float diameter = 0.75f;
    public bool adult;

    void Awake() {
        rb = gameObject.GetComponent<Rigidbody>();
        baby = Resources.Load("Prefabs/Algae") as GameObject;
        plants = GameObject.Find("Plants");
        staticPlants = GameObject.Find("Static Plants");

        //tick = Random.Range(0, 9);
        //reproduceCount = Random.Range(0, reproduceThreshold);
        reproduceTimer = 20f;
        growTime = 90f;
        health = 50;
        
        adult = false;

        Invoke("Grow", Random.Range(0.8f,1.2f)*growTime);
    }

    void Start() {
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

    public void Grow() {
        if (!adult) {
            adult = true;
            transform.localScale *= 2f;
            transform.gameObject.GetComponent<SphereCollider>().radius = 0.5f;
            transform.SetParent(plants.transform, true);
            InvokeRepeating("Reproduce", Random.Range(0f, reproduceTimer), reproduceTimer);
        }
    }

    void Reproduce() {
        // only a 1 in four chance of bebe
        //if (Random.Range(0,4) > 0){ return; }
        Vector3 pos = FindSpot();
        if (pos == Vector3.zero) { return; } // no room
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            staticPlants.transform);
        clone.name = clone.name.Split('(')[0];
    }

    // This funciton now only tries once, keeping code there for future possibilities
    Vector3 FindSpot() {
        // Standard, spread outwards
        if (Random.Range(0,40) > 0) {
            Vector3 direction = Vector3.zero;
            float angle = Random.Range(0f, 2f) * Mathf.PI;
            direction.x = diameter*Mathf.Cos(angle);
            direction.z = diameter*Mathf.Sin(angle);
            if (!Physics.CheckSphere(transform.position+direction, diameter*0.5f)) {
                return transform.position + direction;
            } else {
                return Vector3.zero;
            }
        } else { // Occasional far-spread
            Vector3 direction = Vector3.zero;
            float angle = Random.Range(0f, 2f) * Mathf.PI;
            float dist = Random.Range(5f, 20f);
            direction.x = diameter*Mathf.Cos(angle) * dist;
            direction.z = diameter*Mathf.Sin(angle) * dist;
            if (!Physics.CheckSphere(transform.position+direction, diameter*0.5f) &&
                !Physics.Raycast(transform.position, direction, direction.magnitude, LayerMask.GetMask("Terrain"))) {
                return transform.position + direction;
            } else {
                return Vector3.zero;
            }
        }
        /* This is for a 360 test
        int sentinel = 0;
        Vector3 probe1 = new Vector3();
        Vector3 probe2 = new Vector3();
        float[] fork = {angle + Mathf.PI/6, angle - Mathf.PI/6};
        do {
            if (sentinel > 0) {
                angle += Mathf.PI/3; // iterate around the circle
                fork[0] += Mathf.PI/3;
                fork[1] += Mathf.PI/3;
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
        }*/
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


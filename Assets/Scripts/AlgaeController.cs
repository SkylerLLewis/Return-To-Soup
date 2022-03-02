using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;
using TMPro;

public class AlgaeController : MonoBehaviour
{
    Rigidbody rb;
    GameObject baby, plants, staticPlants, xSprite, ySprite;
    TextMeshProUGUI panelTitle, panelLabels, panelValues;
    //int tick, reproduceThreshold = 100;
    float reproduceTimer;
    public int reproduceCount;
    float growTime;
    public float health, maxHealth;
    float diameter = 1.1f;
    public bool adult;

    void Awake() {
        gameObject.layer = 8; // Statics

        rb = gameObject.GetComponent<Rigidbody>();
        baby = Resources.Load("Prefabs/Algae") as GameObject;
        plants = GameObject.Find("Plants");
        staticPlants = GameObject.Find("Static Plants");
        panelTitle = GameObject.Find("Info Panel Title").GetComponent<TextMeshProUGUI>();
        panelLabels = GameObject.Find("Info Panel Labels").GetComponent<TextMeshProUGUI>();
        panelValues = GameObject.Find("Info Panel Values").GetComponent<TextMeshProUGUI>();

        foreach (Transform child in transform) {
            if (child.name == "xSprite") {
                xSprite = child.gameObject;
            } else if (child.name == "ySprite") {
                ySprite = child.gameObject;
            }
        }
        xSprite.SetActive(false);
        ySprite.SetActive(false);

        //tick = Random.Range(0, 9);
        //reproduceCount = Random.Range(0, reproduceThreshold);
        reproduceTimer = 25f;
        growTime = 90f;
        maxHealth = 5;
        health = maxHealth;
        
        adult = false;

        Invoke("Grow", Random.Range(0.8f,1.2f)*growTime);
    }

    public void Grow() {
        if (!adult) {
            gameObject.layer = 6; // Plants
            adult = true;
            //transform.localScale *= 2f;
            //transform.gameObject.GetComponent<SphereCollider>().radius = 0.5f;
            transform.SetParent(plants.transform, true);
            xSprite.SetActive(true);
            ySprite.SetActive(true);
            
            /*GameObject xSprite = new GameObject("xSprite", typeof(SpriteRenderer));
            Vector3 pos = transform.position;
            pos.y -= 0.5f;
            xSprite.transform.position = pos;
            xSprite.transform.SetParent(transform, true);
            xSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Algae Below");
            GameObject ySprite = new GameObject("ySprite", typeof(SpriteRenderer));
            pos = transform.position;
            pos.y -= 0.5f;
            ySprite.transform.position = pos;
            ySprite.transform.eulerAngles = new Vector3(0, 90, 0);
            ySprite.transform.SetParent(transform, true);
            ySprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Algae Below");*/
            InvokeRepeating("Reproduce", Random.Range(0f, reproduceTimer), reproduceTimer);
        }
    }

    void Reproduce() {
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
        if (Random.Range(0,50) > 0) {
            Vector3 direction = Vector3.zero;
            float angle = Random.Range(0f, 2f) * Mathf.PI;
            direction.x = diameter*Mathf.Cos(angle);
            direction.z = diameter*Mathf.Sin(angle);
            if (!Physics.CheckSphere(transform.position+direction, diameter*0.5f) &&
                !Physics.Raycast(transform.position, direction, diameter*2f, LayerMask.GetMask("Terrain"))) {
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

    public void DisplayStats() {
        string labels="", values="";
        if (!adult) {
            panelTitle.text = "Algae Spores";
        } else {
            panelTitle.text = "Algae";
        }
        labels += "HP:\n"; values += Mathf.RoundToInt(100*health/maxHealth)+"%\n";
        panelLabels.text = labels;
        panelValues.text = values;
    }
}


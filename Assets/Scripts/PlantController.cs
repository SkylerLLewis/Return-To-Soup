using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;
using TMPro;

public class PlantController : MonoBehaviour
{
    TextMeshProUGUI panelTitle, panelLabels, panelValues;
    GameObject baby, leafPrefab, plants, roots;
    public float food;
    float metaTick = 5,
          growInterval = 30,
          leafCost = 100,
          reproduceInterval = 200;
    float depth, depthFactor, growTimer, saveThreshold, reproduceTimer, birth,
          depthanin, reproduceCost, seedDelay;
    Color color;
    int leaves = 0;
    List<int> missingLeaves;
    
    void Awake() {
        gameObject.layer = 9; // Roots

        baby = Resources.Load("Prefabs/Root") as GameObject;
        leafPrefab = Resources.Load("Prefabs/Leaf") as GameObject;
        plants = GameObject.Find("Plants");
        roots = GameObject.Find("Roots");
        panelTitle = GameObject.Find("Info Panel Title").GetComponent<TextMeshProUGUI>();
        panelLabels = GameObject.Find("Info Panel Labels").GetComponent<TextMeshProUGUI>();
        panelValues = GameObject.Find("Info Panel Values").GetComponent<TextMeshProUGUI>();

        missingLeaves = new List<int>();

        depth = Mathf.Abs(transform.position.y);
        saveThreshold = 200+ 25*leaves;
        birth = Time.time;
    }
    
    public void SetStats(float d, float repCost, float sD, bool seeder=false) {
        // Seed investment
        reproduceCost = repCost;
        food = reproduceCost/2;

        // Seed delay
        seedDelay = sD;

        // Depthanin pigment effects
        depthanin = d;
        // Standard photosynthesis breaks even at 50m depth
        depthFactor = (50-depth)/50;
        if (depthanin > 0) {
            // Average depthanin and standard (depthanin 1 is just depthanin)
            // Depthanin Starts at 50% efficiency, but only loses 0.25%/m
            depthFactor = (depthFactor * (1-depthanin)) + ((0.5f - 0.5f*depth/200) * depthanin);
        }

        // Coloration
        if (depthanin > 0) {
            color = new Color(
                0.4f-0.2f*depthanin,
                0.7f-0.7f*depthanin,
                0.2f+0.2f*depthanin
            );
        } else {
            color = Color.green;
        }
        
        if (!seeder) {
            Invoke("Sprout", growInterval*seedDelay);
        } else {
            food = Random.Range(reproduceCost/2, reproduceCost*3);
            Sprout();
        }
    }

    public void Sprout() {
        birth = Time.time;
        InvokeRepeating("Metabolism", Random.Range(0f, metaTick), metaTick);
        InvokeRepeating("Grow", Random.Range(0f, growInterval), growInterval);
        InvokeRepeating("Reproduce", Random.Range(0f, growInterval), reproduceInterval);
    }

    void Metabolism() {
        leaves = transform.childCount-1;
        if (leaves != 0) {
            if (food < 4000) {
                food += (20 + leaves*5) * depthFactor;
            }
        } else {
            food -= 20;
            if (food <= 0) {
                Die();
            }
        }
    }

    void Grow() {
        saveThreshold = 200+ 25*leaves;
        if ((leaves != 0 && food >= saveThreshold) || (leaves == 0 && food >= leafCost) &&
        (leaves <= 80 && Mathf.Ceil(leaves/5)+1 < Mathf.Floor(depth))) {
            if (missingLeaves.Count > 0) {
                missingLeaves.Sort();
            }
            do {
                int leaf = leaves;
                if (missingLeaves.Count > 0) {
                    leaf = missingLeaves[0];
                    missingLeaves.RemoveAt(0);
                }
                food -= leafCost;
                Quaternion angle = Quaternion.identity;
                Vector3 eulerAngle = Vector3.zero;
                Vector3 pos = this.transform.position;
                pos.x += Random.Range(-0.05f, 0.05f);
                pos.z += Random.Range(-0.05f, 0.05f);
                int mod = leaf % 5;
                if (mod == 0) { // Upward growth
                    pos.y += 1f * (leaf/5) + 0.75f;
                } else { // Angled
                    pos.y += 1f * (leaf/5) + 0.55f;
                    eulerAngle.x = 45;
                    eulerAngle.y = Random.Range(0, 360);
                    pos.z += 0.3f*Mathf.Cos(eulerAngle.y*Mathf.Deg2Rad);
                    pos.x += 0.3f*Mathf.Sin(eulerAngle.y*Mathf.Deg2Rad);
                    /*if (mod == 1) { // Northeast Growth
                        pos.x += 0.15f;
                        pos.z += 0.15f;
                        eulerAngle.y = 45;
                    } else if (mod == 2) { // Southeast Growth
                        pos.x += 0.15f;
                        pos.z -= 0.15f;
                        eulerAngle.y = 135;
                    } else if (mod == 3) { // Southwest Growth
                        pos.x -= 0.15f;
                        pos.z -= 0.15f;
                        eulerAngle.y = 225;
                    } else if (mod == 4) { // Northwest Growth
                        pos.x -= 0.15f;
                        pos.z += 0.15f;
                        eulerAngle.y = 315;
                    }*/
                }
                angle.eulerAngles = eulerAngle;
                // Create New leaf!
                GameObject clone = Instantiate(
                    leafPrefab,
                    pos,
                    angle,
                    this.transform);
                clone.name = clone.name.Split('(')[0];
                clone.GetComponent<LeafController>().SetStats(leaf, color);
                leaves++;
            } while (food > saveThreshold);
        }
    }

    void Reproduce() {
        while (food >= reproduceCost) {
            Vector3 pos = FindSpot();
            if (pos == Vector3.zero) { break; } // no spot
            float newDepthanin = depthanin;
            float newReproduceCost = reproduceCost;
            float newSeedDelay = seedDelay;
            if ((depthanin != 0 && depthanin != 1) || Random.Range(0,100) < 5) { // 5% chance to evolve depthanin
                newDepthanin = Mathf.Clamp01(depthanin+Random.Range(-0.50f, 0.50f)); // max 50% variance
            }
            if (Random.Range(0, 100) < 5) { // 5% chance to change seed investment
                newReproduceCost = reproduceCost * Random.Range(0.8f, 1.2f); // max 20% variance
            }
            if (Random.Range(0,100) < 5) { // 5% chance to change it
                newSeedDelay = seedDelay * Random.Range(0.8f, 1.2f); // max 20% variance
            }
            GameObject clone = Instantiate(
                baby,
                pos,
                Quaternion.identity,
                roots.transform);
            clone.name = clone.name.Split('(')[0];
            clone.GetComponent<PlantController>().SetStats(newDepthanin, newReproduceCost, newSeedDelay);
            food -= reproduceCost;
        }
        reproduceTimer = Time.time;
        // Die, if old enough
        if (Time.time - birth > reproduceInterval*10) {
            Die();
        }
    }

    Vector3 FindSpot() {
        Vector3 pos = transform.position;
        float angle = Random.Range(0f, 2f) * Mathf.PI;
        float distance = Random.Range(3f, 4f);
        if (Random.Range(0,25) == 0) {
            distance = Random.Range(10f, 25f);
        }
        pos.x += distance*Mathf.Cos(angle);
        pos.z += distance*Mathf.Sin(angle);
        pos.y = 50;
        // Find the ground nearby
        RaycastHit hit;
        if (!Physics.Raycast(pos, -Vector3.up, out hit, 150, LayerMask.GetMask("Terrain"))) {
            return Vector3.zero; // No hit = no spot
        }
        if (hit.point.y > -1) { return Vector3.zero; } // Not deep enough
        pos.y = hit.point.y;
        // Check if space is occupied
        if (Physics.CheckSphere(pos, 2f, LayerMask.GetMask("Roots"))) {
            return Vector3.zero;
        }
        return pos;
    }

    public void UpdateLeaves(int h) {
        missingLeaves.Add(h);
    }


    void Die() {
        Destroy(transform.gameObject);
    }

    public void DisplayStats() {
        string labels="", values="";
        panelTitle.text = "Seaweed";
        labels += "Food:\n"; values += Mathf.RoundToInt(food)+"\n";
        labels += "Seed Time:\n";
        int timeToSeed = Mathf.RoundToInt(reproduceInterval - (Time.time - reproduceTimer));
        if (timeToSeed > 0) {
            values += timeToSeed+"s\n";
        } else {
            values += "0s\n";
        }
        labels += "Seed Cost:\n"; values += Mathf.RoundToInt(reproduceCost)+"\n";
        labels += "Sprout T:\n"; values += Mathf.RoundToInt(seedDelay)+"\n";
        if (depthanin > 0) {
            labels += "Depthanin:\n"; values += Mathf.RoundToInt(depthanin*100)+"%\n";
        } else {
            labels += "Depthanin:\n"; values += "- \n";
        }
        labels += "Light:\n"; values += Mathf.RoundToInt(100f*depthFactor)+"%\n";
        labels += "Age:\n"; values += Mathf.RoundToInt(Time.time - birth)+"\n";
        panelLabels.text = labels;
        panelValues.text = values;
    }
}

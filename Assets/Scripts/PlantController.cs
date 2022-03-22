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
          growInterval = 25,
          leafCost = 100,
          reproduceCost = 500,
          reproduceInterval = 200;
    float depth, depthFactor, growTimer, saveThreshold, reproduceTimer, birth,
          depthanin;
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

        birth = Time.time;
        depth = Mathf.Abs(transform.position.y);
        food = reproduceCost + 20*(1+growInterval/metaTick);
        saveThreshold = 200+ 25*leaves;
    }
    
    public void SetStats(float d, bool seeder=false) {
        
        // Depthanin pigment effects
        depthanin = d;
        depthFactor = Mathf.Clamp(depth/10f, 0.5f, Mathf.Infinity);
        if (depthanin > 0) {
            depthFactor = 0.5f + (depthFactor/(1+depthanin));
        }

        // Coloration
        if (depthanin > 0) {
            color = new Color(
                0.5f*depthanin,
                1f-depthanin,
                1f*depthanin
            );
        } else {
            color = Color.green;
        }
        
        if (!seeder) {
            Invoke("Sprout", growInterval*Random.Range(16f, 24f));
        } else {
            Sprout();
        }
    }

    public void Sprout() {
        growTimer = Time.time - growInterval;
        reproduceTimer = Time.time;
        InvokeRepeating("Metabolism", Random.Range(0f, metaTick), metaTick);
    }

    void Metabolism() {
        leaves = transform.childCount-1;
        if (leaves != 0) {
            if (food < 4000) {
                food += (10 + leaves*5) /depthFactor;
                saveThreshold = 200+ 25*leaves;
            }
            // Reproduce if you can
            if (Time.time - reproduceTimer > reproduceInterval) {
                Reproduce();
            }
        } else {
            food -= 20;
            if (food <= 0) {
                Die();
            }
        }
        // Make leaf if you can, save up if you're not leafless
        if (((leaves != 0 && food >= saveThreshold) || (leaves == 0 && food >= leafCost))
        && Time.time - growTimer >= growInterval) {
            Grow();
        } else {
            //Debug.Log("Truth values: (((leaves != 0 && food >= saveThreshold) || (leaves == 0 && food >= leafCost)) && Time.time - growTimer >= growInterval) \n((("+(leaves != 0 && food >= saveThreshold)+") || ("+(leaves == 0 && food >= leafCost)+")) && "+(Time.time - growTimer >= growInterval)+")");
        }
    }

    void Grow() {
        if (Mathf.Ceil(leaves/5)+1 >= Mathf.Floor(depth)) {
            return; // Do not grow out of the water
        }
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
        growTimer = Time.time;
    }

    void Reproduce() {
        Vector3 pos = FindSpot();
        if (pos == Vector3.zero) { return; } // no spot
        float newDepthanin = depthanin;
        if ((depthanin != 0 && depthanin != 1) || Random.Range(0,100) < 1) { // 1% chance to evolve depthanin
            Debug.Log("DEPTHANIN SPOTTED");
            newDepthanin = Mathf.Clamp01(depthanin*Random.Range(0.90f, 1.10f)); // max 10% variance
        }
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            roots.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<PlantController>().SetStats(newDepthanin);
        reproduceTimer = Time.time;
        // Die, if old enough
        if (Time.time - birth > reproduceInterval*10) {
            Die();
        }
    }

    Vector3 FindSpot() {
        Vector3 pos = transform.position;
        float angle = Random.Range(0f, 2f) * Mathf.PI;
        float distance = Random.Range(1.5f, 3f);
        if (Random.Range(0,25) == 0) {
            distance = Random.Range(10f, 25f);
        }
        pos.x += distance*Mathf.Cos(angle);
        pos.z += distance*Mathf.Sin(angle);
        pos.y = 50;
        // Find the ground nearby
        RaycastHit hit;
        if (!Physics.Raycast(pos, -Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
            return Vector3.zero; // No hit = no spot
        }
        if (hit.point.y > -1) { return Vector3.zero; } // Not deep enough
        pos.y = hit.point.y;
        // Check if space is occupied
        if (Physics.CheckSphere(pos, 1, LayerMask.GetMask("Roots"))) {
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
        labels += "Seed:\n";
        int timeToSeed = Mathf.RoundToInt(reproduceInterval - (Time.time - reproduceTimer));
        if (timeToSeed > 0) {
            values += timeToSeed+"s\n";
        } else {
            values += "0s\n";
        }
        labels += "Age:\n"; values += Mathf.RoundToInt(Time.time - birth)+"\n";
        panelLabels.text = labels;
        panelValues.text = values;
    }
}

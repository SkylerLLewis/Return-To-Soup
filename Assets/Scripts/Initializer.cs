using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    static int seedPlants = 100,
               seedFish = 10,
               seedOmnivores = 0,
               seedCarnivores = 0;
    // Spawn Rang: X {-250, -400}, Z {300, 440}
    static Vector3 spawnCenter = new Vector3(-325, 5, 370),
                   spawnRange = new Vector3(75, 0, 70);
    GameObject staticPlants, plants, fishes, plant, fish, roots;
    //int regenCounter = 0, regenThreshold = 500;
    public bool deadMode;

    void Start() {
        Screen.SetResolution (1920, 1080, false);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        staticPlants = GameObject.Find("Static Plants");
        roots = GameObject.Find("Roots");
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");

        // Prefabs
        plant = Resources.Load("Prefabs/Root") as GameObject;

        fish = Resources.Load("Prefabs/Fish") as GameObject;

        if (!deadMode) {
            // Spawn Plants
            for (int i=0; i < seedPlants; i++) {
                SpawnPlants();
            }
            // Spawn Fishies
            for (int i=0; i < seedFish; i++) {
                SpawnFish();
            }

            // Spooky fishie
            for (int i=0; i < seedOmnivores; i++) {
                SpawnOmnivore();
            }

            // Spooky fishie
            for (int i=0; i < seedCarnivores; i++) {
                SpawnCarnivore();
            }
        }

        Time.timeScale = 1f;
    }

    /*void FixedUpdate() {
        if (!deadMode) {
            regenCounter++;
            if (regenCounter >= regenThreshold) {
                Regrowth();
                regenCounter = 0;
            }
        }
    }*/

    void Regrowth() {
        //SpawnAlgae();
    }

    Vector3 FindWater() {
        Vector3 pos = Vector3.zero;
        int sentinel = 0;
        do {
            //if (sentinel > 0) { TraceLine(new Vector3(pos.x, 20, pos.z), pos, Color.red); }
            pos.x = spawnCenter.x + Random.Range(-spawnRange.x, spawnRange.x);
            pos.z = spawnCenter.z + Random.Range(-spawnRange.z, spawnRange.z);
            sentinel++;
            if (sentinel > 10) {
                pos = spawnCenter;
                break;
            }
        } while (Physics.Raycast(new Vector3(pos.x, 50, pos.z), -Vector3.up, 50));
        //TraceLine(new Vector3(pos.x, 20, pos.z), pos, Color.green);
        return pos;
    }

    Vector3 FindUnderWater() {
        Vector3 pos = Vector3.zero;
        int sentinel = 0;
        RaycastHit hit;
        // Trash raycast to fill hit var
        Physics.Raycast(Vector3.zero, -Vector3.up, out hit, 50);
        hit.point = Vector3.zero;
        do {
            //if (sentinel > 0) { TraceLine(new Vector3(pos.x, 20, pos.z), pos, Color.red); }
            pos.x = spawnCenter.x + Random.Range(-spawnRange.x, spawnRange.x);
            pos.z = spawnCenter.z + Random.Range(-spawnRange.z, spawnRange.z);
            sentinel++;
            if (sentinel > 10) {
                pos = spawnCenter;
                break;
            }
        } while (!Physics.Raycast(new Vector3(pos.x, -1, pos.z), -Vector3.up, out hit, 50));
        //TraceLine(new Vector3(pos.x, 20, pos.z), pos, Color.green);
        return hit.point;
    }

    void SpawnAlgae() {
        GameObject clone = Instantiate(
            plant,
            FindWater(),
            Quaternion.identity,
            staticPlants.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<AlgaeController>().Grow();
    }

    void SpawnPlants() {
        GameObject clone = Instantiate(
            plant,
            FindUnderWater(),
            Quaternion.identity,
            roots.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<PlantController>().Sprout();
    }

    void SpawnFish() {
        GameObject clone = Instantiate(
            fish,
            FindWater(),
            Quaternion.identity,
            fishes.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<FishController>().SetStats(1f, 1, Color.white);//Random.Range(0.1f, 2f), 1);
    }

    void SpawnOmnivore() {
        GameObject clone = Instantiate(
            fish,
            FindWater(),
            Quaternion.identity,
            fishes.transform);
        clone.name = "Omnivore";
        clone.GetComponent<FishController>().SetStats(1.5f, 0.5f, Color.black);
    }

    void SpawnCarnivore() {
        GameObject clone = Instantiate(
            fish,
            FindWater(),
            Quaternion.identity,
            fishes.transform);
        clone.name = "Omnivore";
        clone.GetComponent<FishController>().SetStats(4f, 0, Color.blue);
    }
}

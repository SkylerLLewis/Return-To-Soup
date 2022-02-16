using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    static int seedPlants = 400,
               seedFish = 40,
               seedOmnivores = 2;
    // Spawn Rang: X {-250, -400}, Z {300, 440}
    static Vector3 spawnCenter = new Vector3(-325, 5, 370),
                   spawnRange = new Vector3(75, 0, 70);
    GameObject staticPlants, plants, fishes, plant, fish;
    //int regenCounter = 0, regenThreshold = 500;
    public bool deadMode;

    void Start() {
        Screen.SetResolution (1920, 1080, false);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        staticPlants = GameObject.Find("Static Plants");
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");

        // Prefabs
        plant = Resources.Load("Prefabs/Algae") as GameObject;

        fish = Resources.Load("Prefabs/Fish") as GameObject;

        if (!deadMode) {
            // Spawn Plants
            for (int i=0; i < seedPlants; i++) {
                SpawnAlgae();
            }
            // Spawn Fishies
            for (int i=0; i < seedFish; i++) {
                SpawnFish();
            }

            // Spooky fishie
            for (int i=0; i < seedOmnivores; i++) {
                SpawnOmnivore();
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
        } while (Physics.Raycast(new Vector3(pos.x, 20, pos.z), -Vector3.up, 20));
        //TraceLine(new Vector3(pos.x, 20, pos.z), pos, Color.green);
        return pos;
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
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<FishController>().SetStats(2f, 0.5f, Color.black);
    }
}

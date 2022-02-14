using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    static int seedPlants = 20,
               seedFish = 10;
    static Vector3 spawnCenter = new Vector3(-320, 5, 370),
                   spawnRange = new Vector3(60, 0, 70);
    GameObject plants, fishes, plant, fish;
    int regenCounter = 0, regenThreshold = 500;
    public bool deadMode;

    void Start() {

        Screen.SetResolution (1920, 1080, false);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
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
        }

        Time.timeScale = 10f;
    }

    void Update() {
        
    }

    void FixedUpdate() {
        if (!deadMode) {
            regenCounter++;
            if (regenCounter >= regenThreshold) {
                Regrowth();
                regenCounter = 0;
            }
        }
    }

    void Regrowth() {
        SpawnAlgae();
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
            plants.transform);
        clone.name = clone.name.Split('(')[0];
    }

    void SpawnFish() {
        GameObject clone = Instantiate(
            fish,
            FindWater(),
            Quaternion.identity,
            fishes.transform);
        clone.name = clone.name.Split('(')[0];
    }
}

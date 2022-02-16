using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggController : MonoBehaviour
{
    GameObject baby, fishes;
    float size, herbivorousness;
    Color color;
    public void SetStats(float s, float h, Color c) {
        baby = Resources.Load("Prefabs/Fish") as GameObject;
        fishes = GameObject.Find("Fishes");
        size = s;
        herbivorousness = h;
        color = c;
        transform.gameObject.GetComponent<MeshRenderer>().material.color = color;
        transform.localScale *= size;
        Invoke("Spawn", 60*size);
    }

    void Spawn() {
        Vector3 pos = transform.position;
        pos.y += 0.2f;
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            fishes.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<FishController>().SetStats(size, herbivorousness, color);
        Destroy(transform.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EggController : MonoBehaviour
{
    GameObject baby, fishes;
    float size, herbivorousness;
    Color color;
    TextMeshProUGUI panelTitle, panelLabels, panelValues;
    public void SetStats(float s, float h, Color c) {
        gameObject.layer = 8; // Statics

        baby = Resources.Load("Prefabs/Fish") as GameObject;
        fishes = GameObject.Find("Fishes");
        panelTitle = GameObject.Find("Info Panel Title").GetComponent<TextMeshProUGUI>();
        panelLabels = GameObject.Find("Info Panel Labels").GetComponent<TextMeshProUGUI>();
        panelValues = GameObject.Find("Info Panel Values").GetComponent<TextMeshProUGUI>();

        size = s;
        herbivorousness = h;
        color = c;
        transform.gameObject.GetComponent<MeshRenderer>().material.color = color;
        transform.localScale *= size;
        Invoke("Spawn", 120*size);
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

    public void DisplayStats() {
        string labels="", values="";
        if (herbivorousness == 1) {
            panelTitle.text = "Herbivore";
        } else if (herbivorousness > 0) {
            panelTitle.text = "Omnivore";
        } else {
            panelTitle.text = "Carnivore";
        }
        labels += "Size:\n"; values += (Mathf.RoundToInt(size*100)/100f).ToString()+"\n";
        panelLabels.text = labels;
        panelValues.text = values;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIController : MonoBehaviour
{
    TextMeshProUGUI timeSpeed;
    void Start()
    {
        timeSpeed = GameObject.Find("Speed Label").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        
    }

    public void SpeedUp() {
        if (Time.timeScale < 32f) {
            Time.timeScale *= 2;
        }
        timeSpeed.text = Time.timeScale+"x";
    }

    public void SlowDown() {
        if (Time.timeScale > 0.125f) {
            Time.timeScale /= 2;
        }
        timeSpeed.text = Time.timeScale+"x";
    }
}

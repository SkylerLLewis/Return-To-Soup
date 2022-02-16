using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    public Rigidbody body;
    public CharacterController character;
    
    TextMeshProUGUI plantCount, herbCount, omniCount;
    GameObject plants, fishes;
    Vector2 mouse;
    float sensitvity = 10,
        moveSpeed = 0.4f,
        headRotationLimit = 90f;
    float forwardInput, lateralInput, verticalInput;

    public Material skybox, seabox;
    bool underwater;
    bool escaped = false, escapePressed = false;

    // Start is called before the first frame update
    void Start() {
        mouse = Vector2.zero;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        underwater = false;
        //skybox = Resources.Load<Material>("Resources/unity_builtin_extra");
        //seabox = Resources.Load<Material>("Seabox");
        
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");

        plantCount = GameObject.Find("Plant Count").GetComponent<TextMeshProUGUI>();
        herbCount = GameObject.Find("Herbivore Count").GetComponent<TextMeshProUGUI>();
        omniCount = GameObject.Find("Omnivore Count").GetComponent<TextMeshProUGUI>();

        InvokeRepeating("UpdateCounts", 0f, 5f);
    }

    // Update is called once per frame
    void Update() {
        if (!escaped) {
            // Look around
            mouse.x += Input.GetAxis("Mouse X") * sensitvity;
            mouse.y -= Input.GetAxis("Mouse Y") * sensitvity;

            mouse.y = Mathf.Clamp(mouse.y, -headRotationLimit, headRotationLimit);
            transform.localEulerAngles = new Vector3(mouse.y, 0, 0);
            player.localEulerAngles = new Vector3(0, mouse.x, 0);
            
            /*mouse.x = Input.GetAxis("Mouse X") * sensitvity;
            mouse.y -= Input.GetAxis("Mouse Y") * sensitvity;

            mouse.y = Mathf.Clamp(mouse.y, -headRotationLimit, headRotationLimit);
            player.Rotate(0f, mouse.x, 0f);
            transform.localEulerAngles = new Vector3(mouse.y, 0f, 0f);*/

            // Move!
            forwardInput = Input.GetAxis("Forward");
            lateralInput = Input.GetAxis("Lateral");
            verticalInput = Input.GetAxis("Vertical");

            if (forwardInput != 0 || lateralInput != 0 || verticalInput != 0) {
                /*Vector3 moveVec = transform.TransformDirection(new Vector3(forwardInput, 0, lateralInput));
                moveVec.y = verticalInput;
                player.position += moveSpeed * moveVec;*/
                /*Vector3 moveBy = transform.right * forwardInput +
                                transform.forward * lateralInput +
                                transform.up * verticalInput;
                body.MovePosition(transform.position + moveBy.normalized * moveSpeed);*/
                Vector3 moveBy = transform.right * forwardInput +
                                transform.forward * lateralInput +
                                transform.up * verticalInput;
                character.Move(moveBy * moveSpeed);// / Time.deltaTime);
            }

            // Underwater Visual Effect
            if(!underwater && transform.position.y < 0) {
                underwater = true;
                RenderSettings.fogDensity = 0.02f;
                RenderSettings.skybox = seabox;
                DynamicGI.UpdateEnvironment();
            } else if (underwater && transform.position.y > 0) {
                underwater = false;
                RenderSettings.fogDensity = 0f;
                RenderSettings.skybox = skybox;
                DynamicGI.UpdateEnvironment();     
            }
        }

        // Escape Control mode
        if (Input.GetKeyDown(KeyCode.Escape) && !escapePressed) {
            if (!escaped) {
                escaped = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            } else {
                escaped = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            escapePressed = true;
        }
        if (escapePressed && !Input.GetKeyDown(KeyCode.Escape)) {
            escapePressed = false;
        }
    }

    void UpdateCounts() {
        plantCount.text = plants.transform.childCount.ToString();
        int herb = 0, omni = 0;
        foreach (Transform child in fishes.transform) {
            if (child.GetComponent<FishController>().herbivorousness == 1) {
                herb++;
            } else {
                omni++;
            }
        }
        herbCount.text = herb.ToString();
        omniCount.text = omni.ToString();
    }
}

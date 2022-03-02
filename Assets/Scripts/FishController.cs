using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PIDtools;
using util;
using TMPro;

public class FishController : MonoBehaviour
{
    Rigidbody rb;
    GameObject plants, fishes, baby, eggs;
    TextMeshProUGUI panelTitle, panelLabels, panelValues;
    public Vector3 targetPosition;
    int tick, lifespan;
    float birth;
    float maxFood;
    public float reproduceTimer;
    float reproduceDelay;
    public float food, health, maxHealth;
    float standoff = 1f;
    public Vector3 idleTorque;

    public enum Behavior {idle, eat, predate};
    int retargetCounter = 0;
    public Behavior behavior;
    PID angleController, angVelController;
    static float yawCoefficient = 0.15f,
                 pitchCoefficient = 0.15f,
                 rollCoefficient = 0.18f;
    float speed, maxSpeed, maxTurn;
    Color scaleColor;
    Vector3 targetAngle, angleCorrection, angVelCorrection, torque;
    int sightDistance = 256; // actual distance is 16, but using squared dists

    public float size, sizeSqr, herbivorousness;

    public float axisX {
        get { return transform.rotation.eulerAngles.x; }
        set {
            Vector3 v = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(value, v.y, v.z);
        }
    }

    struct FoodItem {
        public AlgaeController plant;
        public FishController fish;

        /*public FoodItem(Vector3 p, AlgaeController a, FishController f) {
            position = p;
        }*/
    }
    FoodItem foodItem;

    void Awake() {
        gameObject.layer = 7; // Fish

        // References
        rb = gameObject.GetComponent<Rigidbody>();
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");
        eggs = GameObject.Find("Eggs");
        panelTitle = GameObject.Find("Info Panel Title").GetComponent<TextMeshProUGUI>();
        panelLabels = GameObject.Find("Info Panel Labels").GetComponent<TextMeshProUGUI>();
        panelValues = GameObject.Find("Info Panel Values").GetComponent<TextMeshProUGUI>();
        
        // Prefabs
        baby = Resources.Load("Prefabs/Egg") as GameObject;

        // Stats
        maxSpeed = 10;
        speed = 100; // Newtons per 0.2sec
        maxTurn = 8; // Newton meters per 0.2sec
        maxFood = 300;
        maxHealth = 10;

        Retarget();

        targetAngle = new Vector3();
        torque = Vector3.zero;

        idleTorque = Vector3.zero;

        behavior = Behavior.idle;
        reproduceTimer = Time.time - Random.Range(0, 30);
        reproduceDelay = 60f * Random.Range(0.9f, 1.1f);
        birth = Time.time;

        foodItem = new FoodItem();
    }

// Set attributes based on genetic features
    public void SetStats(float s, float h, Color color) {
        size = s;
        sizeSqr = Mathf.Pow(size, 2);
        transform.localScale *= size;
        float sizeMod = 0.5f+(size*0.5f);

        // Movement
        standoff = 0.5f + size*0.5f;
        speed *= sizeMod;
        maxSpeed *= sizeMod;
        maxTurn /= sizeMod;
        //yawCoefficient *= sizeMod;

        // Food
        sightDistance = Mathf.RoundToInt(sightDistance*sizeMod);
        herbivorousness = h;
        maxFood *= Mathf.Pow(size, 1.2f);
        food = maxFood/2;

        // Reproduction
        reproduceDelay *= Mathf.Pow(size,2);

        // Coloration
        scaleColor = color;
        transform.gameObject.GetComponent<MeshRenderer>().material.color = color;

        // Age
        lifespan = Mathf.RoundToInt(300 * size);
        Invoke("Die", lifespan);

        // Health
        maxHealth *= size;
        health = maxHealth;

        // Live
        // Act every 0.5 sec
        InvokeRepeating("Move", Random.Range(0.5f,1f), 0.5f);
    }

    private void FixedUpdate() {
        // Gravity above water
        if (transform.position.y > 0) {
            rb.AddForce(Vector3.up*-10);
        }
    }

    // Fish body driver, 2/s
    void Move() {
        // Manage Behavior changes,
        // Searching for a target can only happen 1/4 brain ticks
        if ((behavior == Behavior.eat && foodItem.plant == null) ||
            (behavior == Behavior.predate && foodItem.fish == null)) {
            Retarget();
        }
        if (behavior == Behavior.idle && food < maxFood*0.75f) {
            retargetCounter++;
            if (retargetCounter >= 4) {
                Retarget();
                retargetCounter = 0;
            }
        }

        // Go eat that thang!
        if (behavior == Behavior.eat || behavior == Behavior.predate) {
            if (behavior == Behavior.eat) {
                // Extremely rarely, a plant is eaten in the nanoseconds between
                // the previous check and now
                //if (foodItem.plant == null) { return; }
                targetPosition = foodItem.plant.transform.position;
            } else if (behavior == Behavior.predate) {
                //if (foodItem.fish == null) { return; }
                targetPosition = foodItem.fish.transform.position;
            }

            Vector3 targetVec = targetPosition - transform.position;

            // --------------------- //
            // -- Roll Correction -- //
            // --------------------- //
            //angleCorrection.x = angleController.Output(Mathf.DeltaAngle(axisX, 0), Time.fixedDeltaTime);
            //angVelCorrection.x = angVelController.Output(-rb.angularVelocity.x, Time.fixedDeltaTime); 
            angleCorrection.x = Mathf.DeltaAngle(axisX, 0) * rollCoefficient;

            // ------------------- //
            // -- Yaw Targeting -- //
            // -- ---------------- //
            float targetY = -Mathf.Atan2(targetVec.z, targetVec.x) * Mathf.Rad2Deg;
            targetAngle.y = targetY;
            float currentY = transform.rotation.eulerAngles.y;
            float deltaY = Mathf.DeltaAngle(currentY, targetY);
            //angleCorrection.y = angleController.Output(deltaY, Time.fixedDeltaTime);
            //angVelCorrection.y = angVelController.Output(-rb.angularVelocity.y, Time.fixedDeltaTime); 
            angleCorrection.y = Mathf.Clamp(deltaY * yawCoefficient, -maxTurn, maxTurn);

            // --------------------- //
            // -- Pitch Targeting -- //
            // -- ------------------ //
            // Note that positional deltaX & Z are used to calculate the
            // desired rotation on the Z-axis by the diagonal across an angled square
            float targetZ = Mathf.Atan2(targetVec.y, Mathf.Sqrt(Mathf.Pow(targetVec.x, 2)+Mathf.Pow(targetVec.z, 2)))*Mathf.Rad2Deg;
            if (behavior == Behavior.eat) { targetZ -= 5; } // Eat Algae from below
            float currentZ = transform.rotation.eulerAngles.z;
            float deltaZ = Mathf.DeltaAngle(currentZ, targetZ);
            //angleCorrection.z = angleController.Output(deltaZ, Time.fixedDeltaTime);
            //angVelCorrection.z = angVelController.Output(-rb.angularVelocity.z, Time.fixedDeltaTime); 
            angleCorrection.z = deltaZ * pitchCoefficient;

            // -------------------- //
            // -- Apply steering -- //
            // -- ----------------- //
            string s = "targetVec:   "+targetVec+"\n";
            s += "Yaw delta:   "+deltaY+"\n";
            s += "Pitch Delta: "+deltaZ+"\n";
            s += "Angle Correction:  "+angleCorrection+"\n";
            s += "AngVel Correction: "+angVelCorrection+"\n";
            rb.AddRelativeTorque(angleCorrection, ForceMode.VelocityChange);
            s += "Torque: "+angleCorrection+"\n";
            //Debug.Log(s);

            // Swimmy swimmy
            if (rb.velocity.magnitude < maxSpeed && targetVec.magnitude > standoff) {
                rb.AddForce(Vector3.Normalize(transform.right)*speed);
            } else if (targetVec.magnitude < standoff * 0.75f) {
                rb.AddForce(Vector3.Normalize(transform.right)*-0.5f*speed);
            }

            // ---------- //
            // -- food -- //
            // ---------- //
            if (Vector3.Distance(transform.position, targetPosition) <= standoff) {
                if (behavior == Behavior.eat) {
                    Eat();
                } else if (behavior == Behavior.predate) {
                    Bite();
                }
            }
        // Wander idly
        } else if (behavior == Behavior.idle) {
            // Swimmy swimmy
            rb.AddForce(Vector3.Normalize(transform.right)*speed*0.5f);
            if (transform.position.y > 0) {
                //rb.AddForce(Vector3.Normalize(transform.right)*-5f*speed, ForceMode.Acceleration);
                //rb.AddRelativeTorque(new Vector3(0, 1, 1) *5*speed);
            }
            
            // Yaw to wander side to side
            // The yaw is lightly bound to allow for rapid changes
            idleTorque.y += Random.Range(-sizeSqr, sizeSqr) - idleTorque.y/8;
            idleTorque.y = Mathf.Clamp(idleTorque.y, -sizeSqr, sizeSqr);

            // Slowly pitch to explore depth
            // Pitch strongly tends towards zero
            float deltaZ = Mathf.DeltaAngle(0, transform.eulerAngles.z);
            if (deltaZ > 180) { deltaZ -= 360; }
            idleTorque.z = Mathf.Clamp(idleTorque.z + Random.Range(-sizeSqr, sizeSqr)*0.2f - (deltaZ/90f)*size, -sizeSqr, sizeSqr);

            // Roll correction
            idleTorque.x = Mathf.DeltaAngle(axisX, 0) * rollCoefficient;

            rb.AddRelativeTorque(idleTorque);
        }

        // Get Hungry
        food -= size*2;
        if (food <= 0) {
            Die();
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == 3) {
            rb.AddForce(Vector3.Normalize(transform.right)*-speed, ForceMode.Acceleration);
        }
    }

    void DrawDebugStuff()
	{
		  //---------------------------------//
		 // - Debug Stuff (as advertised) - //
		//---------------------------------//
		Vector3 vectorToTarget = new Vector3(Mathf.Sin(targetAngle.y * Mathf.Deg2Rad), 0, Mathf.Cos(targetAngle.y * Mathf.Deg2Rad));//Vector3.Normalize(Target.transform.position - transform.position);
		Vector3 targetCrossForward = Vector3.Cross(vectorToTarget, transform.forward);
		float targetDotForward = Mathf.Clamp(Vector3.Dot(vectorToTarget, transform.forward), -1, 1);
        // Target vector is green
		Debug.DrawLine(transform.position, transform.position + vectorToTarget * 6, Color.green);
        // Forward is white
		Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.white);
        // Red is torque
		Debug.DrawLine(transform.position + transform.forward * 5, transform.position + transform.forward * 5 + transform.position + transform.right * torque.y/4, Color.red);
	}

    // Brain of Fish, Decides where to go
    void Retarget() {
         if (herbivorousness < 1) { // search for fishie
            foodItem.fish = null;
            float bestDist = Mathf.Infinity;
            // Dumb search over everything
            /*foreach (Transform child in fishes.transform) {
                FishController morsel = child.gameObject.GetComponent<FishController>();
                // Can't eat yoself
                if (morsel == this) { continue; }
                // Only eat fish appreciably smaller than you
                if (morsel.size > size * 0.8f) { continue; }
                float dist = (transform.position - child.position).sqrMagnitude;
                if (dist < bestDist/morsel.size && dist < sightDistance) {
                    bestDist = dist/morsel.size;
                    foodItem.fish = morsel;
                    targetPosition = child.position;
                }
            }*/
            // Collider based search
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Mathf.Sqrt(sightDistance), LayerMask.GetMask("Fish"));
            if (hitColliders.Length != 0) {
                foreach (Collider hit in hitColliders) {
                    FishController morsel = hit.gameObject.GetComponent<FishController>();
                    // Can't eat yoself
                    if (morsel == this) { continue; }
                    // Only eat fish appreciably smaller than you
                    if (morsel.size > size * 0.8f) { continue; }
                    float dist = (transform.position - hit.transform.position).sqrMagnitude;
                    if (dist < bestDist/morsel.size) {
                        bestDist = dist/morsel.size;
                        foodItem.fish = morsel;
                        targetPosition = hit.transform.position;
                    }
                }
            }

            // Check if fish is visible
            if (foodItem.fish != null) {
                Vector3 targetVec = foodItem.fish.transform.position-transform.position;
                if (!Physics.Raycast(transform.position, targetVec, targetVec.magnitude, LayerMask.GetMask("Terrain"))) {
                    behavior = Behavior.predate;
                    return;
                } else {
                    foodItem.fish = null;
                    Idle();
                }
            } else {
                Idle();
            }
        }
        // Herbivores, and starving omnivores
        if (Mathf.Min(food/maxFood, 1f) <= herbivorousness) { // Search for plant
            foodItem.plant = null;
            float bestDist = Mathf.Infinity;
            // Dumb search method
            /*foreach (Transform child in plants.transform) {
                float dist = (transform.position - child.position).sqrMagnitude;
                if (dist < bestDist && dist < sightDistance) {
                    bestDist = dist;
                    foodItem.plant = child.gameObject.GetComponent<AlgaeController>();
                    targetPosition = child.position;
                }
            }*/
            // Collider based search
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Mathf.Sqrt(sightDistance), LayerMask.GetMask("Plants"));
            if (hitColliders.Length != 0) {
                foreach (Collider hit in hitColliders) {
                    float dist = (transform.position - hit.transform.position).sqrMagnitude;
                    if (dist < bestDist) {
                        bestDist = dist;
                        foodItem.plant = hit.gameObject.GetComponent<AlgaeController>();
                        targetPosition = hit.transform.position;
                    }
                }
            }

            if (foodItem.plant != null) {
                Vector3 targetVec = foodItem.plant.transform.position-transform.position;
                if (!Physics.Raycast(transform.position, targetVec, targetVec.magnitude, LayerMask.GetMask("Terrain"))) {
                    behavior = Behavior.eat;
                } else {
                    foodItem.plant = null;
                    Idle();
                }
            } else {
                Idle();
            }
        }
    }

    void Eat() {
        food += 8 * size * herbivorousness;
        if (foodItem.plant.Eaten(size)) {
            Invoke("Retarget", 0.1f);
        }
        if (food > maxFood) {
            Idle();
            idleTorque.y = Random.Range(-2*sizeSqr, 2*sizeSqr);
            rb.AddForce(Vector3.Normalize(transform.right)*-1f*speed, ForceMode.Acceleration);
            if (Time.time - reproduceTimer > reproduceDelay) {
                Reproduce();
            }
        }
    }

    void Bite() {
        if (foodItem.fish.Bitten(size*10)) {
            food += foodItem.fish.size * 800;
            if (food > maxFood) {
                Idle();
                idleTorque.y = Random.Range(-2*sizeSqr, 2*sizeSqr);
                if (Time.time - reproduceTimer > reproduceDelay) {
                    Reproduce();
                }
            } else {
                Retarget();
            }
        }
    }

    public bool Bitten(float dmg) {
        health -= dmg;
        if (health <= 0) {
            Die();
            return true;
        }
        return false;
    }

    void Idle() {
        behavior = Behavior.idle;
    }

    void Reproduce() {
        // Evolve new stats
        float newSize = size;
        if (Random.Range(0,100) < 100) { // 100% chance of mutation
            newSize *= Random.Range(0.90f, 1.10f); // max 10% variance
        }
        Color newColor = scaleColor;
        if (Random.Range(0,1000) <= 1) { // 0.1% of color mutation
            newColor.r = Mathf.Clamp01(newColor.r + Random.Range(-0.5f, 0.5f));
            newColor.g = Mathf.Clamp01(newColor.g + Random.Range(-0.5f, 0.5f));
            newColor.b = Mathf.Clamp01(newColor.b + Random.Range(-0.5f, 0.5f));
        }
        food -= maxFood/2;
        reproduceTimer = Time.time;
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"));
        Vector3 pos = hit.point;
        pos.y += 0.05f*newSize;
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            eggs.transform);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<EggController>().SetStats(newSize, herbivorousness, newColor);
    }

    void Die() {
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
        labels += "HP:\n"; values += Mathf.RoundToInt(100*health/maxHealth)+"%\n";
        labels += "Food:\n"; values += Mathf.RoundToInt(100*food/maxFood)+"%\n";
        labels += "Egg:\n";
        int timeToEgg = Mathf.RoundToInt(reproduceDelay - (Time.time - reproduceTimer));
        if (timeToEgg > 0) {
            values += timeToEgg+"s\n";
        } else {
            values += "0s\n";
        }
        labels += "Age:\n"; values += Mathf.RoundToInt(Time.time - birth)+"\n";
        panelLabels.text = labels;
        panelValues.text = values;
    }
}

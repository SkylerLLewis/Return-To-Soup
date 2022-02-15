using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PIDtools;
using util;

public class FishController : MonoBehaviour
{
    Rigidbody rb;
    GameObject plants, fishes, baby;
    public Vector3 targetPosition;
    int tick;
    float maxFood = 200;
    public float reproduceTimer;
    float reproduceDelay;
    public float food, health;
    float standoff = 1f;
    public Vector3 idleTorque;

    public enum Behavior {idle, eat, predate};
    public Behavior behavior;
    PID angleController, angVelController;
    static float yawCoefficient = 0.1f,
                 pitchCoefficient = 0.1f,
                 rollCoefficient = 0.12f;
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
        tick = Random.Range(0, 9);

        rb = gameObject.GetComponent<Rigidbody>();
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");
        behavior = Behavior.idle;
        
        // Prefabs
        baby = Resources.Load("Prefabs/Fish") as GameObject;

        // Stats
        maxSpeed = 4;
        speed = 40; // Newtons per 0.2sec
        maxTurn = 2; // Newton meters per 0.2sec

        Retarget();

        targetAngle = new Vector3();
        torque = Vector3.zero;

        idleTorque = Vector3.zero;

        reproduceTimer = Time.time - Random.Range(0, 30);
        reproduceDelay = 90f * Random.Range(0.9f, 1.1f);

        foodItem = new FoodItem();
    }

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
        reproduceDelay *= sizeMod;

        // Coloration
        scaleColor = color;
        transform.gameObject.GetComponent<MeshRenderer>().material.color = color;
    }

    void Update() {
        //DrawDebugStuff();
    }

    private void FixedUpdate() {
        //axisX = 0;
        // Gravity above water
        if (transform.position.y > 0) {
            rb.AddForce(Vector3.up*-10);
        }

        // Brain ticks at 5 times per second 
        if (tick == 10) {
            // Manage Behavior changes
            // * These do need to happen before decisions are made
            if (behavior == Behavior.idle && food < maxFood*0.75f) {
                Retarget();
            }
            if (behavior == Behavior.eat && foodItem.plant == null) {
                Retarget();
            }
            if (behavior == Behavior.predate && foodItem.fish == null) {
                Retarget();
            }

            // Go eat that thang!
            if (behavior == Behavior.eat || behavior == Behavior.predate) {
                if (behavior == Behavior.eat) {
                    targetPosition = foodItem.plant.transform.position;
                    //targetPosition.y -= 0.2f;
                } else {
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
                if (rb.velocity.magnitude < maxSpeed && targetVec.magnitude > standoff && Mathf.Abs(deltaY) < 90) {
                    rb.AddForce(Vector3.Normalize(transform.right)*speed);
                } else if (targetVec.magnitude < standoff * 0.75f) {
                    rb.AddForce(Vector3.Normalize(transform.right)*-1.5f*speed);
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
                    rb.AddForce(Vector3.Normalize(transform.right)*-5f*speed, ForceMode.Acceleration);
                }
                
                // Yaw to wander side to side
                // The yaw is lightly bound to allow for rapid changes
                idleTorque.y += Random.Range(-sizeSqr, sizeSqr) - idleTorque.y/8;
                idleTorque.y = Mathf.Clamp(idleTorque.y, -sizeSqr, sizeSqr);

                // Slowly pitch to explore depth
                // Pitch strongly tends towards zero
                float deltaZ = Mathf.DeltaAngle(0, transform.eulerAngles.z);
                if (deltaZ > 180) { deltaZ -= 360; }
                idleTorque.z = Mathf.Clamp(idleTorque.z + Random.Range(-sizeSqr, sizeSqr)*0.1f - (deltaZ/90f)*size, -size, size);

                // Roll correction
                idleTorque.x = Mathf.DeltaAngle(axisX, 0) * rollCoefficient;

                rb.AddRelativeTorque(idleTorque);
            }

            // Get Hungry
            food -= size;
            if (food <= 0) {
                Die();
            }
            tick = 0;
        }
        tick++;

    }

    /*private void OnCollisionEnter(Collision other) {
        if (behavior == Behavior.idle) {
            idleTorque.y *= 2;
        }
    }*/

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

    void Retarget() {
        // Herbivores, and starving omnivores
        if (herbivorousness > food/maxFood) { // Search for plant
            foodItem.plant = null;
            float bestDist = Mathf.Infinity;
            foreach (Transform child in plants.transform) {
                float dist = (transform.position - child.position).sqrMagnitude;
                if (dist < bestDist && dist < sightDistance) {
                    bestDist = dist;
                    foodItem.plant = child.gameObject.GetComponent<AlgaeController>();
                    targetPosition = child.position;
                }
            }
            if (foodItem.plant != null) {
                Vector3 targetVec = foodItem.plant.transform.position-transform.position;
                if (!Physics.Raycast(transform.position, targetVec, targetVec.magnitude, LayerMask.GetMask("Terrain"))) {
                    behavior = Behavior.eat;
                }
            } else {
                Idle();
            }
        } else { // search for fishie
            foodItem.fish = null;
            float bestDist = Mathf.Infinity;
            foreach (Transform child in fishes.transform) {
                FishController morsel = child.gameObject.GetComponent<FishController>();
                // Only eat fish appreciably smaller than you
                if (morsel.size > size * 0.8f) { continue; }
                float dist = (transform.position - child.position).sqrMagnitude;
                if (dist < bestDist && dist < sightDistance) {
                    bestDist = dist;
                    foodItem.fish = morsel;
                    targetPosition = child.position;
                }
            }
            if (foodItem.fish != null) {
                Vector3 targetVec = foodItem.fish.transform.position-transform.position;
                if (!Physics.Raycast(transform.position, targetVec, targetVec.magnitude, LayerMask.GetMask("Terrain"))) {
                    behavior = Behavior.predate;
                }
            } else {
                Idle();
            }
        }
    }

    void Eat() {
        food += 4 * size;
        if (foodItem.plant.Eaten(size)) { foodItem.plant = null; }
        if (food > maxFood) {
            Idle();
            idleTorque.y = Random.Range(-2, 2);
            rb.AddForce(Vector3.Normalize(transform.right)*-2.5f*speed, ForceMode.Acceleration);
            if (Time.time - reproduceTimer > reproduceDelay) {
                Reproduce();
            }
        }
    }

    void Bite() {
        if (foodItem.fish.Bitten(size*10)) {
            food += foodItem.fish.size * 400;
            if (food > maxFood) {
                Idle();
                idleTorque.y = Random.Range(-2, 2);
                rb.AddForce(Vector3.Normalize(transform.right)*-2.5f*speed, ForceMode.Acceleration);
                if (Time.time - reproduceTimer > reproduceDelay) {
                    Reproduce();
                }
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
        Vector3 pos = transform.position;
        pos.x += Random.Range(-1f, 1f);
        pos.z += Random.Range(-1f, 1f);
        pos.y -= 0.2f;
        GameObject clone = Instantiate(
            baby,
            pos,
            Quaternion.identity,
            fishes.transform);
        clone.name = clone.name.Split('(')[0];
        food = maxFood/2;
        reproduceTimer = Time.time;
        clone.GetComponent<FishController>().SetStats(size, herbivorousness, scaleColor);
    }

    void Die() {
        Destroy(transform.gameObject);
    }
}

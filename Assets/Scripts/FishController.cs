using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PIDtools;
using util;

public class FishController : MonoBehaviour
{
    Rigidbody rb;
    GameObject plants, fishes, baby;
    AlgaeController foodItem;
    public Vector3 targetPosition;
    int tick,
        maxFood = 200;
    public float reproduceTimer;
    float reproduceDelay;
    public int food;
    float standoff = 1f;
    public Vector3 idleTorque;

    public enum Behavior {idle, eat};
    public Behavior behavior;
    PID angleController, angVelController;
    static float yawCoefficient = 0.1f,
                 pitchCoefficient = 0.1f,
                 rollCoefficient = 0.12f;
    Vector3 targetAngle, angleCorrection, angVelCorrection, torque;
    int sightDistance = 256; // actual distance is 16, but using squared dists

    public float axisX {
        get { return transform.rotation.eulerAngles.x; }
        set {
            Vector3 v = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(value, v.y, v.z);
        }
    }

    void Start() {
        tick = Random.Range(0, 9);

        rb = gameObject.GetComponent<Rigidbody>();
        plants = GameObject.Find("Plants");
        fishes = GameObject.Find("Fishes");
        behavior = Behavior.idle;
        
        // Prefabs
        baby = Resources.Load("Prefabs/Fish") as GameObject;

        Retarget();

        // The angle controller drives the ship's angle towards the target angle.
		// This PID controller takes in the error between the ship's rotation angle 
		// and the target angle as input, and returns a signed torque magnitude.
        angleController = new PID(0.1f, 0.001f, 0.0005f);
        // The angular veloicty controller drives the object's angular velocity to 0.
		// This PID controller takes in the negated angular velocity of the object, 
		// and returns a signed torque magnitude.
        angVelController = new PID(0.5f, 0, 0.001f);
        targetAngle = new Vector3();
        torque = Vector3.zero;

        idleTorque = Vector3.zero;

        reproduceTimer = Time.time - Random.Range(0, 30);
        reproduceDelay = 90f * Random.Range(0.9f, 1.1f);


        food = maxFood/2;
    }

    void Update() {
        //DrawDebugStuff();
    }

    private void FixedUpdate() {
        //axisX = 0;
        // Gravity above water
        if (transform.position.y > 0) {
            rb.AddForce(Vector3.up*-10);
            if (behavior == Behavior.eat) {
                //rb.AddForce(Vector3.Normalize(transform.right)*-20);
            }
        }

        // Brain ticks at 5 times per second 
        if (tick == 10) {
            // Manage Behavior changes
            // * These do need to happen before decisions are made
            if (behavior == Behavior.idle && food < maxFood*0.8f) {
                Retarget();
            }
            if (behavior == Behavior.eat && foodItem == null) {
                Retarget();
            }

            // Go eat that thang!
            if (behavior == Behavior.eat) {
                targetPosition = foodItem.transform.position;

                Vector3 targetVec = targetPosition - transform.position;

                // Swimmy swimmy
                if (rb.velocity.magnitude < 4 && targetVec.magnitude > standoff) {
                    rb.AddForce(Vector3.Normalize(transform.right)*40);
                } else if (targetVec.magnitude < standoff * 0.75f) {
                    rb.AddForce(Vector3.Normalize(transform.right)*-50);
                }

                  // --------------------- //
                 // -- Roll Correction -- //
                // --------------------- //
                //angleCorrection.x = angleController.Output(Mathf.DeltaAngle(axisX, 0), Time.fixedDeltaTime);
                //angVelCorrection.x = angVelController.Output(-rb.angularVelocity.x, Time.fixedDeltaTime); 
                angleCorrection.x = Mathf.DeltaAngle(axisX, 0) * rollCoefficient;

                  // ------------------- //
                 // -- Yaw Targeting -- //
                // -- ---------------- //
                float targetY = -Mathf.Atan2(targetVec.z, targetVec.x) * Mathf.Rad2Deg; // Negative to actick for top-down perspective
                targetAngle.y = targetY;
                float currentY = transform.rotation.eulerAngles.y;
                float deltaY = Mathf.DeltaAngle(currentY, targetY);
                //angleCorrection.y = angleController.Output(deltaY, Time.fixedDeltaTime);
                //angVelCorrection.y = angVelController.Output(-rb.angularVelocity.y, Time.fixedDeltaTime); 
                angleCorrection.y = deltaY * yawCoefficient;

                  // --------------------- //
                 // -- Pitch Targeting -- //
                // -- ------------------ //
                // Note that positional deltaX & Z are used to calculate the
                // desired rotation on the Z-axis by the diagonal across an angled square
                float targetZ = Mathf.Atan2(targetVec.y, Mathf.Sqrt(Mathf.Pow(targetVec.x, 2)+Mathf.Pow(targetVec.z, 2)))*Mathf.Rad2Deg;
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

                  // ---------- //
                 // -- food -- //
                // ---------- //
                if (behavior == Behavior.eat && Vector3.Distance(transform.position, targetPosition) <= standoff) {
                    if (foodItem.Eaten()) { foodItem = null; }
                    Eat();
                }
            // Wander idly
            } else if (behavior == Behavior.idle) {
                // Swimmy swimmy
                rb.AddForce(Vector3.Normalize(transform.right)*30);
                if (transform.position.y > 0) {
                    rb.AddForce(Vector3.Normalize(transform.right)*-100);
                }
                
                // Yaw to wander side to side
                // The yaw is lightly bound to allow for rapid changes
                idleTorque.y += Random.Range(-1f, 1f) - idleTorque.y/8;

                // Slowly pitch to explore depth
                // Pitch strongly tends towards zero
                float deltaZ = Mathf.DeltaAngle(0, transform.eulerAngles.z);
                if (deltaZ > 180) { deltaZ -= 360; }
                idleTorque.z = Mathf.Clamp(idleTorque.z + Random.Range(-0.1f, 0.1f) - deltaZ/90f, -1f, 1f);

                // Roll correction
                idleTorque.x = angleController.Output(Mathf.DeltaAngle(axisX, 0), Time.fixedDeltaTime);

                rb.AddRelativeTorque(idleTorque);
            }

            // Get Hungry
            food--;
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
        foodItem = null;
        float bestDist = Mathf.Infinity;
        foreach (Transform child in plants.transform) {
            float dist = (transform.position - child.position).sqrMagnitude;
            if (dist < bestDist && dist < sightDistance) {
                bestDist = dist;
                foodItem = child.gameObject.GetComponent<AlgaeController>();
                targetPosition = child.position;
            }
        }
        if (foodItem != null) {
            Vector3 targetVec = foodItem.transform.position-transform.position;
            if (!Physics.Raycast(transform.position, targetVec, targetVec.magnitude, LayerMask.GetMask("Terrain"))) {
                behavior = Behavior.eat;
            }
        } else {
            Idle();
        }
    }

    void Eat() {
        food += 5;
        if (food > maxFood) {
            food = maxFood;
            Idle();
            idleTorque.y = Random.Range(-2, 2);
            rb.AddForce(Vector3.Normalize(transform.right)*-100, ForceMode.Acceleration);
            if (Time.time - reproduceTimer > reproduceDelay) {
                Reproduce();
            }
        }
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
    }

    void Die() {
        Destroy(transform.gameObject);
    }
}

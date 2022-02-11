using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishControllerOLD : MonoBehaviour
{
    Rigidbody rb;
    GameObject plants;
    AlgaeController foodItem;
    public Vector3 targetPosition;
    int tick = 0,
        maxFood = 100;
    public int food = 100;
    float standoff = 1f;

    public enum Behavior {idle, eat};
    public Behavior behavior;

    public float axisX {
        get { return transform.rotation.eulerAngles.x; }
        set {
            Vector3 v = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(value, v.y, v.z);
        }
    }

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        plants = GameObject.Find("Plants");
        behavior = Behavior.idle;
        Retarget();

        //Time.timeScale = 0.2f;
    }

    void Update() {
        
    }

    private void FixedUpdate() {
        // Gravity above water
        if (transform.position.y > 0) {
            rb.AddForce(Vector3.up*-10);
        }

        // Brain ticks at 5 times per second 
        if (tick == 9) {
            // Manage Behavior changes
            if (behavior == Behavior.idle && food < maxFood*0.75f) {
                Retarget();
            }
            if (behavior == Behavior.eat && foodItem == null) {
                Retarget();
            }
            if (behavior == Behavior.eat) {
                targetPosition = foodItem.transform.position;
            }
            // Only move if you have a target
            if (targetPosition != null) {

                Vector3 targetVec = targetPosition - transform.position;

                // Swimmy swimmy
                if (rb.velocity.magnitude < 5 && targetVec.magnitude > standoff) {
                    rb.AddForce(Vector3.Normalize(transform.right)*50);
                }

                // ------------------- //
                // -- Yaw Targeting -- //
                // -- ---------------- //
                string s = "";
                s += "targetVec: "+targetVec+"\n";
                float targetY = -Mathf.Atan2(targetVec.z, targetVec.x); // Negative to actick for top-down perspective
                s += "targetY (rad): "+targetY+"\n";
                s += "targetY (deg): "+targetY*180/Mathf.PI+"\n";
                float currentY = transform.rotation.eulerAngles.y;
                s += "current (deg): "+currentY+"\n";
                float deltaY = currentY - targetY*180/Mathf.PI;
                if (deltaY > 180) {
                    deltaY -= 360;
                } else if (deltaY < -180) {
                    deltaY += 360;
                }
                s += "deltaY (deg):  "+deltaY+"\n";
                //Debug.Log(s);
                if (Mathf.Abs(deltaY) > 0.3f) {
                    rb.AddRelativeTorque(new Vector3(0, (-0.2f*deltaY), 0));// - rb.angularVelocity.y, 0));
                }

                // --------------------- //
                // -- Pitch Targeting -- //
                // -- ------------------ //
                // Note that positional deltaX is used to calculate the
                // desired rotation on the Z-axis
                s = "";
                s += "targetVec: "+targetVec+"\n";
                float targetZ = Mathf.Atan2(targetVec.y, Mathf.Sqrt(Mathf.Pow(targetVec.x, 2)+Mathf.Pow(targetVec.z, 2)));
                //if (targetVec.x < 0) { targetZ = Mathf.PI - targetZ; } // don't flip over
                s += "targetZ (rad): "+targetZ+"\n";
                s += "targetZ (deg): "+targetZ*180/Mathf.PI+"\n";
                float currentZ = transform.rotation.eulerAngles.z;
                s += "current (deg): "+currentZ+"\n";
                float deltaZ = currentZ - targetZ*180/Mathf.PI;
                if (deltaZ > 180) {
                    deltaZ -= 360;
                } else if (deltaZ < -180) {
                    deltaZ += 360;
                }
                s += "deltaZ (deg):  "+deltaZ+"\n";
                if (Mathf.Abs(deltaY) < 90) { // Do not pitch behind
                    //Debug.Log(s);
                    if (Mathf.Abs(deltaZ) > 0.3f) { // pitch towards object
                        rb.AddRelativeTorque(new Vector3(0, 0, (-0.1f*deltaZ) - rb.angularVelocity.z));
                    }
                }

                // --------------------- //
                // -- Roll Correction -- //
                // --------------------- //
                if (Mathf.Abs(axisX) > 30) {
                    rb.AddRelativeTorque(new Vector3(-0.05f*axisX, 0, 0));
                }

                // ---------- //
                // -- food -- //
                // ---------- //
                if (behavior == Behavior.eat && Vector3.Distance(transform.position, targetPosition) <= standoff) {
                    if (foodItem.Eaten()) { foodItem = null; }
                    Eat();
                }
            }

            // Get Hungry
            food--;
            if (food <= 0) {
                Die();
            }

            tick = 0;
        } else {
            tick++;
        }

    }

    void Retarget() {
        float bestDist = 100000000000;
        foreach (Transform child in plants.transform) {
            if (Vector3.Distance(child.position, transform.position) < bestDist) {
                bestDist = Vector3.Distance(child.position, transform.position);
                foodItem = child.gameObject.GetComponent<AlgaeController>();
                targetPosition = child.position;
            }
        }
        if (foodItem != null) {
            behavior = Behavior.eat;
        } else {
            behavior = Behavior.idle;
        }
    }

    void Eat() {
        food += 10;
        if (food > maxFood) {
            food = maxFood;
            behavior = Behavior.idle;
            targetPosition = transform.position;
            targetPosition.x += Random.Range(-2,2);
            targetPosition.z += Random.Range(-2,2);
            targetPosition.y += Random.Range(-3,1);
        }
    }

    void Die() {
        tick = -10000;
        Destroy(transform.gameObject, 2);
    }
}

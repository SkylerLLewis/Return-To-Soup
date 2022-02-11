using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace util {
    public class Utilities {
        public static void TraceLine(Vector3 origin, Vector3 terminus, Color color) {
            GameObject line = new GameObject("Trace");
            LineRenderer l = line.AddComponent<LineRenderer>();
            l.startWidth = 0.2f;
            l.endWidth = 0.2f;
            l.SetPositions(new Vector3[2] {origin, terminus});
            l.material = new Material(Shader.Find("Mobile/Particles/Additive"));
            l.material.color = color;
            l.startColor = color;
            l.endColor = color;
        }

    }
}
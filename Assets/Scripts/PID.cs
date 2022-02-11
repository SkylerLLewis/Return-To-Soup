// -- NOTES FROM THE SHIP MASTA -- //
// Here are some sets of gains for the two PID controllers which 
// impart different control behaviors to the ship.
//
// -----------------------------+
//   Critically Damped Controls*
// -----------------------------+
// Angle Controller:
// Kp = 9.244681
// Ki = 0
// Kd = 0.06382979
// 
// Angular Velocity Controller:
// Kp = 33.7766
// Ki = 0
// Kd = 0.2553191
// 
// *Or, at least this is close to critically damped. 
//  It's very difficult to remove all oscillation.
//
//
// -----------------------------+
//     Fast Controls
// -----------------------------+
// Angle Controller:
// Kp = 33.51064
// Ki = 0
// Kd = 0.02127661
// 
// Angular Velocity Controller:
// Kp = 46.54256
// Ki = 0
// Kd = 0.1808511
//
//
// -----------------------------+
//     Spongy Controls
// -----------------------------+
// Angle Controller:
// Kp = 0.7093059
// Ki = 0
// Kd = 0
//
// Angular Velocity Controller:
// Kp = 11.17021
// Ki = 0
// Kd = 0
//
// 
// -----------------------------+
//     Springy Controls
// -----------------------------+
// Angle Controller:
// Kp = 0.7093059
// Ki = 0
// Kd = 0.1648936
//
// Angular Velocity Controller:
// Kp = 0
// Ki = 0
// Kd = 0
//

namespace PIDtools {

    public class PID {
        private float Kp {get; set;}
        private float Ki {get; set;}
        private float Kd {get; set;}
        private float prevError=0, I=0;

        public PID(float kp, float ki, float kd) {
            Kp = kp;
            Ki = ki;
            Kd = kd;

        }

        public float Output(float error, float deltaTime) {
            float P = error;
            I += P * deltaTime;
            float D = (P - prevError) / deltaTime;
            prevError = error;
            
            return P*Kp + I*Ki + D*Kd;
        }

    }
}
using System;
using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa
{
    [Serializable]
    public sealed class TravelSnapshot
    {
        public Vector3 Position;
        public float Yaw, Pitch, VerticalSpeed;
        public bool Riding;
        public Vector3 ScooterPosition, ScooterVelocity, ScooterAngularVelocity;
        public Quaternion ScooterRotation;
        public int CollisionCount;
        public float CollisionCooldown;
    }

    [Serializable]
    public sealed class SaveGame
    {
        public int Version = 1;
        public string World = "student-morning-v1";
        public string SavedUtc;
        public MorningSnapshot Simulation;
        public TravelSnapshot Travel;
        public float MinuteFraction, RealSecondsPerMinute;
        public bool Started;
        public int ReadMessages;

        public void Validate()
        {
            if (Version != 1 || World != "student-morning-v1") throw new ArgumentException("Unsupported save version/world.");
            var sim = MorningSimulation.Restore(Simulation);
            bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
            bool VectorValid(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && value.sqrMagnitude < 1000000;
            if (Travel == null || !VectorValid(Travel.Position) || !VectorValid(Travel.ScooterPosition) ||
                !VectorValid(Travel.ScooterVelocity) || !VectorValid(Travel.ScooterAngularVelocity) ||
                !Finite(Travel.Yaw) || !Finite(Travel.Pitch) || Math.Abs(Travel.Pitch) > 65.1f || !Finite(Travel.VerticalSpeed) ||
                !Finite(MinuteFraction) || MinuteFraction < 0 || MinuteFraction >= 1 ||
                !Finite(RealSecondsPerMinute) || RealSecondsPerMinute < 0.1f || RealSecondsPerMinute > 3600 ||
                !Finite(Travel.CollisionCooldown) || Travel.CollisionCooldown < 0 || Travel.CollisionCooldown > 2.01f ||
                Travel.CollisionCount < 0 || ReadMessages < 0 || ReadMessages > Simulation.Journal.Length ||
                (Travel.Riding && sim.Activity != MorningActivity.None))
                throw new ArgumentException("Invalid world snapshot.");
            var q = Travel.ScooterRotation;
            float norm = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
            if (!Finite(norm) || Math.Abs(norm - 1) > 0.01f) throw new ArgumentException("Invalid vehicle rotation.");
        }
    }
}

using UnityEngine;
using System;

namespace UnityChan
{
    public class SpringManager : MonoBehaviour
    {
        public SpringBone[] springBones;

        private float DeltaTime => Time.deltaTime > 0f ? Time.deltaTime : 1f / FPS;
        private int FPS => Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;

        void LateUpdate()
        {
            Array.ForEach(springBones, bone => bone.UpdateSpring(DeltaTime));
        }
    }
}
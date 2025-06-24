using UnityEngine;
using UnityEngine.Serialization;

namespace AngelBeat
{
    public class FloatingUI : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.up * 2f;

        void LateUpdate()
        {
            if (!target) return;
            transform.position = target.position + offset;
            transform.LookAt(Camera.main.transform); // 항상 카메라 바라보게
        }
    }
}
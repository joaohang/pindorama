using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Câmera simples de seguimento (ortográfica). Substituível por Cinemachine.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private float orthographicSize = 9.5f;

        private Vector3 velocity;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        private void Start()
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}

using UnityEngine;

public class FogFollower : MonoBehaviour
{
    [SerializeField] private Transform target; // player or camera
    [SerializeField] private float height = 0.03f;
    [SerializeField] private bool followRotationY = false;

    void LateUpdate()
    {
        if (!target) return;

        var p = transform.position;
        p.x = target.position.x;
        p.z = target.position.z;
        p.y = height;
        transform.position = p;

        if (followRotationY)
        {
            var e = transform.eulerAngles;
            e.y = target.eulerAngles.y;
            transform.eulerAngles = e;
        }
    }
}

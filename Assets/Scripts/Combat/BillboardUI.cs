using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Guarda direttamente verso la camera invece di usare il forward
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180f, 0);
    }
}
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera cam;

    // Recupera il riferimento alla camera principale
    void Start()
    {
        cam = Camera.main;
    }

    // Ruota il canvas verso la camera ogni frame per tenerlo sempre leggibile
    void LateUpdate()
    {
        if (cam == null) return;

        // Guarda direttamente verso la camera invece di usare il forward
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180f, 0);
    }
}
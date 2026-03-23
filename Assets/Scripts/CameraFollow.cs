using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 3f;
    public float rotationSpeed = 3f;
    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float zoomSpeed = 1f;
    public float velocitaTransizione = 5f;

    private float currentYaw = 0f;
    private float currentPitch = 15f;

    // --- SISTEMA INTERAZIONE ---
    private bool inModalitaInterazione = false;
    private Transform targetInterazione;
    private Vector3 offsetInterazione;

    void LateUpdate()
    {
        if (target == null) return;

        // Se in modalità interazione, muovi gradualmente la camera verso l'oggetto
        if (inModalitaInterazione && targetInterazione != null)
        {
            Vector3 posizioneDesiderata = targetInterazione.position + offsetInterazione;

            // Sposta gradualmente la camera verso la posizione desiderata
            transform.position = Vector3.Lerp(
                transform.position,
                posizioneDesiderata,
                Time.deltaTime * velocitaTransizione
            );

            // Ruota gradualmente la camera verso l'oggetto
            Quaternion rotazioneDesiderata = Quaternion.LookRotation(
                targetInterazione.position - transform.position
            );
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                rotazioneDesiderata,
                Time.deltaTime * velocitaTransizione
            );

            return;
        }

        // Ruota la telecamera con il tasto destro del mouse
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentYaw += (mouseDelta.x / 10f) * rotationSpeed;
            currentPitch -= (mouseDelta.y / 10f) * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, -10f, 70f);
        }

        // Zoom con la rotellina del mouse
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed * 0.01f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Calcola la posizione della telecamera
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        transform.position = target.position + Vector3.up * height + offset;
        transform.LookAt(target.position + Vector3.up);
    }

    public void ImpostaTargetInterazione(Transform oggetto, Vector3 offset)
    {
        inModalitaInterazione = true;
        targetInterazione = oggetto;
        offsetInterazione = offset;
    }

    public void RipristinaCamera()
    {
        inModalitaInterazione = false;
        targetInterazione = null;
    }

    // Restituisce la direzione in avanti della telecamera (ignorando l'asse Y)
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
}
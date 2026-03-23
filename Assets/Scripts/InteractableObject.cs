using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObject : MonoBehaviour
{
    // Raggio entro cui il giocatore può interagire
    public float raggioInterazione = 3f;

    // Offset laterale della camera rispetto all'oggetto
    public Vector3 offsetCamera = new Vector3(3f, 2f, -3f);

    private Transform giocatore;
    private CameraFollow cameraFollow;
    private bool inInterazione = false;

    void Start()
    {
        // Trova automaticamente il giocatore e la camera
        giocatore = GameObject.FindWithTag("Player").transform;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        if (giocatore == null || cameraFollow == null) return;

        float distanza = Vector3.Distance(transform.position, giocatore.position);

        // Se il giocatore è vicino e preme E
        if (distanza <= raggioInterazione && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!inInterazione)
                IniziaInterazione();
            else
                TerminaInterazione();
        }
    }

    void IniziaInterazione()
    {
        inInterazione = true;
        cameraFollow.ImpostaTargetInterazione(transform, offsetCamera);
        Debug.Log("Interazione iniziata con: " + gameObject.name);
    }

    void TerminaInterazione()
    {
        inInterazione = false;
        cameraFollow.RipristinaCamera();
        Debug.Log("Interazione terminata");
    }

    // Mostra il raggio di interazione nell'editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raggioInterazione);
    }
}
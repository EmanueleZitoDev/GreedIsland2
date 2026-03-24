using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("Interazione")]
    public float raggioInterazione = 5f;

    [Header("Combattimento")]
    public float distanzaCombattimento = 3f;
    public Vector3 offsetCamera = new Vector3(-4f, 2f, -3f);

    private Transform giocatore;
    private CameraFollow cameraFollow;
    private MonoBehaviour controllerGiocatore;
    private bool inInterazione = false;
    private bool inMovimentoVersoMostro = false;

    void Start()
    {
        giocatore = GameObject.FindWithTag("Player").transform;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();

        // Cerca il controller del giocatore — prova prima con il namespace
        controllerGiocatore = giocatore.GetComponent<StarterAssets.ThirdPersonController>();

        // Se non lo trova, prova senza namespace
        if (controllerGiocatore == null)
            controllerGiocatore = giocatore.GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (giocatore == null || cameraFollow == null) return;

        // Ignora input mentre il giocatore si sta muovendo verso il mostro
        if (inMovimentoVersoMostro) return;

        float distanza = Vector3.Distance(transform.position, giocatore.position);

        if (distanza <= raggioInterazione && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!inInterazione)
                StartCoroutine(IniziaInterazione());
            else
                TerminaInterazione();
        }
    }

    System.Collections.IEnumerator IniziaInterazione()
    {
        inMovimentoVersoMostro = true;

        // Blocca il controller mentre gestiamo il movimento manualmente
        if (controllerGiocatore != null)
            controllerGiocatore.enabled = false;

        // Calcola la posizione di destinazione — di fronte al mostro a distanzaCombattimento
        Vector3 direzione = (giocatore.position - transform.position).normalized;
        Vector3 destinazione = transform.position + direzione * distanzaCombattimento;
        destinazione.y = giocatore.position.y; // mantieni la stessa altezza

        // Gira il giocatore verso il mostro
        giocatore.LookAt(new Vector3(transform.position.x, giocatore.position.y, transform.position.z));

        // Muovi gradualmente il giocatore verso la destinazione
        float velocitaMovimento = 4f;
        while (Vector3.Distance(giocatore.position, destinazione) > 0.1f)
        {
            giocatore.position = Vector3.MoveTowards(
                giocatore.position,
                destinazione,
                velocitaMovimento * Time.deltaTime
            );
            yield return null;
        }

        // Posizione raggiunta — blocca tutto e attiva la camera di combattimento
        giocatore.position = destinazione;
        inMovimentoVersoMostro = false;
        inInterazione = true;

        cameraFollow.ImpostaTargetInterazione(transform, offsetCamera);

        Debug.Log("Combattimento iniziato con: " + gameObject.name);
    }

    void TerminaInterazione()
    {
        inInterazione = false;
        cameraFollow.RipristinaCamera();

        // Riattiva il movimento del giocatore
        if (controllerGiocatore != null)
            controllerGiocatore.enabled = true;

        Debug.Log("Combattimento terminato");
    }

    void OnDrawGizmosSelected()
    {
        // Cerchio giallo — raggio di interazione
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raggioInterazione);

        // Cerchio rosso — distanza di combattimento
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanzaCombattimento);
    }
}
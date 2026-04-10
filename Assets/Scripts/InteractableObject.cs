using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("Interazione")]
    public float raggioInterazione = 5f;

    [Header("Combattimento")]
    public float distanzaCombattimento = 3f;

    private Transform giocatore;
    private CameraFollow cameraFollow;
    private MonoBehaviour controllerGiocatore;
    private NavMeshAgent agente;
    private bool inInterazione = false;
    private bool inMovimentoVersoMostro = false;

    // Recupera i riferimenti al giocatore, alla camera e al controller
    void Start()
    {
        giocatore = GameObject.FindWithTag("Player").transform;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        agente = giocatore.GetComponent<NavMeshAgent>();

        controllerGiocatore = giocatore.GetComponent<StarterAssets.ThirdPersonController>();
        if (controllerGiocatore == null)
            controllerGiocatore = giocatore.GetComponent<ThirdPersonController>();
    }

    // Rileva la pressione di E quando il giocatore è nel raggio e avvia o termina l'interazione
    void Update()
    {
        if (GameState.inputBloccato) return;
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

    // Muove il giocatore verso il mostro via NavMesh, poi avvia il combattimento e cambia la camera
    System.Collections.IEnumerator IniziaInterazione()
    {
        inMovimentoVersoMostro = true;

        // Disabilita il controller manuale
        if (controllerGiocatore != null)
            controllerGiocatore.enabled = false;

        // Calcola la destinazione davanti al mostro
        Vector3 direzione = (giocatore.position - transform.position).normalized;
        Vector3 destinazione = transform.position + direzione * distanzaCombattimento;
        destinazione.y = giocatore.position.y;

        // Attiva il NavMeshAgent e aspetta un frame prima di usarlo
        agente.enabled = true;
        yield return null;

        agente.SetDestination(destinazione);

        // Aspetta che il percorso sia calcolato
        yield return new WaitUntil(() => !agente.pathPending);

        // Aspetta che il giocatore raggiunga la destinazione, aggiornando l'animazione
        Animator animatore = giocatore.GetComponent<Animator>();

        while (agente.enabled && agente.remainingDistance > 0.1f)
        {
            if (animatore != null)
                animatore.SetFloat("Speed", agente.velocity.magnitude);
            yield return null;
        }

        // Destinazione raggiunta — resetta l'animazione
        if (animatore != null)
            animatore.SetFloat("Speed", 0f);

        // Disattiva il NavMeshAgent
        agente.enabled = false;

        // Gira il giocatore verso il mostro
        giocatore.LookAt(new Vector3(
            transform.position.x,
            giocatore.position.y,
            transform.position.z
        ));

        inMovimentoVersoMostro = false;
        inInterazione = true;

        cameraFollow.ImpostaTargetInterazione(transform, Vector3.zero);

        Debug.Log("Combattimento iniziato con: " + gameObject.name);

        // Avvia il combattimento
        CombatUnit unitaGiocatore = giocatore.GetComponent<CombatUnit>();
        CombatUnit unitaMostro = GetComponent<CombatUnit>();

        if (unitaGiocatore != null && unitaMostro != null)
            CombatManager.Instance.IniziaCombattimento(unitaGiocatore, unitaMostro, this);
        else
            Debug.LogWarning("CombatUnit mancante su giocatore o mostro.");
    }

    // Ripristina la camera, riabilita il controller e nasconde le UI di combattimento
    void TerminaInterazione()
    {
        inInterazione = false;
        cameraFollow.RipristinaCamera();

        if (controllerGiocatore != null)
            controllerGiocatore.enabled = true;

        // Nascondi le UI
        CombatUnit unitaGiocatore = giocatore.GetComponent<CombatUnit>();
        CombatUnit unitaMostro = GetComponent<CombatUnit>();
        if (unitaGiocatore != null) unitaGiocatore.NascondiUI();
        if (unitaMostro != null) unitaMostro.NascondiUI();
        CombatUI.Instance.NascondiCombatUI();
        //Debug.Log("Combattimento terminato");
    }

    // Chiamato dal CombatManager alla vittoria per forzare la chiusura del combattimento
    public void ForzaUscitaCombattimento()
    {
        if (!inInterazione) return;
        TerminaInterazione();
    }

    // Disegna i raggi di interazione e combattimento nell'editor per facilitare il debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raggioInterazione);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanzaCombattimento);
    }
}
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

    void Start()
    {
        giocatore = GameObject.FindWithTag("Player").transform;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        agente = giocatore.GetComponent<NavMeshAgent>();

        controllerGiocatore = giocatore.GetComponent<StarterAssets.ThirdPersonController>();
        if (controllerGiocatore == null)
            controllerGiocatore = giocatore.GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (inMovimentoVersoMostro) return;

        float distanza = Vector3.Distance(transform.position, giocatore.position);

        if (distanza <= raggioInterazione && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!inInterazione)
                StartCoroutine(IniziaInterazione());
            else
                TerminaInterazione();
        }

        //// Test temporaneo — premi A per attaccare
        //if (inInterazione && Keyboard.current.aKey.wasPressedThisFrame)
        //    CombatManager.Instance.AggiungiAzioneGiocatore(TipoAzione.AttaccoFisico);
    }

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

        // Aspetta che il giocatore raggiunga la destinazione
        while (agente.enabled && agente.remainingDistance > 0.1f)
        {
            yield return null;
        }

        // Destinazione raggiunta
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
        Debug.Log("Combattimento terminato");
    }

    public void ForzaUscitaCombattimento()
    {
        if (!inInterazione) return;
        TerminaInterazione();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raggioInterazione);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanzaCombattimento);
    }
}
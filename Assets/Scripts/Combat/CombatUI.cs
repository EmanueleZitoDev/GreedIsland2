using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance;

    [Header("Pannello Azioni Selezionate")]
    public GameObject[] slots;           // Slot1, Slot2, Slot3
    public TMP_Text[] testiSlot;         // TextMeshPro di ogni slot
    public Button[] bottoniRimuovi;      // Pulsante X di ogni slot

    [Header("Pulsante Conferma")]
    public GameObject pulsanteConferma;

    [Header("Pannello Inferiore")]
    public GameObject pannelloPreferiti;
    public GameObject pannelloContenuto;

    [Header("Preferiti")]
    public Button preferito1;
    public Button preferito2;
    public Button preferito3;
    public Button preferito4;

    private TipoAzione[] azioniInSlot = new TipoAzione[3];
    private bool[] slotOccupato = new bool[3];

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        gameObject.SetActive(false);

    }

    void Start()
    {
        // Collega i pulsanti X
        for (int i = 0; i < bottoniRimuovi.Length; i++)
        {
            int index = i;
            bottoniRimuovi[i].onClick.AddListener(() => RimuoviAzione(index));
        }

        // Collega i preferiti
        preferito1.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito2.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito3.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito4.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));

        AggiornaUI();
    }

    // Chiamato da CombatManager quando inizia il combattimento
    public void MostraCombatUI()
    {
        gameObject.SetActive(true);
        ResetSlots();
    }

    // Chiamato da CombatManager quando finisce il combattimento
    public void NascondiCombatUI()
    {
        gameObject.SetActive(false);
    }

    // Seleziona un'azione e la mette nel primo slot libero
    public void SelezionaAzione(TipoAzione tipo)
    {
        if (!CombatManager.Instance.IsInFaseSelezione()) return;

        for (int i = 0; i < slotOccupato.Length; i++)
        {
            if (!slotOccupato[i])
            {
                slotOccupato[i] = true;
                azioniInSlot[i] = tipo;
                CombatManager.Instance.AggiungiAzioneGiocatore(tipo);
                AggiornaUI();
                return;
            }
        }
    }

    // Rimuove l'azione da uno slot
    public void RimuoviAzione(int index)
    {
        if (!CombatManager.Instance.IsInFaseSelezione()) return;
        if (!slotOccupato[index]) return;

        slotOccupato[index] = false;
        CombatManager.Instance.RimuoviUltimaAzione();
        AggiornaUI();
    }

    // Aggiorna la visualizzazione degli slot
    void AggiornaUI()
    {
        int azioniSelezionate = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slotOccupato[i])
            {
                testiSlot[i].text = azioniInSlot[i].ToString();
                bottoniRimuovi[i].gameObject.SetActive(true);
                azioniSelezionate++;
            }
            else
            {
                testiSlot[i].text = "---";
                bottoniRimuovi[i].gameObject.SetActive(false);
            }
        }

        // Mostra il pulsante Conferma solo quando tutti gli slot sono pieni
        pulsanteConferma.SetActive(azioniSelezionate >= 3);
    }

    void ResetSlots()
    {
        for (int i = 0; i < slotOccupato.Length; i++)
            slotOccupato[i] = false;

        AggiornaUI();
    }
}
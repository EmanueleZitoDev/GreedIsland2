using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance;

    [Header("Pannello Azioni Selezionate")]
    public GameObject pannelloAzioniSelezionate;
    public GameObject slotTemplate; // il prefab

    // Sostituisce gli array fissi
    private List<GameObject> slotsAttivi = new List<GameObject>();
    private List<TMP_Text> testiSlotAttivi = new List<TMP_Text>();
    private List<Button> bottoniRimuoviAttivi = new List<Button>();

    private TipoAzione[] azioniInSlot;
    private StanceTipo[] stanceInSlot;
    private bool[] slotOccupato;

    [Header("Pulsante Conferma")]
    public GameObject pulsanteConferma;

    [Header("Pulsante Indietro")]
    public GameObject pulsanteIndietro;

    [Header("Pannello Inferiore")]
    public GameObject pannelloPreferiti;

    [Header("Preferiti")]
    public Button preferito1;
    public Button preferito2;
    public Button preferito3;
    public Button preferito4;

    [Header("Pannello Lista")]
    public GameObject pulsanteAzioni;
    public GameObject pulsanteCarte;
    public GameObject pannelloLista;
    public Transform contenutoLista;
    public GameObject templatePulsanteAzione;

    [Header("Stance")]
    public Button pulsanteTen;
    public Button pulsanteRen;

    private AbilitaDato[] abilitaInSlot;

    // Colori per feedback visivo dei toggle stance
    private Color coloreStanceAttiva = new Color(0.2f, 0.6f, 1f);     // blu chiaro = attiva
    private Color coloreStanceInattiva = new Color(0.8f, 0.8f, 0.8f); // grigio = inattiva

    // Stance attualmente selezionata — default Ten
    private StanceTipo stanceCorrente = StanceTipo.Ten;

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
        // Collega i preferiti
        preferito1.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito2.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito3.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
        preferito4.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));

        // Collega i pulsanti Azioni e Carte
        GameObject.Find("PulsanteAzioni").GetComponent<Button>().onClick.AddListener(ApriListaAzioni);
        GameObject.Find("PulsanteCarte").GetComponent<Button>().onClick.AddListener(ApriListaCarte);

        pulsanteIndietro.GetComponent<Button>().onClick.AddListener(TornaAlDefault);
        pulsanteConferma.GetComponent<Button>().onClick.AddListener(() =>
        {
            CombatManager.Instance.ConfermaAzioni();
            pulsanteConferma.SetActive(false);
            ResetSlots();
            TornaAlDefault();
        });

        // Collega i pulsanti stance
        pulsanteTen.onClick.AddListener(() => SelezionaStance(StanceTipo.Ten));
        pulsanteRen.onClick.AddListener(() => SelezionaStance(StanceTipo.Ren));

        AggiornaUI();
        AggiornaPulsantiStance();
    }

    public void MostraCombatUI()
    {
        gameObject.SetActive(true);

        int numAzioni = CombatManager.Instance.giocatore.azioniPerTurno;
        GeneraSlots(numAzioni);
        ResetSlots();

        stanceCorrente = StanceTipo.Ten;
        AggiornaPulsantiStance();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NascondiCombatUI()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Cambia la stance corrente — mutualmente esclusiva
    public void SelezionaStance(StanceTipo stance)
    {
        stanceCorrente = stance;
        AggiornaPulsantiStance();
        //Debug.Log("Stance selezionata: " + stanceCorrente);
    }

    // Aggiorna il feedback visivo dei pulsanti stance
    void AggiornaPulsantiStance()
    {
        SetColoreBottone(pulsanteTen, stanceCorrente == StanceTipo.Ten ? coloreStanceAttiva : coloreStanceInattiva);
        SetColoreBottone(pulsanteRen, stanceCorrente == StanceTipo.Ren ? coloreStanceAttiva : coloreStanceInattiva);
    }

    void SetColoreBottone(Button btn, Color colore)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = colore;
        cb.selectedColor = colore;
        btn.colors = cb;
    }

    // Seleziona un'azione e la mette nel primo slot libero con la stance corrente
    public void SelezionaAzione(TipoAzione tipo)
    {
        if (!CombatManager.Instance.IsInFaseSelezione()) return;

        for (int i = 0; i < slotOccupato.Length; i++)
        {
            if (!slotOccupato[i])
            {
                slotOccupato[i] = true;
                azioniInSlot[i] = tipo;
                stanceInSlot[i] = stanceCorrente;
                CombatManager.Instance.AggiungiAzioneGiocatore(tipo, stanceCorrente, i);
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
        //CombatManager.Instance.RimuoviUltimaAzione();
        //Debug.Log("Rimuovi azione in slot " + index);
        CombatManager.Instance.RimuoviAzioneAIndice(index);
        AggiornaUI();
    }

    public void ApriListaAzioni()
    {
        pannelloPreferiti.SetActive(false);
        pulsanteAzioni.SetActive(false);
        pulsanteCarte.SetActive(false);
        pannelloLista.SetActive(true);
        pulsanteIndietro.SetActive(true);

        // Distruggi tutti i pulsanti esistenti tranne il template
        for (int i = contenutoLista.childCount - 1; i >= 0; i--)
        {
            GameObject figlio = contenutoLista.GetChild(i).gameObject;
            if (figlio != templatePulsanteAzione)
                Destroy(figlio);
        }

        // Crea un pulsante per ogni abilità sbloccata
        SkillTreePersonaggio skillTree = CombatManager.Instance.giocatore.GetComponent<SkillTreePersonaggio>();
        if (skillTree == null) return;

        // Aggiungi Attacco Fisico come prima opzione
        GameObject btnAttacco = Instantiate(templatePulsanteAzione, contenutoLista);
        btnAttacco.SetActive(true);
        TMP_Text testoAttacco = btnAttacco.GetComponentInChildren<TMP_Text>();
        if (testoAttacco != null) testoAttacco.text = "Attacco Fisico";
        Button btnAttaccoBtn = btnAttacco.GetComponent<Button>();
        btnAttaccoBtn.onClick.RemoveAllListeners();
        btnAttaccoBtn.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));

        foreach (AbilitaDato abilita in skillTree.abilitaSbloccate)
        {
            if (abilita == null) continue;

            GameObject nuovo = Instantiate(templatePulsanteAzione, contenutoLista);
            nuovo.SetActive(true);

            TMP_Text testo = nuovo.GetComponentInChildren<TMP_Text>();
            if (testo != null) testo.text = abilita.nomeAbilita;

            Button btn = nuovo.GetComponent<Button>();
            AbilitaDato abilitaRef = abilita;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelezionaAbilita(abilitaRef));
        }
    }

    public void SelezionaAbilita(AbilitaDato abilita)
    {
        if (!CombatManager.Instance.IsInFaseSelezione()) return;

        for (int i = 0; i < slotOccupato.Length; i++)
        {
            if (!slotOccupato[i])
            {
                slotOccupato[i] = true;
                azioniInSlot[i] = TipoAzione.UsaAbilita;
                stanceInSlot[i] = stanceCorrente;
                abilitaInSlot[i] = abilita; // ← mancava questa riga
                CombatManager.Instance.AggiungiAbilitaGiocatore(abilita, stanceCorrente, i);
                AggiornaUI();
                return;
            }
        }
    }

    public void TornaAlDefault()
    {
        pannelloPreferiti.SetActive(true);
        pulsanteAzioni.SetActive(true);
        pulsanteCarte.SetActive(true);
        pannelloLista.SetActive(false);
        pulsanteIndietro.SetActive(false);
    }

    public void ApriListaCarte()
    {
        Debug.Log("Lista carte — da implementare");
    }

    void CreaPulsanteAzione(string nome, TipoAzione tipo)
    {
        GameObject nuovo = Instantiate(templatePulsanteAzione, contenutoLista);
        nuovo.SetActive(true);
        nuovo.GetComponentInChildren<TMP_Text>().text = nome;
        nuovo.GetComponent<Button>().onClick.AddListener(() =>
        {
            SelezionaAzione(tipo);
            TornaAlDefault();
        });
    }

    // Aggiorna la visualizzazione degli slot
    // Mostra azione e stance in ogni slot occupato
    void AggiornaUI()
    {
        int azioniSelezionate = 0;

        for (int i = 0; i < slotsAttivi.Count; i++)
        {
            if (slotOccupato[i])
            {
                string nomeAzione = azioniInSlot[i] == TipoAzione.UsaAbilita && abilitaInSlot[i] != null
                    ? abilitaInSlot[i].nomeAbilita
                    : azioniInSlot[i].ToString();

                testiSlotAttivi[i].text = nomeAzione + "\n<size=80%>" + stanceInSlot[i].ToString() + "</size>";
                bottoniRimuoviAttivi[i].gameObject.SetActive(true);
                azioniSelezionate++;
            }
            else
            {
                testiSlotAttivi[i].text = "---";
                bottoniRimuoviAttivi[i].gameObject.SetActive(false);
            }
        }

        pulsanteConferma.SetActive(azioniSelezionate >= slotsAttivi.Count);
    }

    void ResetSlots()
    {
        if (slotOccupato == null) return;
        for (int i = 0; i < slotOccupato.Length; i++)
            slotOccupato[i] = false;
        AggiornaUI();
    }

    void GeneraSlots(int numAzioni)
    {
        abilitaInSlot = new AbilitaDato[numAzioni];
        // Distruggi slot esistenti
        foreach (GameObject slot in slotsAttivi)
            Destroy(slot);

        slotsAttivi.Clear();
        testiSlotAttivi.Clear();
        bottoniRimuoviAttivi.Clear();

        // Inizializza array con la dimensione corretta
        azioniInSlot = new TipoAzione[numAzioni];
        stanceInSlot = new StanceTipo[numAzioni];
        slotOccupato = new bool[numAzioni];

        // Crea gli slot dinamicamente
        for (int i = 0; i < numAzioni; i++)
        {
            int index = i;
            GameObject nuovoSlot = Instantiate(slotTemplate, pannelloAzioniSelezionate.transform);
            nuovoSlot.SetActive(true);

            TMP_Text testo = nuovoSlot.GetComponentInChildren<TMP_Text>();
            Button btnRimuovi = nuovoSlot.GetComponentInChildren<Button>();

            btnRimuovi.onClick.RemoveAllListeners();
            btnRimuovi.onClick.AddListener(() => RimuoviAzione(index));

            slotsAttivi.Add(nuovoSlot);
            testiSlotAttivi.Add(testo);
            bottoniRimuoviAttivi.Add(btnRimuovi);
        }
    }

}
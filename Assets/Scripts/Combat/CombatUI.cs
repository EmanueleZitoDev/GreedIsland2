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

	// Colori per feedback visivo dei toggle stance
	private Color coloreStanceAttiva = new Color(0.2f, 0.6f, 1f);     // blu chiaro = attiva
	private Color coloreStanceInattiva = new Color(0.8f, 0.8f, 0.8f); // grigio = inattiva

	private TipoAzione[] azioniInSlot = new TipoAzione[3];
	private StanceTipo[] stanceInSlot = new StanceTipo[3];
	private bool[] slotOccupato = new bool[3];

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

		// Adatta gli slot al numero di azioni del giocatore
		int numAzioni = CombatManager.Instance.giocatore.azioniPerTurno;
		for (int i = 0; i < slots.Length; i++)
			slots[i].SetActive(i < numAzioni);

		ResetSlots();

		// Reset stance a Ten all'inizio di ogni combattimento
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
		Debug.Log("Rimuovi azione in slot " + index);
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

		foreach (Transform figlio in contenutoLista)
		{
			Button btn = figlio.GetComponent<Button>();
			if (btn == null) continue;
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() => SelezionaAzione(TipoAzione.AttaccoFisico));
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

		for (int i = 0; i < slots.Length; i++)
		{
			if (slotOccupato[i])
			{
				testiSlot[i].text = azioniInSlot[i].ToString() + "\n<size=80%>" + stanceInSlot[i].ToString() + "</size>";
				bottoniRimuovi[i].gameObject.SetActive(true);
				azioniSelezionate++;
			}
			else
			{
				testiSlot[i].text = "---";
				bottoniRimuovi[i].gameObject.SetActive(false);
			}
		}

		pulsanteConferma.SetActive(azioniSelezionate >= 3);
	}

	void ResetSlots()
	{
		for (int i = 0; i < slotOccupato.Length; i++)
			slotOccupato[i] = false;

		AggiornaUI();
	}
}
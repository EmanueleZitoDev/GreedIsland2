using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AbilitaImporter : EditorWindow
{
    private string percorsoCSV = "Assets/Data/abilita.csv";
    private string cartellaOutput = "Assets/ScriptableObjects/Abilita";

    [MenuItem("GreedIsland/Importa Abilità da CSV")]
    public static void MostraFinestra()
    {
        GetWindow<AbilitaImporter>("Importa Abilità");
    }

    void OnGUI()
    {
        GUILayout.Label("Importa Abilità da CSV", EditorStyles.boldLabel);

        GUILayout.Space(10);
        percorsoCSV = EditorGUILayout.TextField("Percorso CSV:", percorsoCSV);
        cartellaOutput = EditorGUILayout.TextField("Cartella Output:", cartellaOutput);

        GUILayout.Space(10);
        if (GUILayout.Button("Importa"))
            ImportaCSV();
    }

    void ImportaCSV()
    {
        if (!File.Exists(percorsoCSV))
        {
            Debug.LogError("File CSV non trovato: " + percorsoCSV);
            return;
        }

        if (!AssetDatabase.IsValidFolder(cartellaOutput))
        {
            Directory.CreateDirectory(cartellaOutput);
            AssetDatabase.Refresh();
        }

        // Legge tutto il file come testo unico e gestisce le righe multi-linea
        string tuttoIlTesto = File.ReadAllText(percorsoCSV);
        List<string> righe = ParseRigheCSV(tuttoIlTesto);

        int create = 0;
        int aggiornate = 0;

        for (int i = 1; i < righe.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(righe[i])) continue;

            string[] campi = ParseRiga(righe[i]);

            // Log per debug
            Debug.Log("Riga " + i + " — campi trovati: " + campi.Length + " — contenuto: " + string.Join("|", campi));

            if (campi.Length < 12)
            {
                Debug.LogWarning("Riga " + i + " ignorata — campi insufficienti: " + righe[i]);
                continue;
            }

            string id = campi[0].Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string percorsoAsset = cartellaOutput + "/" + id + ".asset";
            AbilitaDato abilita = AssetDatabase.LoadAssetAtPath<AbilitaDato>(percorsoAsset);
            bool nuova = abilita == null;

            if (nuova)
            {
                abilita = ScriptableObject.CreateInstance<AbilitaDato>();
                create++;
            }
            else
            {
                aggiornate++;
            }

            abilita.id = id;
            abilita.livelloHatsu = int.TryParse(campi[1].Trim(), out int lv) ? lv : 1;
            abilita.costoPA = int.TryParse(campi[2].Trim(), out int pa) ? pa : 1;
            abilita.nomeAbilita = campi[4].Trim();
            abilita.costoNen = int.TryParse(campi[5].Trim(), out int nen) ? nen : 0;
            abilita.isPassiva = campi[6].Trim().ToLower() == "passiva";
            //abilita.effetto = campi[7].Trim();
            abilita.descrizione = campi[8].Trim();
            abilita.tags = campi[9].Trim().Split(',');
            abilita.priorita = int.TryParse(campi[10].Trim(), out int pri) ? pri : 0;

            if (System.Enum.TryParse(campi[11].Trim(), out TipoHatsu hatsu))
                abilita.albero = hatsu;

            if (nuova)
                AssetDatabase.CreateAsset(abilita, percorsoAsset);
            else
                EditorUtility.SetDirty(abilita);
        }

        // Secondo passaggio — prerequisiti
        for (int i = 1; i < righe.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(righe[i])) continue;
            string[] campi = ParseRiga(righe[i]);
            if (campi.Length < 4) continue;

            string id = campi[0].Trim();
            string prerequisitoId = campi[3].Trim();
            if (string.IsNullOrEmpty(prerequisitoId) || prerequisitoId == "---") continue;

            AbilitaDato abilita = AssetDatabase.LoadAssetAtPath<AbilitaDato>(cartellaOutput + "/" + id + ".asset");
            AbilitaDato prerequisito = AssetDatabase.LoadAssetAtPath<AbilitaDato>(cartellaOutput + "/" + prerequisitoId + ".asset");

            if (abilita != null && prerequisito != null)
            {
                abilita.prerequisiti = new AbilitaDato[] { prerequisito };
                EditorUtility.SetDirty(abilita);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Import completato — Create: " + create + ", Aggiornate: " + aggiornate);
    }

    // Parsa il testo CSV completo gestendo campi multi-linea tra virgolette
    List<string> ParseRigheCSV(string testo)
    {
        List<string> righe = new List<string>();
        bool inVirgolette = false;
        string rigaCorrente = "";

        for (int i = 0; i < testo.Length; i++)
        {
            char c = testo[i];

            if (c == '"')
            {
                inVirgolette = !inVirgolette;
                rigaCorrente += c;
            }
            else if ((c == '\n' || c == '\r') && !inVirgolette)
            {
                // Salta \r\n doppio
                if (c == '\r' && i + 1 < testo.Length && testo[i + 1] == '\n')
                    i++;

                if (!string.IsNullOrWhiteSpace(rigaCorrente))
                    righe.Add(rigaCorrente);
                rigaCorrente = "";
            }
            else
            {
                rigaCorrente += c;
            }
        }

        if (!string.IsNullOrWhiteSpace(rigaCorrente))
            righe.Add(rigaCorrente);

        return righe;
    }

    // Parser CSV che gestisce valori tra virgolette
    string[] ParseRiga(string riga)
    {
        List<string> campi = new List<string>();
        int i = 0;

        while (i < riga.Length)
        {
            if (riga[i] == '"')
            {
                // Campo tra virgolette
                i++; // salta la virgoletta di apertura
                string campo = "";
                while (i < riga.Length)
                {
                    if (riga[i] == '"' && i + 1 < riga.Length && riga[i + 1] == '"')
                    {
                        // Virgoletta escapata ""
                        campo += '"';
                        i += 2;
                    }
                    else if (riga[i] == '"')
                    {
                        i++; // salta la virgoletta di chiusura
                        break;
                    }
                    else
                    {
                        campo += riga[i];
                        i++;
                    }
                }
                campi.Add(campo);
                if (i < riga.Length && riga[i] == ',') i++; // salta la virgola
            }
            else
            {
                // Campo normale senza virgolette
                string campo = "";
                while (i < riga.Length && riga[i] != ',')
                {
                    campo += riga[i];
                    i++;
                }
                campi.Add(campo);
                if (i < riga.Length) i++; // salta la virgola
            }
        }

        return campi.ToArray();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestionnairePoulets : MonoBehaviour
{
    public static GestionnairePoulets instance;

    [Header("Parametres")]
    public string nourritureRequise = "Ble";
    public int nourritureParPoulet = 2;
    public int piecesParOeuf = 5;

    [Header("UI")]
    public GameObject boutonNourririPrefab;
    public GameObject boutonRamasserPrefab;
    public Canvas canvas;

    private GameObject monBoutonNourrir;
    private GameObject monBoutonRamasser;
    private Camera mainCamera;
    private bool playerDedans = false;
    private Transform playerTransform;

    private Poulet[] tousLesPoulets;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        tousLesPoulets = FindObjectsOfType<Poulet>();
        Debug.Log("Poulets trouves : " + tousLesPoulets.Length);

        if (boutonNourririPrefab != null && canvas != null)
        {
            monBoutonNourrir = Instantiate(boutonNourririPrefab, canvas.transform);
            monBoutonNourrir.SetActive(false);
            Button btn = monBoutonNourrir.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(NourririTousLesPoulets);
        }

        if (boutonRamasserPrefab != null && canvas != null)
        {
            monBoutonRamasser = Instantiate(boutonRamasserPrefab, canvas.transform);
            monBoutonRamasser.SetActive(false);
            Button btn = monBoutonRamasser.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(RamasserTousLesOeufs);
        }
    }

    void Update()
    {
        GererUI();

        if (playerDedans && Input.GetKeyDown(KeyCode.E))
            NourririTousLesPoulets();

        if (playerDedans && Input.GetKeyDown(KeyCode.R))
            RamasserTousLesOeufs();
    }

    void GererUI()
    {
        if (playerTransform == null) return;

        Vector3 posEcran = mainCamera.WorldToScreenPoint(
            playerTransform.position + Vector3.up * 2f
        );

        if (monBoutonNourrir != null)
        {
            // CORRECTION : nom de fonction sans caracteres speciaux
            bool aDesNonNourris = ADesPouletsNonNourris();
            bool afficher = playerDedans && aDesNonNourris;
            monBoutonNourrir.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonNourrir.transform.position = posEcran;

            TextMeshProUGUI tmp = monBoutonNourrir
                .GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                int stock = GestionnaireArgent.instance != null ?
                    GestionnaireArgent.instance.GetStock(nourritureRequise) : 0;
                int pouletsNourissables = Mathf.Min(
                    stock / nourritureParPoulet,
                    tousLesPoulets.Length
                );
                tmp.text = "[E] Nourrir " + pouletsNourissables
                    + "/" + tousLesPoulets.Length
                    + " poulets\n(" + stock + " " + nourritureRequise + ")";
            }
        }

        if (monBoutonRamasser != null)
        {
            bool aDesOeufs = ADesOeufsARamasser();
            bool afficher = playerDedans && aDesOeufs;
            monBoutonRamasser.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonRamasser.transform.position = posEcran;

            TextMeshProUGUI tmp = monBoutonRamasser
                .GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                int totalOeufs = CompterOeufs();
                tmp.text = "[R] Ramasser " + totalOeufs + " oeuf(s)";
            }
        }
    }

    public void NourririTousLesPoulets()
    {
        if (GestionnaireArgent.instance == null) return;

        int stock = GestionnaireArgent.instance.GetStock(nourritureRequise);
        int pouletsNourris = 0;

        foreach (Poulet poulet in tousLesPoulets)
        {
            if (poulet == null) continue;
            if (poulet.EstNourri()) continue;

            if (stock >= nourritureParPoulet)
            {
                GestionnaireArgent.instance.UtiliserRecolte(
                    nourritureRequise, nourritureParPoulet);
                poulet.NourririDepuisGestionnaire();
                stock -= nourritureParPoulet;
                pouletsNourris++;
            }
            else break;
        }

        Debug.Log(pouletsNourris + " poulet(s) nourri(s) !");
    }

    public void RamasserTousLesOeufs()
    {
        int totalOeufs = 0;

        foreach (Poulet poulet in tousLesPoulets)
        {
            if (poulet == null) continue;
            if (!poulet.ADesOeufs()) continue;

            totalOeufs += poulet.GetNombreOeufs();
            poulet.RamasserDepuisGestionnaire();
        }

        if (totalOeufs > 0 && GestionnaireArgent.instance != null)
        {
            GestionnaireArgent.instance.AjouterRecolte("Oeuf", totalOeufs);
            GestionnaireArgent.instance.AjouterPieces(totalOeufs * piecesParOeuf);
            Debug.Log("Total oeufs ramasses : " + totalOeufs);
        }
    }

    // CORRECTION : nom corrige en ASCII pur
    bool ADesPouletsNonNourris()
    {
        foreach (Poulet poulet in tousLesPoulets)
        {
            if (poulet != null && !poulet.EstNourri())
                return true;
        }
        return false;
    }

    bool ADesOeufsARamasser()
    {
        foreach (Poulet poulet in tousLesPoulets)
        {
            if (poulet != null && poulet.ADesOeufs())
                return true;
        }
        return false;
    }

    int CompterOeufs()
    {
        int total = 0;
        foreach (Poulet poulet in tousLesPoulets)
        {
            if (poulet != null && poulet.ADesOeufs())
                total += poulet.GetNombreOeufs();
        }
        return total;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDedans = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDedans = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDedans = false;
            playerTransform = null;
            if (monBoutonNourrir != null) monBoutonNourrir.SetActive(false);
            if (monBoutonRamasser != null) monBoutonRamasser.SetActive(false);
        }
    }
}
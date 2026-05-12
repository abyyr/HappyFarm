using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Stand : MonoBehaviour
{
    public static Stand instance;

    [Header("Prix des produits")]
    public int prixBle = 2;
    public int prixOeuf = 3;
    public int prixTomate = 5;
    public int prixMais = 8;
    public int prixCarotte = 4;

    [Header("UI")]
    public GameObject boutonDeposerPrefab;
    public Canvas canvas;

    // Stock du stand
    private Dictionary<string, int> stockStand
        = new Dictionary<string, int>();

    // UI
    private GameObject monBoutonDeposer;
    private bool playerDedans = false;
    private Transform playerTransform;
    private Camera mainCamera;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;

        // Init stock
        stockStand["Blé"] = 0;
        stockStand["Oeuf"] = 0;
        stockStand["Tomate"] = 0;
        stockStand["Maïs"] = 0;
        stockStand["Carotte"] = 0;

        // Crée bouton
        if (boutonDeposerPrefab != null && canvas != null)
        {
            monBoutonDeposer = Instantiate(boutonDeposerPrefab, canvas.transform);
            monBoutonDeposer.SetActive(false);
            Button btn = monBoutonDeposer.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(DeposerTousProduits);
        }
    }

    void Update()
    {
        GererUI();

        if (playerDedans && Input.GetKeyDown(KeyCode.E))
            DeposerTousProduits();
    }

    void GererUI()
    {
        if (playerTransform == null) return;

        Vector3 posEcran = mainCamera.WorldToScreenPoint(
            playerTransform.position + Vector3.up * 2f
        );

        if (monBoutonDeposer != null)
        {
            bool afficher = playerDedans;
            monBoutonDeposer.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonDeposer.transform.position = posEcran;

            // Affiche stock disponible
            TextMeshProUGUI tmp = monBoutonDeposer
                .GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null && GestionnaireArgent.instance != null)
            {
                string texte = "[E] Deposer sur le stand\n";
                foreach (var kvp in stockStand)
                {
                    int stockDispo = GestionnaireArgent.instance.GetStock(kvp.Key);
                    if (stockDispo > 0)
                        texte += kvp.Key + " : " + stockDispo + " | ";
                }
                tmp.text = texte;
            }
        }
    }

    public void DeposerTousProduits()
    {
        if (GestionnaireArgent.instance == null) return;

        int totalDepose = 0;

        foreach (string produit in new List<string>(stockStand.Keys))
        {
            int quantite = GestionnaireArgent.instance.GetStock(produit);
            if (quantite > 0)
            {
                GestionnaireArgent.instance.UtiliserRecolte(produit, quantite);
                stockStand[produit] += quantite;
                totalDepose += quantite;
                Debug.Log("Deposé " + quantite + " " + produit + " sur le stand");
            }
        }

        if (totalDepose == 0)
            Debug.Log("Rien à déposer !");
    }

    public bool VendreProduit(string produit, int quantite)
    {
        if (!stockStand.ContainsKey(produit)) return false;
        if (stockStand[produit] < quantite) return false;

        stockStand[produit] -= quantite;
        int prix = GetPrix(produit) * quantite;

        if (GestionnaireArgent.instance != null)
            GestionnaireArgent.instance.AjouterPieces(prix);

        Debug.Log("Vendu " + quantite + " " + produit + " pour " + prix + " pieces !");
        return true;
    }

    public bool AStockSuffisant(string produit, int quantite)
    {
        if (!stockStand.ContainsKey(produit)) return false;
        return stockStand[produit] >= quantite;
    }

    public int GetPrix(string produit)
    {
        switch (produit)
        {
            case "Blé": return prixBle;
            case "Oeuf": return prixOeuf;
            case "Tomate": return prixTomate;
            case "Maïs": return prixMais;
            case "Carotte": return prixCarotte;
            default: return 1;
        }
    }

    public Dictionary<string, int> GetStock()
    {
        return stockStand;
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
            if (monBoutonDeposer != null)
                monBoutonDeposer.SetActive(false);
        }
    }
}
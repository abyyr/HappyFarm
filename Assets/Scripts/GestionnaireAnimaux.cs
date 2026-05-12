using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestionnaire générique pour tout enclos d'animaux.
/// Fonctionne avec le composant Animal.cs, peu importe l'espèce.
/// Placez ce composant sur un Trigger Collider englobant l'enclos.
/// </summary>
public class GestionnaireAnimaux : MonoBehaviour
{
    public static GestionnaireAnimaux instance;

    [Header("Nourriture")]
    [Tooltip("Nom de la ressource requise (ex: Ble, Herbe, Grain)")]
    public string nourritureRequise = "Ble";

    [Tooltip("Quantité de nourriture consommée par animal et par cycle")]
    public int nourritureParAnimal = 2;

    [Header("Récompense")]
    [Tooltip("Pièces gagnées par unité de produit ramassé")]
    public int piecesParProduit = 5;

    [Tooltip("Nom de la récolte ajoutée à l'inventaire (ex: Oeuf, Lait, Laine)")]
    public string nomRecolte = "Oeuf";

    [Header("UI")]
    public GameObject boutonNourririPrefab;
    public GameObject boutonRamasserPrefab;
    public Canvas canvas;

    // --- Privé ---
    private GameObject monBoutonNourrir;
    private GameObject monBoutonRamasser;
    private Camera mainCamera;
    private bool playerDedans = false;
    private Transform playerTransform;
    private Animal[] tousLesAnimaux;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        // Cherche tous les animaux enfants de cet objet ET dans la scène
        tousLesAnimaux = GetComponentsInChildren<Animal>();
        if (tousLesAnimaux.Length == 0)
            tousLesAnimaux = FindObjectsOfType<Animal>();

        Debug.Log($"[{gameObject.name}] Animaux trouvés : {tousLesAnimaux.Length}");

        CreerBouton(boutonNourririPrefab, ref monBoutonNourrir, NourririTousLesAnimaux);
        CreerBouton(boutonRamasserPrefab, ref monBoutonRamasser, RamasserTousLesProduits);
    }

    void CreerBouton(GameObject prefab, ref GameObject cible, UnityEngine.Events.UnityAction action)
    {
        if (prefab == null || canvas == null) return;
        cible = Instantiate(prefab, canvas.transform);
        cible.SetActive(false);
        Button btn = cible.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(action);
    }

    void Update()
    {
        GererUI();

        if (playerDedans)
        {
            if (Input.GetKeyDown(KeyCode.E)) NourririTousLesAnimaux();
            if (Input.GetKeyDown(KeyCode.R)) RamasserTousLesProduits();
        }
    }

    void GererUI()
    {
        if (playerTransform == null) return;

        Vector3 posEcran = mainCamera.WorldToScreenPoint(
            playerTransform.position + Vector3.up * 2f
        );

        // --- Bouton Nourrir ---
        if (monBoutonNourrir != null)
        {
            bool afficher = playerDedans && ADesAnimauxNonNourris();
            monBoutonNourrir.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonNourrir.transform.position = posEcran;

            TextMeshProUGUI tmp = monBoutonNourrir.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                int stock = GestionnaireArgent.instance != null
                    ? GestionnaireArgent.instance.GetStock(nourritureRequise) : 0;
                int nourissables = Mathf.Min(stock / nourritureParAnimal, tousLesAnimaux.Length);
                tmp.text = $"[E] Nourrir {nourissables}/{tousLesAnimaux.Length} animaux\n" +
                           $"({stock} {nourritureRequise})";
            }
        }

        // --- Bouton Ramasser ---
        if (monBoutonRamasser != null)
        {
            bool afficher = playerDedans && ADesProduits();
            monBoutonRamasser.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonRamasser.transform.position = posEcran + Vector3.down * 50f;

            TextMeshProUGUI tmp = monBoutonRamasser.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = $"[R] Ramasser {CompterProduits()} {nomRecolte}(s)";
        }
    }

    public void NourririTousLesAnimaux()
    {
        if (GestionnaireArgent.instance == null) return;

        int stock = GestionnaireArgent.instance.GetStock(nourritureRequise);
        int nourris = 0;

        foreach (Animal animal in tousLesAnimaux)
        {
            if (animal == null || animal.EstNourri()) continue;
            if (stock < nourritureParAnimal) break;

            GestionnaireArgent.instance.UtiliserRecolte(nourritureRequise, nourritureParAnimal);
            animal.NourririDepuisGestionnaire();
            stock -= nourritureParAnimal;
            nourris++;
        }

        Debug.Log($"{nourris} animal(ux) nourri(s) dans {gameObject.name} !");
    }

    public void RamasserTousLesProduits()
    {
        int total = 0;

        foreach (Animal animal in tousLesAnimaux)
        {
            if (animal == null || !animal.AProduits()) continue;
            total += animal.GetNombreProduits();
            animal.RamasserDepuisGestionnaire();
        }

        if (total > 0 && GestionnaireArgent.instance != null)
        {
            GestionnaireArgent.instance.AjouterRecolte(nomRecolte, total);
            GestionnaireArgent.instance.AjouterPieces(total * piecesParProduit);
            Debug.Log($"{total} {nomRecolte}(s) ramassé(s) dans {gameObject.name} !");
        }
    }

    // --- Helpers ---
    bool ADesAnimauxNonNourris()
    {
        foreach (Animal a in tousLesAnimaux)
            if (a != null && !a.EstNourri()) return true;
        return false;
    }

    bool ADesProduits()
    {
        foreach (Animal a in tousLesAnimaux)
            if (a != null && a.AProduits()) return true;
        return false;
    }

    int CompterProduits()
    {
        int total = 0;
        foreach (Animal a in tousLesAnimaux)
            if (a != null && a.AProduits()) total += a.GetNombreProduits();
        return total;
    }

    // --- Triggers ---
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
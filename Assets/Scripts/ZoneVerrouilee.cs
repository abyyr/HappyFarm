using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Verrou universel pour n'importe quelle zone.
/// 
/// Comment utiliser :
/// - Pour une culture  : ajoute ZoneVerrouillee + ZoneCulture sur le même GameObject
/// - Pour une cour     : ajoute ZoneVerrouillee + GestionnaireAnimaux sur le même GameObject
/// - Pour autre chose  : ajoute ZoneVerrouillee seul
/// 
/// ZoneVerrouillee détecte automatiquement les autres scripts présents.
/// Rien à glisser manuellement !
/// </summary>
public class ZoneVerrouillee : MonoBehaviour
{
    [Header("Déverrouillage")]
    public int coutDeblocage = 50;
    public bool estVerrouillee = true;

    [Header("Nom de la zone (affiché dans le bouton)")]
    public string nomZone = "Zone";

    [Header("Objets à désactiver quand verrouillé (optionnel)")]
    [Tooltip("Ex: barrière, panneau. Laisse vide si pas besoin.")]
    public GameObject[] objetsADesactiver;

    [Header("Cadenas 3D")]
    public GameObject cadenasPrefab;
    public float hauteurCadenas = 3f;

    [Header("UI Canvas")]
    public Canvas canvas;
    public GameObject boutonDebloquerPrefab;
    public GameObject textePasAssezPrefab;

    // --- Privé ---
    private GameObject monCadenas;
    private GameObject monBoutonDebloquer;
    private GameObject monTextePasAssez;
    private bool playerDedans = false;
    private Transform playerTransform;
    private Camera mainCamera;

    // Composants détectés automatiquement
    private MonoBehaviour[] composantsDetectes;

    void Start()
    {
        mainCamera = Camera.main;

        // Détecte automatiquement tous les autres scripts sur ce GameObject
        // (ZoneCulture, GestionnaireAnimaux, etc.) sauf ZoneVerrouillee lui-même
        var tous = GetComponents<MonoBehaviour>();
        var liste = new System.Collections.Generic.List<MonoBehaviour>();
        foreach (var comp in tous)
            if (comp != this) liste.Add(comp);
        composantsDetectes = liste.ToArray();

        Debug.Log($"[ZoneVerrouillee] '{nomZone}' : {composantsDetectes.Length} composant(s) détecté(s).");

        AppliquerEtatInitial();
        CreerCadenas();
        CreerUI();
    }

    void AppliquerEtatInitial()
    {
        // Désactive tous les autres scripts si verrouillé
        foreach (var comp in composantsDetectes)
            if (comp != null) comp.enabled = !estVerrouillee;

        // Désactive les objets optionnels
        foreach (var obj in objetsADesactiver)
            if (obj != null) obj.SetActive(!estVerrouillee);
    }

    void CreerCadenas()
    {
        if (cadenasPrefab == null || !estVerrouillee) return;
        Vector3 pos = transform.position + Vector3.up * hauteurCadenas;
        monCadenas = Instantiate(cadenasPrefab, pos, Quaternion.identity);
    }

    void CreerUI()
    {
        if (canvas == null) return;

        if (boutonDebloquerPrefab != null)
        {
            monBoutonDebloquer = Instantiate(boutonDebloquerPrefab, canvas.transform);
            monBoutonDebloquer.SetActive(false);
            Button btn = monBoutonDebloquer.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(TenterDeblocage);

            TextMeshProUGUI tmp = monBoutonDebloquer.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = $"[E] Débloquer {nomZone}\n({coutDeblocage} pièces)";
        }

        if (textePasAssezPrefab != null)
        {
            monTextePasAssez = Instantiate(textePasAssezPrefab, canvas.transform);
            monTextePasAssez.SetActive(false);

            TextMeshProUGUI tmp = monTextePasAssez.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = $"Pas assez de pièces ! ({coutDeblocage} requis)";
        }
    }

    void Update()
    {
        if (!estVerrouillee) return;

        GererBouton();
        AnimerCadenas();

        if (playerDedans && Input.GetKeyDown(KeyCode.E))
            TenterDeblocage();
    }

    void AnimerCadenas()
    {
        if (monCadenas != null)
            monCadenas.transform.Rotate(Vector3.up, 45f * Time.deltaTime);
    }

    void GererBouton()
    {
        if (monBoutonDebloquer == null || playerTransform == null) return;

        bool afficher = playerDedans && estVerrouillee;
        monBoutonDebloquer.SetActive(afficher);

        if (afficher)
        {
            Vector3 posEcran = mainCamera.WorldToScreenPoint(
                playerTransform.position + Vector3.up * 2f
            );
            if (posEcran.z > 0)
                monBoutonDebloquer.transform.position = posEcran;
        }
    }

    public void TenterDeblocage()
    {
        if (!estVerrouillee) return;

        if (GestionnaireArgent.instance == null)
        {
            Debug.LogError("GestionnaireArgent introuvable !");
            return;
        }

        if (GestionnaireArgent.instance.APiecesSuffisantes(coutDeblocage))
        {
            GestionnaireArgent.instance.DepensesPieces(coutDeblocage);
            Debloquer();
        }
        else
        {
            int manque = coutDeblocage - GestionnaireArgent.instance.pieces;
            Debug.Log($"Pas assez de pièces pour {nomZone}. Manque : {manque}");
            AfficherPasAssez();
        }
    }

    void Debloquer()
    {
        estVerrouillee = false;

        // Active tous les composants détectés automatiquement
        foreach (var comp in composantsDetectes)
            if (comp != null) comp.enabled = true;

        // Réactive les objets
        foreach (var obj in objetsADesactiver)
            if (obj != null) obj.SetActive(true);

        if (monCadenas != null) Destroy(monCadenas);
        if (monBoutonDebloquer != null) monBoutonDebloquer.SetActive(false);

        Debug.Log($"Zone '{nomZone}' débloquée ! -{coutDeblocage} pièces.");
    }

    void AfficherPasAssez()
    {
        if (monTextePasAssez == null) return;
        monTextePasAssez.SetActive(true);

        if (playerTransform != null)
        {
            Vector3 posEcran = mainCamera.WorldToScreenPoint(
                playerTransform.position + Vector3.up * 3f
            );
            if (posEcran.z > 0)
                monTextePasAssez.transform.position = posEcran;
        }

        CancelInvoke(nameof(CacherPasAssez));
        Invoke(nameof(CacherPasAssez), 2f);
    }

    void CacherPasAssez()
    {
        if (monTextePasAssez != null) monTextePasAssez.SetActive(false);
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
            if (monBoutonDebloquer != null) monBoutonDebloquer.SetActive(false);
        }
    }

    public void DebloquerGratuitement()
    {
        if (!estVerrouillee) return;
        Debloquer();
    }

    public bool EstVerrouillee() => estVerrouillee;
}
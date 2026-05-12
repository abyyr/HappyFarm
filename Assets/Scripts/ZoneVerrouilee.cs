using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneVerrouilee : MonoBehaviour
{
    [Header("Déblocage")]
    public int coutDeblocage = 50;
    public bool estVerrouillee = true;

    [Header("Prefabs")]
    public GameObject cadenasPrefab;
    public GameObject boutonDebloquerPrefab;
    public GameObject textePasAssezPrefab;
    public Canvas canvas;

    [Header("Position Cadenas")]
    public float hauteurCadenas = 3f;

    // Objets instanciés
    private GameObject monCadenas;
    private GameObject monBoutonDebloquer;
    private GameObject monTextePasAssez;
    private bool playerDedans = false;
    private Transform playerTransform;
    private Camera mainCamera;

    // Composants
    private ZoneCulture zoneCulture;

    void Start()
    {
        mainCamera = Camera.main;
        zoneCulture = GetComponent<ZoneCulture>();

        // Désactive ZoneCulture si verrouillée
        if (zoneCulture != null)
            zoneCulture.enabled = !estVerrouillee;

        // Crée le cadenas
        if (cadenasPrefab != null && estVerrouillee)
        {
            Vector3 pos = transform.position + Vector3.up * hauteurCadenas;
            monCadenas = Instantiate(cadenasPrefab, pos, Quaternion.identity);
        }

        // Crée bouton Débloquer
        if (boutonDebloquerPrefab != null && canvas != null)
        {
            monBoutonDebloquer = Instantiate(boutonDebloquerPrefab, canvas.transform);
            monBoutonDebloquer.SetActive(false);

            Button btn = monBoutonDebloquer.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(TenterDeblocage);
        }

        // Crée texte Pas assez
        if (textePasAssezPrefab != null && canvas != null)
        {
            monTextePasAssez = Instantiate(textePasAssezPrefab, canvas.transform);
            monTextePasAssez.SetActive(false);
        }
    }

    void Update()
    {
        if (!estVerrouillee) return;

        GererBouton();
        GererCadenas();

        // Touche E pour débloquer
        if (playerDedans && Input.GetKeyDown(KeyCode.E))
            TenterDeblocage();
    }

    void GererCadenas()
    {
        if (monCadenas == null) return;

        // Rotation du cadenas
        monCadenas.transform.Rotate(Vector3.up, 45f * Time.deltaTime);
    }

    void GererBouton()
    {
        if (monBoutonDebloquer == null) return;

        if (playerDedans && estVerrouillee && playerTransform != null)
        {
            monBoutonDebloquer.SetActive(true);

            Vector3 posEcran = mainCamera.WorldToScreenPoint(
                playerTransform.position + Vector3.up * 2f
            );

            if (posEcran.z > 0)
                monBoutonDebloquer.transform.position = posEcran;
        }
        else
        {
            monBoutonDebloquer.SetActive(false);
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

        // Assez de pièces ?
        if (GestionnaireArgent.instance.APiecesSuffisantes(coutDeblocage))
        {
            GestionnaireArgent.instance.DepensesPieces(coutDeblocage);
            Debloquer();
        }
        else
        {
            Debug.Log("Pas assez de pieces ! "
                + GestionnaireArgent.instance.pieces
                + "/" + coutDeblocage);
            AfficherPasAssez();
        }
    }

    void Debloquer()
    {
        estVerrouillee = false;

        // Supprime le cadenas
        if (monCadenas != null)
            Destroy(monCadenas);

        // Cache le bouton
        if (monBoutonDebloquer != null)
            monBoutonDebloquer.SetActive(false);

        // Active ZoneCulture
        if (zoneCulture != null)
            zoneCulture.enabled = true;

        Debug.Log("Zone debloquee ! -" + coutDeblocage + " pieces");
    }

    void AfficherPasAssez()
    {
        if (monTextePasAssez == null) return;

        monTextePasAssez.SetActive(true);

        // Cache après 2 secondes
        Invoke("CacherPasAssez", 2f);

        // Position au dessus du player
        if (playerTransform != null)
        {
            Vector3 posEcran = mainCamera.WorldToScreenPoint(
                playerTransform.position + Vector3.up * 3f
            );
            if (posEcran.z > 0)
                monTextePasAssez.transform.position = posEcran;
        }
    }

    void CacherPasAssez()
    {
        if (monTextePasAssez != null)
            monTextePasAssez.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger touché par : " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            playerDedans = true;
            playerTransform = other.transform;
            Debug.Log("Player detecte dans zone verrouillee !");
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

            if (monBoutonDebloquer != null)
                monBoutonDebloquer.SetActive(false);
        }
    }
}
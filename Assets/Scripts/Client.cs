using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Client : MonoBehaviour
{
    [Header("Paramètres")]
    public float vitesse = 3f;
    public float tempsAttente = 3f;

    public enum EtatClient
    {
        Marche,
        Attente,
        Achete,
        Repart
    }

    public EtatClient etat = EtatClient.Marche;

    private string produitDemande;
    private int quantiteDemandee;

    private GameObject maBulle;
    private Camera mainCamera;

    private Vector3 positionFile;
    private Vector3 positionDepart;
    private float tempsAttenteActuel = 0f;

    [Header("Prefabs")]
    public GameObject bullePrefab;
    public Canvas canvas;

    private bool pret = false; // IX

    void Start()
    {
        mainCamera = Camera.main;

        if (SpawnerClients.instance != null)
            positionDepart = SpawnerClients.instance.GetPointSortie();
        else
            positionDepart = transform.position + Vector3.back * 10f;

        if (bullePrefab != null && canvas != null)
        {
            maBulle = Instantiate(bullePrefab, canvas.transform);
            maBulle.SetActive(false);
        }
    }

    void Update()
    {
        if (!pret) return; // IX : attend SetPositionFile

        switch (etat)
        {
            case EtatClient.Marche:
                MarcherVersFile();
                break;

            case EtatClient.Attente:
                Attendre();
                break;

            case EtatClient.Achete:
                Acheter();
                break;

            case EtatClient.Repart:
                Repartir();
                break;
        }

        MettreAJourBulle();
    }

    void MarcherVersFile()
    {
        Vector3 direction = (positionFile - transform.position).normalized;
        transform.position += direction * vitesse * Time.deltaTime;
        transform.LookAt(positionFile);

        if (Vector3.Distance(transform.position, positionFile) < 0.3f)
        {
            transform.position = positionFile;
            etat = EtatClient.Attente;
            GenererCommande();
        }
    }

    void Attendre()
    {
        tempsAttenteActuel += Time.deltaTime;

        if (tempsAttenteActuel >= tempsAttente)
        {
            etat = EtatClient.Achete;
            tempsAttenteActuel = 0f;
        }
    }

    void Acheter()
    {
        if (Stand.instance == null)
        {
            Debug.LogWarning("Stand.instance est null !");
            etat = EtatClient.Repart;
            return;
        }

        if (Stand.instance.VendreProduit(produitDemande, quantiteDemandee))
        {
            Debug.Log("Client satisfait ! Vendu " + quantiteDemandee + " " + produitDemande);
        }
        else
        {
            Debug.Log("Stock insuffisant — client déçu !");
        }

        etat = EtatClient.Repart;

        if (SpawnerClients.instance != null)
            SpawnerClients.instance.ClientTermine(this);
    }

    void Repartir()
    {
        if (maBulle != null)
            maBulle.SetActive(false);

        Vector3 direction = (positionDepart - transform.position).normalized;
        transform.position += direction * vitesse * Time.deltaTime;
        transform.LookAt(positionDepart);

        if (Vector3.Distance(transform.position, positionDepart) < 0.5f)
        {
            Destroy(gameObject);
        }
    }

    void GenererCommande()
    {
        string[] produits = { "Blé", "Oeuf", "Tomate", "Maïs", "Carotte" };
        produitDemande = produits[Random.Range(0, produits.Length)];
        quantiteDemandee = Random.Range(1, 4);

        if (maBulle != null)
        {
            maBulle.SetActive(true);
            TextMeshProUGUI tmp = maBulle.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = "Je veux " + quantiteDemandee + " " + produitDemande + " !";
        }

        Debug.Log("Commande : " + quantiteDemandee + " " + produitDemande);
    }

    void MettreAJourBulle()
    {
        if (maBulle == null || !maBulle.activeSelf) return;
        if (mainCamera == null) return;

        Vector3 posEcran = mainCamera.WorldToScreenPoint(
            transform.position + Vector3.up * 2f
        );

        if (posEcran.z > 0)
            maBulle.transform.position = posEcran;
    }

    public void SetPositionFile(Vector3 pos)
    {
        positionFile = pos;
        pret = true; // IX : maintenant le client peut bouger
    }

    public void SetPrefabs(GameObject bulle, Canvas c)
    {
        bullePrefab = bulle;
        canvas = c;
    }
}
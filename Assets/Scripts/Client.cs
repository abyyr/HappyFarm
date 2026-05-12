using UnityEngine;
using TMPro;
using System;

public class Client : MonoBehaviour
{
    [Header("Parametres")]
    public float vitesse = 3f;
    public float tempsAttente = 5f;

    [Header("Points")]
    public Transform pointFile;
    public Transform pointSortie;

    [Header("UI")]
    public GameObject bullePrefab;
    public Canvas canvas;

    public enum EtatClient { Marche, Attente, Achete, Repart }
    public EtatClient etat = EtatClient.Marche;

    public Action OnClientTermine;

    private string produitDemande;
    private int quantiteDemandee;
    private float tempsAttenteActuel = 0f;
    private GameObject maBulle;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (bullePrefab != null && canvas != null)
        {
            maBulle = Instantiate(bullePrefab, canvas.transform);
            maBulle.SetActive(false);
        }
    }

    void Update()
    {
        switch (etat)
        {
            case EtatClient.Marche: MarcherVersFile(); break;
            case EtatClient.Attente: Attendre(); break;
            case EtatClient.Achete: Acheter(); break;
            case EtatClient.Repart: Repartir(); break;
        }

        MettreAJourBulle();
    }

    void MarcherVersFile()
    {
        if (pointFile == null) return;

        Vector3 cible = new Vector3(
            pointFile.position.x,
            transform.position.y,
            pointFile.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position, cible, vitesse * Time.deltaTime
        );

        Vector3 dir = cible - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, cible) < 0.3f)
        {
            transform.position = cible;
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
        if (Stand.instance != null)
        {
            if (Stand.instance.VendreProduit(produitDemande, quantiteDemandee))
                Debug.Log("Client satisfait !");
            else
                Debug.Log("Stock insuffisant !");
        }
        etat = EtatClient.Repart;
    }

    void Repartir()
    {
        if (maBulle != null) maBulle.SetActive(false);

        if (pointSortie == null)
        {
            Terminer();
            return;
        }

        Vector3 cible = new Vector3(
            pointSortie.position.x,
            transform.position.y,
            pointSortie.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position, cible, vitesse * Time.deltaTime
        );

        Vector3 dir = cible - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, cible) < 0.5f)
            Terminer();
    }

    void Terminer()
    {
        OnClientTermine?.Invoke();
        Destroy(gameObject);
    }

    void GenererCommande()
    {
        string[] produits = { "Ble", "Oeuf", "Tomate", "Mais", "Carotte" };
        // correction : UnityEngine.Random explicite
        produitDemande = produits[UnityEngine.Random.Range(0, produits.Length)];
        quantiteDemandee = UnityEngine.Random.Range(1, 4);

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
}
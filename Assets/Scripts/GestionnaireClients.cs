using UnityEngine;
using System.Collections;

public class GestionnaireClients : MonoBehaviour
{
    public static GestionnaireClients instance;

    [Header("Prefab")]
    public GameObject clientPrefab;

    [Header("Points")]
    public Transform pointDepart;
    public Transform pointFile;
    public Transform pointSortie;

    [Header("Parametres")]
    public float delaiEntreClients = 10f;
    public int maxClientsEnMemeTemps = 5;

    [Header("UI")]
    public GameObject bullePrefab;
    public Canvas canvas;

    private int nombreClientsActifs = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(LancerClients());
    }

    IEnumerator LancerClients()
    {
        while (true)
        {
            yield return new WaitForSeconds(delaiEntreClients);

            if (nombreClientsActifs < maxClientsEnMemeTemps)
                SpawnerUnClient();
        }
    }

    void SpawnerUnClient()
    {
        if (clientPrefab == null || pointDepart == null) return;

        GameObject go = Instantiate(
            clientPrefab,
            pointDepart.position,
            Quaternion.identity
        );

        Client client = go.GetComponent<Client>();
        if (client == null)
        {
            Destroy(go);
            return;
        }

        // Assigne les points directement
        client.pointFile = pointFile;
        client.pointSortie = pointSortie;
        client.bullePrefab = bullePrefab;
        client.canvas = canvas;

        client.OnClientTermine += () => nombreClientsActifs--;

        nombreClientsActifs++;
        Debug.Log("Client spawne ! Actifs : " + nombreClientsActifs);
    }
}
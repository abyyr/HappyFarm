using UnityEngine;
using System.Collections.Generic;

public class SpawnerClients : MonoBehaviour
{
    public static SpawnerClients instance;

    [Header("Spawn")]
    public GameObject clientPrefab;
    public Transform pointSpawn;
    public Transform pointSortie;
    public float intervalleSpawn = 10f;
    public int maxClients = 5;

    [Header("File d'attente")]
    public Transform pointFile;
    public float espacementFile = 1.5f;

    [Header("Prefabs UI")]
    public GameObject bullePrefab;
    public Canvas canvas;

    private List<Client> fileAttente = new List<Client>();
    private float tempsDepuisDernierSpawn = 0f;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        tempsDepuisDernierSpawn += Time.deltaTime;

        if (tempsDepuisDernierSpawn >= intervalleSpawn
            && fileAttente.Count < maxClients)
        {
            SpawnerUnClient();
            tempsDepuisDernierSpawn = 0f;
        }
    }

    void SpawnerUnClient()
    {
        if (clientPrefab == null)
        {
            Debug.LogError("Client Prefab manquant sur SpawnerClients !");
            return;
        }

        if (pointSpawn == null)
        {
            Debug.LogError("PointSpawn manquant sur SpawnerClients !");
            return;
        }

        if (pointFile == null)
        {
            Debug.LogError("PointFile manquant sur SpawnerClients !");
            return;
        }

        // Spawn du client à PointSpawn
        GameObject clientGO = Instantiate(
            clientPrefab,
            pointSpawn.position,
            Quaternion.identity
        );

        Client client = clientGO.GetComponent<Client>();
        if (client == null)
        {
            Debug.LogError("Le prefab Client n'a pas de script Client !");
            Destroy(clientGO);
            return;
        }

        // Assigne les prefabs UI
        client.SetPrefabs(bullePrefab, canvas);

        // Calcule la position dans la file
        int indexFile = fileAttente.Count;
        Vector3 posFile = pointFile.position
            + pointFile.forward * (indexFile * espacementFile);

        // FIX : SetPositionFile active le mouvement
        client.SetPositionFile(posFile);

        fileAttente.Add(client);

        Debug.Log("Nouveau client spawné ! File : " + fileAttente.Count);
    }

    public void ClientTermine(Client client)
    {
        fileAttente.Remove(client);

        // Réorganise la file
        for (int i = 0; i < fileAttente.Count; i++)
        {
            Vector3 posFile = pointFile.position
                + pointFile.forward * (i * espacementFile);
            fileAttente[i].SetPositionFile(posFile);
        }

        Debug.Log("Client parti. File restante : " + fileAttente.Count);
    }

    public Vector3 GetPointSortie()
    {
        if (pointSortie != null)
            return pointSortie.position;

        Debug.LogWarning("PointSortie non assigné !");
        return Vector3.zero;
    }
}   
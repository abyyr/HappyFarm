using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneCulture : MonoBehaviour
{
    [Header("Type de culture")]
    public string nomCulture = "Blé";
    public GameObject grainePrefab;
    public int piecesRecolte = 10;

    [Header("Croissance")]
    public float tempsPoussee = 30f;
    public Vector3 tailleDepart = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 tailleFin = new Vector3(1f, 1f, 1f);

    [Header("Densité")]
    public float espacement = 0.5f;
    public float offsetAleatoire = 0.2f;

    [Header("Flèche 3D")]
    public GameObject flechePrefab;
    public float hauteurFleche = 2f;
    public float bounceSpeed = 2f;
    public float bounceHeight = 0.3f;
    public float rotationSpeed = 90f;

    [Header("UI")]
    public GameObject boutonPlanterPrefab;
    public GameObject boutonRecolterPrefab;
    public GameObject barreCroissancePrefab;
    public GameObject texteRecolterPrefab;
    public Canvas canvas;

    // État
    private bool estPlantee = false;
    private bool estPrete = false;
    private float tempsPlantation = 0f;
    private BoxCollider zoneCollider;
    private bool playerDedans = false;
    private Transform playerTransform;
    private Camera mainCamera;

    // Flèche
    private GameObject maFleche;
    private Vector3 flecheBasePos;

    // UI
    private GameObject monBoutonPlanter;
    private GameObject monBoutonRecolter;
    private GameObject maBarreCroissance;
    private GameObject monTexteRecolter;
    private Slider slider;

    // Plantes
    private System.Collections.Generic.List<GameObject> plantes
        = new System.Collections.Generic.List<GameObject>();

    // Compteur global
    private static System.Collections.Generic.Dictionary<string, int> compteurs
        = new System.Collections.Generic.Dictionary<string, int>();
    private static TextMeshProUGUI texteCompteur;

    void Start()
    {
        mainCamera = Camera.main;
        zoneCollider = GetComponent<BoxCollider>();

        // Trouve TexteCompteur
        if (texteCompteur == null)
        {
            GameObject go = GameObject.Find("TexteCompteur");
            if (go != null)
                texteCompteur = go.GetComponent<TextMeshProUGUI>();
        }

        // Init compteur
        if (!compteurs.ContainsKey(nomCulture))
            compteurs[nomCulture] = 0;

        // Crée la flèche
        if (flechePrefab != null)
        {
            Vector3 centreWorld = transform.TransformPoint(zoneCollider.center);
            Vector3 pos = centreWorld + Vector3.up * hauteurFleche;
            maFleche = Instantiate(flechePrefab, pos, Quaternion.identity);
            flecheBasePos = pos;
            maFleche.SetActive(false);
        }

        // Crée bouton Planter
        if (boutonPlanterPrefab != null && canvas != null)
        {
            monBoutonPlanter = Instantiate(boutonPlanterPrefab, canvas.transform);
            monBoutonPlanter.SetActive(false);
            Button btn = monBoutonPlanter.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(Planter);
        }

        // Crée bouton Récolter
        if (boutonRecolterPrefab != null && canvas != null)
        {
            monBoutonRecolter = Instantiate(boutonRecolterPrefab, canvas.transform);
            monBoutonRecolter.SetActive(false);
            Button btn = monBoutonRecolter.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(Recolter);
        }

        // Crée barre croissance
        if (barreCroissancePrefab != null && canvas != null)
        {
            maBarreCroissance = Instantiate(barreCroissancePrefab, canvas.transform);
            maBarreCroissance.SetActive(false);
            slider = maBarreCroissance.GetComponent<Slider>();
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 0f;
            }
        }

        // Crée texte Récolter
        if (texteRecolterPrefab != null && canvas != null)
        {
            monTexteRecolter = Instantiate(texteRecolterPrefab, canvas.transform);
            monTexteRecolter.SetActive(false);
        }
    }

    void Update()
    {
        GererFleche();
        GererBoutons();

        // Touche E
        if (playerDedans && Input.GetKeyDown(KeyCode.E))
        {
            if (!estPlantee) Planter();
            else if (estPrete) Recolter();
        }

        // Croissance
        if (estPlantee && !estPrete)
        {
            float progression = (Time.time - tempsPlantation) / tempsPoussee;
            progression = Mathf.Clamp01(progression);

            // Grandir les plantes
            Vector3 tailleActuelle = Vector3.Lerp(tailleDepart, tailleFin, progression);
            foreach (GameObject plante in plantes)
            {
                if (plante != null)
                    plante.transform.localScale = tailleActuelle;
            }

            // Barre de progression
            if (slider != null)
            {
                slider.value = progression;

                Image fill = maBarreCroissance
                    .transform.Find("Fill Area/Fill")
                    ?.GetComponent<Image>();

                if (fill != null)
                    fill.color = Color.Lerp(Color.red, Color.green, progression);
            }

            // Position barre au dessus zone
            if (maBarreCroissance != null && maBarreCroissance.activeSelf)
            {
                Vector3 centreWorld = transform.TransformPoint(zoneCollider.center);
                Vector3 posEcran = mainCamera.WorldToScreenPoint(
                    centreWorld + Vector3.up * (hauteurFleche + 1f)
                );
                if (posEcran.z > 0)
                    maBarreCroissance.transform.position = posEcran;
            }

            // Prête !
            if (progression >= 1f)
            {
                estPrete = true;
                if (monTexteRecolter != null)
                    monTexteRecolter.SetActive(true);
                Debug.Log("Pret a recolter : " + nomCulture);
            }
        }

        // Clignotement texte Récolter
        if (estPrete && monTexteRecolter != null && monTexteRecolter.activeSelf)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * 3f));
            TextMeshProUGUI tmp = monTexteRecolter.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                Color c = tmp.color;
                c.a = alpha;
                tmp.color = c;
            }

            // Position texte au dessus zone
            Vector3 centreWorld = transform.TransformPoint(zoneCollider.center);
            Vector3 posEcran = mainCamera.WorldToScreenPoint(
                centreWorld + Vector3.up * hauteurFleche
            );
            if (posEcran.z > 0)
                monTexteRecolter.transform.position = posEcran;
        }
    }

    void GererFleche()
    {
        if (maFleche == null) return;

        if (playerDedans && !estPlantee)
        {
            maFleche.SetActive(true);

            float newY = flecheBasePos.y +
                Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

            maFleche.transform.position = new Vector3(
                flecheBasePos.x,
                newY,
                flecheBasePos.z
            );

            maFleche.transform.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            maFleche.SetActive(false);
        }
    }

    void GererBoutons()
    {
        if (playerTransform == null) return;

        Vector3 posEcran = mainCamera.WorldToScreenPoint(
            playerTransform.position + Vector3.up * 2f
        );

        // Bouton Planter
        if (monBoutonPlanter != null)
        {
            bool afficher = playerDedans && !estPlantee;
            monBoutonPlanter.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonPlanter.transform.position = posEcran;
        }

        // Bouton Récolter
        if (monBoutonRecolter != null)
        {
            bool afficher = playerDedans && estPrete;
            monBoutonRecolter.SetActive(afficher);
            if (afficher && posEcran.z > 0)
                monBoutonRecolter.transform.position = posEcran;
        }

        // Barre croissance
        if (maBarreCroissance != null)
            maBarreCroissance.SetActive(estPlantee && !estPrete);
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

            if (monBoutonPlanter != null)
                monBoutonPlanter.SetActive(false);
            if (monBoutonRecolter != null)
                monBoutonRecolter.SetActive(false);
        }
    }

    public void Planter()
    {
        if (estPlantee) return;
        if (grainePrefab == null)
        {
            Debug.LogError("Graine Prefab manquant !");
            return;
        }
        if (zoneCollider == null) return;

        Vector3 centreWorld = transform.TransformPoint(zoneCollider.center);
        float largeur = zoneCollider.size.z * transform.lossyScale.z;
        float longueur = zoneCollider.size.x * transform.lossyScale.x;

        float startX = centreWorld.x - largeur / 2f;
        float startZ = centreWorld.z - longueur / 2f;

        plantes.Clear();
        int total = 0;

        for (float x = startX; x < startX + largeur; x += espacement)
        {
            for (float z = startZ; z < startZ + longueur; z += espacement)
            {
                float ox = Random.Range(-offsetAleatoire, offsetAleatoire);
                float oz = Random.Range(-offsetAleatoire, offsetAleatoire);

                Vector3 pos = new Vector3(
                    x + ox,
                    centreWorld.y + 0.1f,
                    z + oz
                );

                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject plante = Instantiate(grainePrefab, pos, rot);
                plante.transform.localScale = tailleDepart;
                plantes.Add(plante);
                total++;
            }
        }

        compteurs[nomCulture] += total;
        MettreAJourCompteur();

        estPlantee = true;
        tempsPlantation = Time.time;
        playerDedans = false;
        playerTransform = null;

        if (monBoutonPlanter != null) monBoutonPlanter.SetActive(false);
        if (maBarreCroissance != null) maBarreCroissance.SetActive(true);

        Debug.Log("Plante ! " + total + " " + nomCulture);
    }

    public void Recolter()
    {
        if (!estPrete) return;

        // Sauvegarde le nombre AVANT de vider
        int nombrePlantes = plantes.Count;

        // Supprime toutes les plantes
        foreach (GameObject plante in plantes)
        {
            if (plante != null)
                Destroy(plante);
        }
        plantes.Clear();

        // Ajoute pièces ET stock
        if (GestionnaireArgent.instance != null)
        {
            GestionnaireArgent.instance.AjouterPieces(piecesRecolte);
            GestionnaireArgent.instance.AjouterRecolte(nomCulture, nombrePlantes);
        }

        Debug.Log("Recolte ! +" + piecesRecolte
            + " pieces | +" + nombrePlantes + " " + nomCulture);

        // Reset zone
        estPlantee = false;
        estPrete = false;
        playerDedans = false;
        playerTransform = null;

        if (monBoutonRecolter != null) monBoutonRecolter.SetActive(false);
        if (maBarreCroissance != null) maBarreCroissance.SetActive(false);
        if (monTexteRecolter != null) monTexteRecolter.SetActive(false);
    }

    void MettreAJourCompteur()
    {
        if (texteCompteur == null) return;
        string texte = "Cultures :\n";
        foreach (var kvp in compteurs)
            texte += kvp.Key + " : " + kvp.Value + "\n";
        texteCompteur.text = texte;
    }
}
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Composant générique pour tout animal producteur (poulet, vache, mouton...).
/// Configurez les paramètres dans l'inspecteur selon l'animal voulu.
/// </summary>
public class Animal : MonoBehaviour
{
    [Header("Identité")]
    [Tooltip("Nom du type d'animal (ex: Poulet, Vache, Mouton)")]
    public string typeAnimal = "Poulet";

    [Tooltip("Nom du produit fabriqué (ex: Oeuf, Lait, Laine)")]
    public string nomProduit = "Oeuf";

    [Header("Production")]
    [Tooltip("Temps en secondes pour produire après avoir été nourri")]
    public float tempsProduction = 20f;

    [Tooltip("Nombre maximum de produits par cycle")]
    public int maxProduits = 3;

    [Tooltip("Nombre minimum de produits par cycle")]
    public int minProduits = 1;

    [Header("Prefabs")]
    [Tooltip("Prefab du produit à instancier autour de l'animal")]
    public GameObject produitPrefab;

    [Tooltip("Rayon de dispersion des produits autour de l'animal")]
    public float rayonDispersion = 0.8f;

    [Tooltip("Hauteur des produits instanciés")]
    public float hauteurProduit = 0.1f;

    // --- État interne ---
    private bool estNourri = false;
    private bool aProduits = false;
    private int nombreProduits = 0;
    private float tempsNourri = 0f;
    private List<GameObject> produitsInstancies = new List<GameObject>();

    void Update()
    {
        if (estNourri && !aProduits)
        {
            float progression = (Time.time - tempsNourri) / tempsProduction;
            if (progression >= 1f)
                Produire();
        }
    }

    /// <summary>Nourrit l'animal et lance le cycle de production.</summary>
    public void NourririDepuisGestionnaire()
    {
        estNourri = true;
        tempsNourri = Time.time;
        Debug.Log($"{gameObject.name} ({typeAnimal}) nourri !");
    }

    void Produire()
    {
        aProduits = true;
        nombreProduits = Random.Range(minProduits, maxProduits + 1);
        produitsInstancies.Clear();

        for (int i = 0; i < nombreProduits; i++)
        {
            if (produitPrefab == null) continue;

            float angle = i * (360f / nombreProduits);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * rayonDispersion,
                hauteurProduit,
                Mathf.Sin(rad) * rayonDispersion
            );

            GameObject produit = Instantiate(
                produitPrefab,
                transform.position + offset,
                Quaternion.identity
            );
            produitsInstancies.Add(produit);
        }

        Debug.Log($"{gameObject.name} a produit {nombreProduits} {nomProduit}(s) !");
    }

    /// <summary>Ramasse tous les produits et remet l'animal à zéro.</summary>
    public void RamasserDepuisGestionnaire()
    {
        foreach (GameObject produit in produitsInstancies)
        {
            if (produit != null) Destroy(produit);
        }
        produitsInstancies.Clear();

        aProduits = false;
        estNourri = false;
        nombreProduits = 0;

        // Animation de collecte si disponible
        FarmerAnimator fa = FindObjectOfType<FarmerAnimator>();
        if (fa != null) fa.JouerCollecte();
    }

    // --- Accesseurs ---
    public bool EstNourri() => estNourri;
    public bool AProduits() => aProduits;
    public int GetNombreProduits() => nombreProduits;
    public string GetNomProduit() => nomProduit;

    /// <summary>Retourne la progression de production entre 0 et 1.</summary>
    public float GetProgression()
    {
        if (!estNourri || aProduits) return aProduits ? 1f : 0f;
        return Mathf.Clamp01((Time.time - tempsNourri) / tempsProduction);
    }
}
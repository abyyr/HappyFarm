using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestionnaireArgent : MonoBehaviour
{
    // Singleton — accessible partout
    public static GestionnaireArgent instance;

    [Header("Pièces")]
    public int pieces = 0;

    [Header("UI")]
    public TextMeshProUGUI textePieces;
    public TextMeshProUGUI texteStock;

    // Stock des récoltes
    private System.Collections.Generic.Dictionary<string, int> stockRecoltes
        = new System.Collections.Generic.Dictionary<string, int>();

    void Awake()
    {
        // Singleton
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        MettreAJourUI();
    }

    // ========================
    // GESTION PIÈCES
    // ========================

    public void AjouterPieces(int montant)
    {
        pieces += montant;
        MettreAJourUI();
        Debug.Log("+ " + montant + " pieces | Total : " + pieces);
    }

    public bool DepensesPieces(int montant)
    {
        if (pieces >= montant)
        {
            pieces -= montant;
            MettreAJourUI();
            Debug.Log("- " + montant + " pieces | Total : " + pieces);
            return true;
        }
        else
        {
            Debug.Log("Pas assez de pieces ! " + pieces + "/" + montant);
            return false;
        }
    }

    public bool APiecesSuffisantes(int montant)
    {
        return pieces >= montant;
    }

    // ========================
    // GESTION STOCK RÉCOLTES
    // ========================

    public void AjouterRecolte(string nomCulture, int quantite)
    {
        if (!stockRecoltes.ContainsKey(nomCulture))
            stockRecoltes[nomCulture] = 0;

        stockRecoltes[nomCulture] += quantite;
        MettreAJourUI();
        Debug.Log("+ " + quantite + " " + nomCulture 
            + " | Stock : " + stockRecoltes[nomCulture]);
    }

    public bool UtiliserRecolte(string nomCulture, int quantite)
    {
        if (!stockRecoltes.ContainsKey(nomCulture))
        {
            Debug.Log("Pas de " + nomCulture + " en stock !");
            return false;
        }

        if (stockRecoltes[nomCulture] >= quantite)
        {
            stockRecoltes[nomCulture] -= quantite;
            MettreAJourUI();
            Debug.Log("- " + quantite + " " + nomCulture 
                + " | Stock : " + stockRecoltes[nomCulture]);
            return true;
        }
        else
        {
            Debug.Log("Pas assez de " + nomCulture 
                + " ! " + stockRecoltes[nomCulture] + "/" + quantite);
            return false;
        }
    }

    public int GetStock(string nomCulture)
    {
        if (stockRecoltes.ContainsKey(nomCulture))
            return stockRecoltes[nomCulture];
        return 0;
    }

    public bool AStockSuffisant(string nomCulture, int quantite)
    {
        return GetStock(nomCulture) >= quantite;
    }

    // ========================
    // UI
    // ========================

    void MettreAJourUI()
    {
        // Pièces
        if (textePieces != null)
            textePieces.text = "Pieces : " + pieces;

        // Stock récoltes
        if (texteStock != null)
        {
            string texte = "Stock :\n";
            foreach (var kvp in stockRecoltes)
            {
                if (kvp.Value > 0)
                    texte += kvp.Key + " : " + kvp.Value + "\n";
            }
            texteStock.text = texte;
        }
    }
}
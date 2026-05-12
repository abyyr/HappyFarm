using UnityEngine;

public class Poulet : MonoBehaviour
{
    [Header("Paramètres")]
    public float tempsProductionOeuf = 20f;
    public int maxOeufs = 3;

    [Header("Prefabs")]
    public GameObject oeufPrefab;

    // État
    private bool estNourri = false;
    private bool aDesOeufs = false;
    private int nombreOeufs = 0;
    private float tempsNourriture = 0f;

    // Oeufs
    private System.Collections.Generic.List<GameObject> oeufsInstancies
        = new System.Collections.Generic.List<GameObject>();

    void Update()
    {
        if (estNourri && !aDesOeufs)
        {
            float progression = (Time.time - tempsNourriture) / tempsProductionOeuf;
            if (progression >= 1f)
                ProduireOeufs();
        }
    }

    // Appelé par GestionnairePoulets
    public void NourririDepuisGestionnaire()
    {
        estNourri = true;
        tempsNourriture = Time.time;
        Debug.Log(gameObject.name + " nourri !");
    }

    void ProduireOeufs()
    {
        aDesOeufs = true;
        nombreOeufs = Random.Range(1, maxOeufs + 1);

        oeufsInstancies.Clear();
        for (int i = 0; i < nombreOeufs; i++)
        {
            if (oeufPrefab != null)
            {
                float angle = i * (360f / nombreOeufs);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(rad) * 0.8f,
                    0.1f,
                    Mathf.Sin(rad) * 0.8f
                );
                GameObject oeuf = Instantiate(
                    oeufPrefab,
                    transform.position + offset,
                    Quaternion.identity
                );
                oeufsInstancies.Add(oeuf);
            }
        }
        Debug.Log(gameObject.name + " a pondu " + nombreOeufs + " oeuf(s) !");
    }

    // Appelé par GestionnairePoulets
    public void RamasserDepuisGestionnaire()
    {
        foreach (GameObject oeuf in oeufsInstancies)
        {
            if (oeuf != null) Destroy(oeuf);
        }
        oeufsInstancies.Clear();

        aDesOeufs = false;
        estNourri = false;
        nombreOeufs = 0;
    }

    // Getters
    public bool EstNourri() { return estNourri; }
    public bool ADesOeufs() { return aDesOeufs; }
    public int GetNombreOeufs() { return nombreOeufs; }
}
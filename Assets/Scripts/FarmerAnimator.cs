using UnityEngine;

public class FarmerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (animator == null) return;
        Vector3 vitesse = rb != null ? rb.linearVelocity : Vector3.zero;
        vitesse.y = 0;
        bool bouge = vitesse.magnitude > 0.1f;
        animator.SetBool("isWalking", bouge);
    }

    public void JouerRecolte()
    {
        if (animator == null) return;
        animator.SetBool("isHarvesting", true);
        animator.SetBool("isGathering", false);
        Invoke("StopperRecolte", 5f);
    }

    void StopperRecolte()
    {
        if (animator != null)
            animator.SetBool("isHarvesting", false);
    }

    public void JouerCollecte()
    {
        if (animator == null) return;
        animator.SetBool("isGathering", true);
        animator.SetBool("isHarvesting", false);
        Invoke("StopperCollecte", 2f);
    }

    void StopperCollecte()
    {
        if (animator != null)
            animator.SetBool("isGathering", false);
    }
}
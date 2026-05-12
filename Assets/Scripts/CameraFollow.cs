using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Position caméra")]
    public float offsetY = 5f;
    public float offsetZ = -7f;
    public float smoothSpeed = 10f;

    [Header("Rotation souris")]
    public float mouseSpeed = 100f;
    private float angleY = 0f;

    void Start()
    {
        angleY = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Rotation horizontale avec la souris
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
        angleY += mouseX;

        // Position fixe autour du joueur
        Quaternion rotation = Quaternion.Euler(0, angleY, 0);
        Vector3 desiredPos = target.position
            + rotation * new Vector3(0, offsetY, offsetZ);

        // LERP fluide mais sans zoom
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
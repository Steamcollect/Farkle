using UnityEngine;

public class CameraController : MonoBehaviour
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] Camera cam;

    //[Header("Input")]
    [Header("Output")]
    [SerializeField] RSO_Camera mainCam;

    private void Awake()
    {
        mainCam.Value = cam;
    }

    public Camera GetCamera() { return cam; }
}
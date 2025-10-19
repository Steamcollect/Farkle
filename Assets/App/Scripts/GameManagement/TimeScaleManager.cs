using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(.5f, 4)] float timeScale;

    //[Header("References")]
    //[Header("Input")]
    //[Header("Output")]

    private void Start()
    {
        Time.timeScale = timeScale;
    }
}
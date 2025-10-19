using UnityEngine;

public class Dice : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float maxLinearVelocityToCheck;
    [SerializeField] float maxAngulatVelocityToCheck;
    [SerializeField] float securityAngle;

    [Space(10)]
    [SerializeField] LayerMask outlineLayer;
    LayerMask defaultLayer;

    int currentValue;

    float lockTimer = 0;

    [Header("References")]
    [SerializeField] Transform[] faces;

    [Space(10)]
    [SerializeField] Rigidbody rb;

    //[Header("Input")]
    //[Header("Output")]

    private void Start()
    {
        defaultLayer = gameObject.layer.ToLayerMask();
    }

    private void Update()
    {
        lockTimer += Time.deltaTime;
    }

    public bool CheckValue()
    {
        if (rb.linearVelocity.sqrMagnitude > maxAngulatVelocityToCheck) return false;
        if (rb.angularVelocity.sqrMagnitude > maxAngulatVelocityToCheck) return false;

        for (int i = 0; i < faces.Length; i++)
        {
            if (Vector3.Dot(faces[i].up, Vector3.up) >= securityAngle)
            {
                currentValue = i + 1;
                return true;
            }
        }

        return false;
    }

    public int GetValue() { return currentValue; }

    public void DisablePhysics()
    {
        rb.isKinematic = true;
    }
    public void EnablePhysics()
    {
        rb.isKinematic = false;
    }

    public void ShowOutline()
    {
        gameObject.layer = outlineLayer.ToLayer();
    }
    public void HideOutline()
    {
        gameObject.layer = defaultLayer.ToLayer();
    }

    public void ResetLockTimer() { lockTimer = 0; }
    public float GetLockTimer() { return lockTimer; }

    public Rigidbody GetRigidbody() { return rb; }
}
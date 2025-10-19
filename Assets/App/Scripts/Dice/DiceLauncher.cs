using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceLauncher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int diceCount;

    [Space(5)]
    [SerializeField] Vector2 minMaxLauchForce;
    [SerializeField] Vector2 minMaxRotationForce;

    [Space(5)]
    [SerializeField] float maxRangeAroundLaunchPoint;

    [Space(10)]
    [SerializeField] float faceCheckDelay;
    [SerializeField] float maxLockTime;

    [Header("References")]
    [SerializeField] Dice DicePrefab;

    [Space(5)]
    [SerializeField] Transform launchPoint;

    List<Dice> dices = new();

    [Header("Input")]
    [SerializeField] RSE_LaunchDices launchDices;

    //[Header("Output")]
    System.Action onDicesScored;

    //private void OnEnable()
    //{
    //    lauchAction.action.started += (InputAction.CallbackContext context) => LaunchDices();
    //}
    //private void OnDisable()
    //{
    //    lauchAction.action.started -= (InputAction.CallbackContext context) => LaunchDices();
    //}

    private void OnEnable()
    {
        launchDices.Action += LaunchDices;
    }
    private void OnDisable()
    {
        launchDices.Action -= LaunchDices;
    }

    private void Start()
    {
        for (int i = 0; i < diceCount; i++)
        {
            Dice dice = Instantiate(DicePrefab, transform);
            dices.Add(dice);
        }

        LaunchDices(dices.ToArray());

        onDicesScored += OnDicesScored;
    }

    void LaunchDices(Dice[] _dices)
    {
        foreach (Dice dice in _dices)
        {
            dice.EnablePhysics();

            dice.transform.position = launchPoint.position + Random.insideUnitSphere * maxRangeAroundLaunchPoint;
            dice.GetRigidbody().AddForce(launchPoint.forward * Random.Range(minMaxLauchForce.x, minMaxLauchForce.y));
            dice.GetRigidbody().AddTorque(launchPoint.forward * Random.Range(minMaxRotationForce.x, minMaxRotationForce.y));
        }

        StartCoroutine(CheckDicesFaces());
    }

    void OnDicesScored()
    {
        print("All dices scored");

        foreach (var dice in dices)
        {
            dice.DisablePhysics();
        }
    }

    IEnumerator CheckDicesFaces()
    {
        yield return new WaitForSeconds(.3f);

        List<Dice> _dices = new(dices);

        while(_dices.Count > 0)
        {
            for (int i = _dices.Count - 1; i >= 0; i--)
            {
                if (_dices[i].CheckValue())
                {
                    _dices.RemoveAt(i);
                }
                else if (_dices[i].GetLockTimer() >= maxLockTime)
                {
                    _dices[i].ResetLockTimer();
                    _dices[i].GetRigidbody().AddForceAtPosition(Vector3.up * Random.Range(minMaxLauchForce.x, minMaxLauchForce.y), Random.insideUnitSphere);
                    _dices[i].GetRigidbody().AddTorque(Vector3.up * Random.Range(minMaxRotationForce.x, minMaxRotationForce.y));
                }
            }

            yield return new WaitForSeconds(faceCheckDelay);
        }

        onDicesScored?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(launchPoint.position, maxRangeAroundLaunchPoint);
    }
}
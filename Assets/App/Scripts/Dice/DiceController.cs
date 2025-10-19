using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DiceController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float rayDistance = 10;

    //[Header("References")]
    Dice currentDiceHoveride;
    List<Dice> dicesSelected = new();

    [Header("Input")]
    [SerializeField] InputActionReference mousePosition;
    [SerializeField] InputActionReference selectionAction;
    [SerializeField] InputActionReference lauchAction;

    [Space(5)]
    [SerializeField] RSO_Camera mainCam;

    [Header("Output")]
    [SerializeField] RSE_LaunchDices launchDices;

    private void OnEnable()
    {
        selectionAction.action.started += (InputAction.CallbackContext context) => OnInteract();
        lauchAction.action.started += (InputAction.CallbackContext context) => LaunchDices();
    }
    private void OnDisable()
    {
        selectionAction.action.started -= (InputAction.CallbackContext context) => OnInteract();
        lauchAction.action.started -= (InputAction.CallbackContext context) => LaunchDices();
    }

    private void FixedUpdate()
    {
        UpdateHover();
    }

    void UpdateHover()
    {
        if (GetMouseHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject.TryGetComponent(out Dice dice) && dice.CheckValue())
            {
                if (currentDiceHoveride != null && dice != currentDiceHoveride)
                    HideOutline();

                currentDiceHoveride = dice;
                currentDiceHoveride.ShowOutline();
            }
            else HideOutline();
        }
        else HideOutline();
    }

    void OnInteract()
    {
        if(currentDiceHoveride != null)
        {
            if (dicesSelected.Contains(currentDiceHoveride))
                dicesSelected.Remove(currentDiceHoveride);
            else 
                dicesSelected.Add(currentDiceHoveride);
        }
    }

    void LaunchDices()
    {
        if(dicesSelected.Count > 0)
        {
            foreach (Dice dice in dicesSelected)
            {
                dice.HideOutline();
                dice.ResetLockTimer();
            }

            launchDices.Call(dicesSelected.ToArray());
            dicesSelected.Clear();
        }
    }

    void HideOutline()
    {
        if(currentDiceHoveride != null)
        {
            if (!dicesSelected.Contains(currentDiceHoveride))
                currentDiceHoveride.HideOutline();

            currentDiceHoveride = null;
        }
    }

    bool GetMouseHit(out RaycastHit hit)
    {
        Ray ray = mainCam.Value.ScreenPointToRay(mousePosition.action.ReadValue<Vector2>());

        //Debug.DrawRay(ray.origin, ray.GetPoint(1), Color.blue, 1);

        if (Physics.Raycast(ray, out RaycastHit _hit, rayDistance))
        {
            hit = _hit;
            return true;
        }

        hit = _hit;
        return false;
    }
}
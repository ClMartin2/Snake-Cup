using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cup : MonoBehaviour
{
    [SerializeField] private InputActionReference click;

    private void Awake()
    {
        click.action.Enable();
        click.action.performed += ClickPerformed;
        click.action.canceled += ClickCanceled; ;

    }

    private void ClickCanceled(InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }

    private void ClickPerformed(InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator FollowMouse()
    {

        yield return null;
    }
}

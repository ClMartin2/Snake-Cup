using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cup : MonoBehaviour
{
    [SerializeField] private InputActionReference click;
    [SerializeField] private float maxXLength = 500;

    [Header("Ball Settings")]
    [SerializeField] private int numberStartBall = 4;
    [SerializeField] private string ballKeyPool = "Ball";
    [SerializeField] private float delayToSpawnBall = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugMobile = false;

    private Coroutine coroutineFollowMouse;
    private float counterToSpawnBall;
    private float counterNumberOfAvailableBall;

    private void Awake()
    {
        click.action.Enable();
        click.action.performed += ClickPerformed;
        click.action.canceled += ClickCanceled;
        counterNumberOfAvailableBall = numberStartBall;
    }

    private void ClickCanceled(InputAction.CallbackContext obj)
    {
       StopCoroutine(coroutineFollowMouse);
       coroutineFollowMouse = null;
    }

    private void ClickPerformed(InputAction.CallbackContext obj)
    {
        counterToSpawnBall = 0;
        coroutineFollowMouse = StartCoroutine(UpdateMouseOnClick());
    }

    private IEnumerator UpdateMouseOnClick()
    {
        while (true)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldMousePosition = Application.isMobilePlatform ? Touchscreen.current.touches[0].position.ReadValue() : Camera.main.ScreenToWorldPoint(mousePosition);
            float clampedX = Mathf.Clamp(worldMousePosition.x, -maxXLength, maxXLength);
            Vector2 cupPosition = new Vector2(clampedX, transform.position.y);

            transform.position = cupPosition;

            counterToSpawnBall += Time.deltaTime;

            if (counterNumberOfAvailableBall > 0 && counterToSpawnBall >= delayToSpawnBall) {
                Transform ball = ObjectPoolManager.Instance.Get(ballKeyPool).transform;

                ball.position = cupPosition;
                counterToSpawnBall = 0;

                counterNumberOfAvailableBall--;
            }

            yield return null;
        }
    }

    [ContextMenu("AddBall")]
    private void AddBall()
    {
        counterNumberOfAvailableBall++;
    }

    private void OnDestroy()
    {
        click.action.performed -= ClickPerformed;
        click.action.canceled -= ClickCanceled;
    }
}

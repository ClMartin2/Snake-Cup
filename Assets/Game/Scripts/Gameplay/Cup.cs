using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void CupEventHandler(Cup sender, Ball ball);

public class Cup : MonoBehaviour
{
    [SerializeField] private InputActionReference click;
    [SerializeField] private TextMeshPro txtNumberOfBallAvailable;

    [Header("Ball Settings")]
    [SerializeField] private int numberStartBall = 4;
    [SerializeField] private string ballKeyPool = "Ball";
    [SerializeField] private float delayToSpawnBall = 0.5f;
    [SerializeField] private float delayToHaveNewBall = 0.1f;
    [SerializeField] private int numberOfNewBall = 2;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("Debug")]
    [SerializeField] private bool debugMobile = false;

    public event CupEventHandler CreateBall;

    private Coroutine coroutineFollowMouse;
    private float counterToSpawnBall;
    private float counterNumberOfAvailableBall;
    private float counterToHaveNewBall = 0;
    private bool pause = false;

    private void Awake()
    {
        click.action.Enable();
        click.action.performed += ClickPerformed;
        click.action.canceled += ClickCanceled;
        counterNumberOfAvailableBall = numberStartBall;
        UpdateTxtBallAvailable();
    }

    public void Pause(bool _pause)
    {
        pause = _pause;

        if (pause)
        {
            click.action.Disable();
            StopCoroutineFollow();
        }
        else
        {
            click.action.Enable();
        }
    }

    public void Restart()
    {
        counterNumberOfAvailableBall = numberStartBall;
        counterToHaveNewBall = 0;
        UpdateTxtBallAvailable();
    }

    private void StopCoroutineFollow()
    {
        if (coroutineFollowMouse != null)
        {
            StopCoroutine(coroutineFollowMouse);
            coroutineFollowMouse = null;
        }
    }

    private void UpdateTxtBallAvailable()
    {
        txtNumberOfBallAvailable.text = counterNumberOfAvailableBall.ToString();
    }

    private void ClickCanceled(InputAction.CallbackContext obj)
    {
        StopCoroutineFollow();
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
            float clampedX = Mathf.Clamp(worldMousePosition.x, GameManager.Limit.x, GameManager.Limit.y);
            Vector2 cupPosition = new Vector2(clampedX, transform.position.y);

            transform.position = cupPosition;

            counterToSpawnBall += Time.deltaTime;

            if (counterNumberOfAvailableBall > 0 && counterToSpawnBall >= delayToSpawnBall) {
                Transform ball = ObjectPoolManager.Instance.Get(ballKeyPool).transform;

                ball.position = ballSpawnPoint.position;
                counterToSpawnBall = 0;

                counterNumberOfAvailableBall--;

                CreateBall?.Invoke(this, ball.GetComponent<Ball>());
                UpdateTxtBallAvailable();
            }

            yield return null;
        }
    }

    private void Update()
    {
        if (pause)
            return;

        counterToHaveNewBall += Time.deltaTime;

        if(counterToHaveNewBall >= delayToHaveNewBall)
        {
            counterNumberOfAvailableBall += numberOfNewBall;
            counterToHaveNewBall = 0;
            UpdateTxtBallAvailable();
        }
    }

    [ContextMenu("AddBall")]
    private void AddBall()
    {
        counterNumberOfAvailableBall++;
        UpdateTxtBallAvailable();
    }

    private void OnDestroy()
    {
        click.action.performed -= ClickPerformed;
        click.action.canceled -= ClickCanceled;
    }
}

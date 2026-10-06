using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Waiting, Playing, Won }

    [Header("Player")]
    public PlayerStats player;
    public Transform playerSpawn;

    [Header("Timer")]
    public float roundTime = 120f;

    [Header("Events")]
    public UnityEvent onGameStarted;
    public UnityEvent onGameStopped;
    public UnityEvent onGameReset;

    public GameState State { get; private set; }
    public float TimeLeft { get; private set; }

    private Rigidbody playerRb;
    private int lastPrintedSecond = -1;

    private void Awake()
    {
        Instance = this;
        playerRb = player.GetComponent<Rigidbody>();
        TimeLeft = roundTime;
        State = GameState.Waiting;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;

        int second = Mathf.CeilToInt(TimeLeft);
        if (second != lastPrintedSecond)
        {
            lastPrintedSecond = second;
            Debug.Log($"Time left: {second}");
        }

        if (TimeLeft <= 0f)
        {
            Debug.Log("Time's up!");
            ResetGame();
        }
    }

    public void StartGame()
    {
        if (State != GameState.Waiting) return;

        State = GameState.Playing;
        TimeLeft = roundTime;
        lastPrintedSecond = -1;

        Debug.Log("Game started!");
        onGameStarted?.Invoke();
    }

    public void WinGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Won;

        Debug.Log($"You win! Time left: {TimeLeft:F1}s");
        onGameStopped?.Invoke();
    }

    public void PlayerDied()
    {
        Debug.Log("Player died!");
        ResetGame();
    }

    public void ResetGame()
    {
        State = GameState.Waiting;
        TimeLeft = roundTime;

        onGameStopped?.Invoke();
        onGameReset?.Invoke();

        RespawnPlayer();
        player.ResetStats();

        Debug.Log("Game reset.");
    }

    private void RespawnPlayer()
    {
        playerRb.linearVelocity = Vector3.zero;
        player.transform.position = playerSpawn.position;
        Physics.SyncTransforms();
    }
}
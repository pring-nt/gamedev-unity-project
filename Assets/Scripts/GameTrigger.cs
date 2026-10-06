using UnityEngine;

public class GameTrigger : MonoBehaviour
{
    public enum TriggerType { Start, Win }

    [Header("Type")]
    public TriggerType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerStats>() == null) return;

        if (type == TriggerType.Start)
            GameManager.Instance.StartGame();
        else
            GameManager.Instance.WinGame();
    }
}
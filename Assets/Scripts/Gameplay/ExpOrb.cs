using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    // 1개당 경험치 증가량
    [SerializeField] public float expValue = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.GetXP(expValue);
            Destroy(gameObject);
        }
    }
}

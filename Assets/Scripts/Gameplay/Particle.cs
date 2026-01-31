using UnityEngine;
using System.Collections;

public class Particle : MonoBehaviour
{
    [SerializeField] private PoolType particleType;

    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        ps.Play();
        StartCoroutine(AutoDespawn(ps.main.duration + ps.main.startLifetime.constantMax));
    }

    IEnumerator AutoDespawn(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        ObjectPoolManager.Instance.Despawn(gameObject, particleType);
    }
}
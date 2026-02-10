using UnityEngine;
using System.Collections;

public class BarkEffect : MonoBehaviour
{
    public float duration = 0.3f;
    private float currentRadius;
    private float currentAngle;

    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void PlayEffect(float radius, float angle, Transform playerTransform, Quaternion rotation, Vector3 firePoint)
    {
        StopAllCoroutines();

        currentRadius = radius;
        currentAngle = angle;

        transform.SetParent(playerTransform);
        transform.localPosition = firePoint;
        transform.localRotation = rotation;  // 방향

        StartCoroutine(BarkRoutine());
    }

    // 부채꼴 범위 표시
    private IEnumerator BarkRoutine()
    {
        int segments = 20;              // 호 점 갯수
        int vertexCount = segments + 2; // 꼭짓점 + 마지막

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        float startAngle = -currentAngle / 2f + 90f; // 위 기준
        float angleStep = currentAngle / segments;

        // 극좌표 -> 직교좌표
        for (int i = 1; i < vertexCount; i++)
        {
            float rad = Mathf.Deg2Rad * (startAngle + (i - 1) * angleStep);
            vertices[i] = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * currentRadius;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        yield return new WaitForSeconds(duration);

        // 비활성화
        transform.SetParent(ObjectPoolManager.Instance.transform);
        ObjectPoolManager.Instance.Despawn(gameObject, PoolType.BarkEffect);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
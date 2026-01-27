using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 카메라 변수
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    // 맵 범위
    public float maxX, minX, maxY, minY;

    public void MapRange(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    // 마지막에 동작
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 화면 이탈 방지
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
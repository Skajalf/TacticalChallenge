using UnityEngine;
using UnityEngine.InputSystem; // Input System 네임스페이스 추가

public static class CameraHelpers
{
    // 새 Input System에서 마우스 위치를 가져오는 유틸리티 메서드 추가
    private static Vector2 GetMousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    public static bool GetCursorLocation(float distance, LayerMask mask)
    {
        Vector3 position;
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    public static bool GetCursorLocation(out Vector3 position, float distance, LayerMask mask)
    {
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    public static bool GetCursorLocation(out Vector3 position, out Vector3 normal, float distance, LayerMask mask)
    {
        position = Vector3.zero;
        normal = Vector3.zero;

        // 새로운 Input System을 사용하여 마우스 위치를 가져옴
        Vector2 mousePosition = GetMousePosition();

        // 마우스 위치에서 레이캐스트를 쏘기 위해 레이를 생성
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            position = hit.point;
            normal = hit.normal;
            return true;
        }

        return false;
    }
}

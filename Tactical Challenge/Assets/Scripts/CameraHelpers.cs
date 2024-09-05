using UnityEngine;
using UnityEngine.InputSystem; // Input System ���ӽ����̽� �߰�

public static class CameraHelpers
{
    // �� Input System���� ���콺 ��ġ�� �������� ��ƿ��Ƽ �޼��� �߰�
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

        // ���ο� Input System�� ����Ͽ� ���콺 ��ġ�� ������
        Vector2 mousePosition = GetMousePosition();

        // ���콺 ��ġ���� ����ĳ��Ʈ�� ��� ���� ���̸� ����
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

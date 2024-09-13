using Unity.VisualScripting;
using UnityEngine;

public static class Extend_TransformHelpers
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in transforms)
        {
            if (t.gameObject.name.Equals(name))
                return t;
        }

        return null;
    }
}

public static class Extend_GameObjects
{


    /// <summary>
    /// ĳ���� ������Ʈ�� �̸����� Character Ŭ������ ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Character</returns>
    public static Character FindCharacterByName(string name)
    {
        Character[] chr = GameObject.FindObjectsByType<Character>(0);

        foreach(var c in chr)
        {
            if(c.name.Equals(name)) return c;
        }

        Debug.Log("FCBN : Not Found");
        return null;
    }

    /// <summary>
    /// Character Ŭ������ �ν��Ͻ��� �ش� ������Ʈ�� weapon ���� (Weapon) �� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns>Weapon, null</returns>
    public static Weapon FindWeaponByCharacter(Character chr)
    {
        Weapon weapon = chr.gameObject.GetComponent<WeaponComponent>().GetWeaponInfo();
        if(weapon != null)
        {
            return weapon;
        }
        return null;
    }


}

public static class MathHelpers
{
    public static bool IsNearlyEqual(float a, float b, float tolerance = 1e-6f)
    {
        return Mathf.Abs(a - b) <= tolerance;
    }

    public static bool IsNearlyZero(float a, float b, float tolerance = 1e-6f)
    {
        return Mathf.Abs(a) <= tolerance;
    }
}
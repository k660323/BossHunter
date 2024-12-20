using Mirror;
using System;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = null;

        if (go.TryGetComponent(out component) == false)
        {
            component = go.AddComponent<T>();
        }

        return component;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    if (transform.TryGetComponent(out T component))
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        System.Reflection.FieldInfo[] fields = type.GetFields();

        if (destination.TryGetComponent(out T copy))
        {
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }

            return copy as T;
        }
        else
        {
            Component com = destination.AddComponent(type);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(com, field.GetValue(original));
            }

            return com as T;
        }

    }

    public static void CopyComponent<T>(T original, T destination) where T : Component
    {
        System.Type type = original.GetType();

        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(destination, field.GetValue(original));
        }
    }

    // enum Ÿ���� string���� ��ȯ
    public static string GetEnumToString<T>(T type) where T : Enum
    {
        string name = System.Enum.GetName(typeof(T), type);
        return name;
    }

    static string[] strItemGrades = new string[6] { "Ŀ��", "�븻", "����", "����ũ", "������", "����" };
    static Color[] itemGradesColor = new Color[6] { Color.gray, Color.white, new Color(0.4f, 0.2f, 0.8f), new Color(0.7f, 0.3f, 0.6f), new Color(0.8f, 0.4f, 0.1f), new Color(0.9f, 0.7f, 0.1f) };
    public static string GetGradeToString(Define.ItemGrade itemGrade)
    {
        return strItemGrades[(int)itemGrade];
    }

    public static Color GetGradeToColor(Define.ItemGrade itemGrade)
    {
        return itemGradesColor[(int)itemGrade];
    }

    static string[] strItemSubTypes = new string[15] { "-", "���� ����", "���Ÿ� ����", "Ÿ�� ����", "���", "�Ӹ����", "����", "����", "��Ʈ", "�Ź�", "����", "�����", "����", "�Һ�", "��Ÿ" };
    public static string GetSubTypeToString(Define.ItemSubType itemSubType)
    {
        return strItemSubTypes[(int)itemSubType];
    }

    public static bool IsSameScene(GameObject targetObject)
    {
        // ȣ��Ʈ�� ���� ���� üũ���� �ʰ� true�� ��ȯ�Ѵ�.
        if (NetworkClient.activeHost == false)
            return true;

        return NetworkClient.localPlayer != null && NetworkClient.localPlayer.gameObject.scene == targetObject.scene;
    }
}

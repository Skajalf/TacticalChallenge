using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleNode
{
    public bool hasChild;
    public RectTransform rectTransform;
}

public class UIToggleAnimation : MonoBehaviour
{
    [SerializeField] private float defaultDistance = 20f;  // �⺻ UI�� �Ÿ�
    [SerializeField] private float toggleUIDistance = 10f; // ������ UI���� �Ÿ� (������ ���� ���� �Ϲ���)
    [SerializeField] private float moveSpeed = 5f;        // UI�� �̵��ӵ�

    private Dictionary<RectTransform, ToggleNode> toggleTree = new();
    private List<ToggleNode> toggleList = new(); // ���� ������

    private Vector2 togglePosition; // ��� UI�� ���� ��ġ -> Monobehaviour ��ũ��Ʈ�� �پ��ִ� ������Ʈ ����

    private void Start()
    {
        OnInitialized(); // �ӽ÷� �׽�Ʈ �ϱ� ���� �κ�, �̺�Ʈ�� �ٲ㼭 ��ũ��Ʈ ������ �����ϴ� ���� ���� �� ����. 
    }

    private void OnInitialized() // �ʱ�ȭ��
    {
        toggleTree.Clear();
        toggleList.Clear();

        foreach (RectTransform obj in gameObject.transform as RectTransform)
        {
            ToggleNode parentNode = new();
            toggleTree[obj] = parentNode;
            if (obj.childCount != 0)
            {
                //temp.hasChild = true;
                List<ToggleNode> tl = new List<ToggleNode>();
                foreach (RectTransform obj2 in obj.transform as RectTransform)
                {
                    ToggleNode tempChilds = new();
                    tempChilds.rectTransform = obj2;
                    tempChilds.hasChild = false; // ��ư �Ʒ� ��ư �ϳ� �� �߰��ϴ°� ���Ƶξ���..
                    tl.Add(tempChilds);
                }
                //toggle.Add(temp, tl);
                continue;
            }
            //toggle.Add(temp, null);
        }

        togglePosition = gameObject.transform.position;
        
    }
}

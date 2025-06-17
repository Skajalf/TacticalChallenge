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
    [SerializeField] private float defaultDistance = 20f;  // 기본 UI간 거리
    [SerializeField] private float toggleUIDistance = 10f; // 펼쳐진 UI와의 거리 (위보다 좁은 것이 일반적)
    [SerializeField] private float moveSpeed = 5f;        // UI의 이동속도

    private Dictionary<RectTransform, ToggleNode> toggleTree = new();
    private List<ToggleNode> toggleList = new(); // 순서 관리용

    private Vector2 togglePosition; // 토글 UI의 기준 위치 -> Monobehaviour 스크립트가 붙어있는 오브젝트 기준

    private void Start()
    {
        OnInitialized(); // 임시로 테스트 하기 위한 부분, 이벤트로 바꿔서 스크립트 순서를 지정하는 것이 좋을 것 같다. 
    }

    private void OnInitialized() // 초기화부
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
                    tempChilds.hasChild = false; // 버튼 아래 버튼 하나 더 추가하는건 막아두었음..
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

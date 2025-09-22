using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class DefenceFlowManager : MonoBehaviour
{
    #region Singleton
    public static DefenceFlowManager instance;
    private DefenceFlowManager() { }

    public static DefenceFlowManager GetInstance()
    {
        if(instance == null)
            instance = new DefenceFlowManager();
        return instance;
    }
    #endregion

    //�ʿ��Ѱ�, ���̵� ����, �ð� ����, ĳ���� ����, ���� ����, ���� ��ų ����
    
    public void SceneInitiation() 
    {

    }
}

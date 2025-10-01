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

    //필요한게, 난이도 정보, 시간 정보, 캐릭터 정보, 무기 정보, 지원 스킬 정보
    
    public void SceneInitiation() 
    {

    }
}

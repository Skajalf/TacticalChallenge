using UnityEngine;
using UnityEngine.UI;

//TODO: 스탯 컴포넌트 체력말고 AP도 추가해야 함.
//public class StatComponent : MonoBehaviour
//{
//    [SerializeField]
//    private float maxHealthPoint = 100.0f;
//    private float currHealthPoint;

//    [SerializeField]
//    private string uiPlayerName = "Image_HealthBar_Foreground";

//    [SerializeField]
//    private string uiEnemyName = "EnemyHealthBar";


//    private Image userInterface;
//    private Canvas uiEnemyCanvas;

//    public bool Dead { get => currHealthPoint <= 0.0f; }

//    private void Start()
//    {
//        currHealthPoint = maxHealthPoint;

//        if (GetComponent<Player>() != null)
//        {
//            GameObject ui = GameObject.Find(uiPlayerName);
//            Debug.Assert(ui != null);

//            userInterface = ui.GetComponent<Image>();
//            Debug.Assert(userInterface != null);
//        }
//        else if (GetComponent<Enemy>() != null)
//        {
//            uiEnemyCanvas = UIHelpers.CreateBillboardCanvas(uiEnemyName, transform, Camera.main);

//            Transform t = uiEnemyCanvas.transform.FindChildByName("Image_Foreground");
//            userInterface = t.GetComponent<Image>();
//        }
//    }

//    public void Damage(float amount)
//    {
//        if (amount < 1.0f)
//            return;

//        currHealthPoint += (amount * -1.0f);
//        currHealthPoint = Mathf.Clamp(currHealthPoint, 0.0f, maxHealthPoint);

//        if (userInterface != null)
//            userInterface.fillAmount = currHealthPoint / maxHealthPoint;
//    }

//    private void Update()
//    {
//        if (uiEnemyCanvas)
//            uiEnemyCanvas.transform.rotation = Camera.main.transform.rotation;
//    }
//}
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Debugging : MonoBehaviour
{
    private Character[] character;
    private Player player;
    private Enemy enemy;

    private Text maintext;
    private string playerText;
    private string enemyText;

    private StatComponent playerStat;
    private StatComponent enemyStat;


    public void Awake()
    {
        Init();
    }

    public void Update()
    {
        UpdatePlayerText();
        UpdateEnemyText();
        UpdateText();
    }

    public void Init()
    {
        maintext = GetComponent<Text>();

        character = FindObjectsByType<Character>(0);
        foreach (Character c in character)
        {
            if( c.name == "CH0137")
                player = c as Player;
            if( c.name == "Dummy")
                enemy = c as Enemy;
        }

        enemyStat = enemy.statComponent;
        playerStat = player.statComponent;
    }

    public void UpdateText()
    {
        maintext.text = $"Debugging /----------\n{playerText} /----------\n{enemyText} ";
    }
    public void UpdatePlayerText()
    {
        playerText = $"player";
    }

    public void UpdateEnemyText()
    {
        enemyText = $"Enemy";
    }


}

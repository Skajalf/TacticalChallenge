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

    private Weapon playerWeapon;

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

        /*
        character = GameObject.FindObjectsByType<Character>(0);
        foreach (Character c in character)
        {
            if( c.name == "CH0137")
                player = c as Player;
            if( c.name == "Dummy")
                enemy = c as Enemy;
        }
        */

        player = (Player)Extend_GameObjects.FindCharacterByName("CH0137");
        enemy = (Enemy)Extend_GameObjects.FindCharacterByName("Dummy");

        playerWeapon = Extend_GameObjects.FindWeaponByCharacter(player);
    }

    public void UpdateText()
    {
        maintext.text = $"Debugging\n----------\n{playerText}/\n----------\n{enemyText}/ ";
    }
    public void UpdatePlayerText()
    {
        playerText = $"player\nHP : {player.statComponent.CurrentHP}/\nAP : {player.statComponent.CurrentAP}/\n" +
            $"Ammo :{playerWeapon.weapondata.currentAmmo} / {playerWeapon.weapondata.Ammo}";
    }

    public void UpdateEnemyText()
    {
        enemyText = $"Enemy\nHP : {enemy.statComponent.CurrentHP}/\nAP : {enemy.statComponent.CurrentAP}";
    }


}

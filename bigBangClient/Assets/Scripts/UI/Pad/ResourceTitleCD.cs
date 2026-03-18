using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using TMPro;
using Utils;
using Babu;
using BigBang;

public class ResourceTitleCD : MonoBehaviour
{

    public TMP_Text timeText;
    private Timer timer;

    public void Regist()
    {
        Player.PackageManager.FixEnergy();
        UpdateTimeText();
        timer = Timer.Register(this.gameObject, 1f, OnUpdateSeconds, null, true, true);
    }
    public void UnRegist()
    {
        timer?.Cancel();
        timer = null;
    }

    private void UpdateTimeText()
    {
        timeText.gameObject.SetActive(Player.PackageManager.Energy < GameConst.PlayerMaxEnergy);
        if (Player.PackageManager.Energy < GameConst.PlayerMaxEnergy)
        {
            timeText.gameObject.SetActive(true);
            int leftSeconds = GameConst.PlayerEnergyRecoverTime - (int)((DataConvUtil.ServerTime - Player.PackageManager.EnergyLastUpdateTime) % GameConst.PlayerEnergyRecoverTime);
            timeText.text = "({0})".SafeFormat(DataConvUtil.FormatTimeLeft(leftSeconds));
        }
    }
    private void OnUpdateSeconds()
    {
        Player.PackageManager.FixEnergy();
        UpdateTimeText();
    }
}
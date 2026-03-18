using BigBang;
using BigBang.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardLearnedPad : MonoBehaviour
{
    [SerializeField] private GameObject perfab;
    [SerializeField] private RectTransform padHeightRect;
    [SerializeField] private Transform connent;//子物体的父节点
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private TMP_Text noSkillText;
    //private int connentHeight = 0;
    //private int padHeight = 0;
    private List<SkillIcon> learnedSkillList = new List<SkillIcon>();
    public Dictionary<BabuButton, Skill> skillDataDict = new Dictionary<BabuButton, Skill>();
    public List<BabuButton> btns = new List<BabuButton>();

    private PlayerCard card;
    public void SetData(PlayerCard card, List<KeyValuePair<int, PlayerCardSkill>> list)
    {
        this.card = card;
        if (list.Count == 0)
        {
            noSkillText.gameObject.SetActive(true);
            Clear();
            return;
        }
        //connentHeight = Screen.height + padHeight;
        Clear();
        noSkillText.gameObject.SetActive(false);
        list = list.OrderByDescending(item => item.Value.Config.Quality).ThenBy(item => item.Value.Id).ToList();
        InitPerfab(list);
        RefreshList(list);
    }
    private void SetPadHeight(int count)
    {
        padHeightRect.sizeDelta = new Vector2(670, (128f + 40) * Mathf.CeilToInt(count / (float)4) + 50f);
        parentRect.sizeDelta = new Vector2(padHeightRect.sizeDelta.x, padHeightRect.sizeDelta.y + 40f);
    }
    private void InitPerfab(List<KeyValuePair<int, PlayerCardSkill>> list)
    {
        int index = Mathf.Max(list.Count, connent.childCount);
        SetPadHeight(list.Count);
        if (list.Count == connent.childCount)
        {
            return;
        }
        if (index == list.Count)
        {
            for (int i = connent.childCount; i < list.Count; ++i)
            {
                GameObject clone = Instantiate(perfab, transform);
                var modelItem = clone.GetComponent<SkillIcon>();
                //modelItem.SetData(model, false);
                //learnedSkillList.Add(modelItem);
                learnedSkillList.Add(modelItem);
            }
        }
        else if (index == connent.childCount)
        {
            for (int i = index - 1; i != list.Count; --i)
                connent.GetChild(i).gameObject.SetActive(false);
        }
        else
            return;
    }

    public void UpdateLevel(int skillId)
    {
        foreach (var item in learnedSkillList)
        {
            if (item.SkillId == skillId)
            {
                item.RefreshLevel();
                return;
            }
        }
    }
    private void RefreshList(List<KeyValuePair<int, PlayerCardSkill>> list)
    {
        int index = 0;
        skillDataDict.Clear();

        foreach (var item in learnedSkillList)
        {
            if (index == list.Count)
                return;
            var model = new Skill(list[index].Key, list[index].Value.Level);
            item.SetData(model, false, false);
            connent.GetChild(index).gameObject.SetActive(true);
            BabuButton sender = item.GetComponent<BabuButton>();
            skillDataDict.Add(sender, model);
            item.GetComponent<BabuButton>().OnClick += OnClickSkill;

            ++index;
        }
    }
    private void Clear()
    {
        for (int i = connent.childCount - 1; i >= 0; i--)
        {
            connent.GetChild(i).gameObject.SetActive(false);
            connent.GetChild(i).gameObject.GetComponent<BabuButton>().OnClick -= OnClickSkill;
        }
    }

    public bool isSelf = true;
    private void OnClickSkill(BabuButton sender)
    {

        foreach (var item in learnedSkillList)
        {
            if (item.gameObject == sender.gameObject)
            {
                //if (isSelf)
                //{
                //    UIController.Instance.OpenWindow<SkillLearnUI>(new SkillLearnUIProperties(this.card.CardId, skillDataDict[sender]));
                //}
                //else
                //{
                UIController.Instance.OpenWindow<SkilltipsUI>(new SkilltipsUIProperties(skillDataDict[sender]));
                //}
            }

        }
    }
}

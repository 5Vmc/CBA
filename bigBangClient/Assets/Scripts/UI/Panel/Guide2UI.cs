using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using GameConfig;
using System.Linq;
using Babu;
using GameConfig.Config;
using Utils;
using System;
using UnityEngine.UI;
using BigBang.Animation;
using UnityTimer;

namespace BigBang.UI
{
    public class Guide2UIProperties : WindowProperties
    {
        public GuideDialogueConfig StartDialogue { get; private set; }
        public Action OnClose;

        public Guide2UIProperties(GuideDialogueConfig startDialogue, Action onClose)
        {
            StartDialogue = startDialogue;
            OnClose = onClose;
        }
    }

    public class Guide2UI : AWindowController<Guide2UIProperties>
    {
        [SerializeField] private List<GuideSelectionItem> selections;
        [SerializeField] private List<GuidePlayerIconItem> portraits;
        [SerializeField] private GuideDialogueItem leftDialoguePrefab;
        [SerializeField] private GuideDialogueItem rightDialoguePrefab;
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform bottomEmpty;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private CanvasGroup selectionCanvas;
        [SerializeField] private VerticalAdapter adapter;

        [SerializeField] public Guide2UIAnim Anim;

        // 随机抽取的球员
        private IEnumerable<PlayerCard> cards;
        // 对话气泡实例
        private List<GameObject> dialogueGameObjects = new List<GameObject>();
        // 当前的对话气泡
        private GuideDialogueConfig currentDialogue;
        // 可选择的对话气泡
        private GuideDialogueConfig[] currentSelection;
        private bool autoNext = true;

        private Action dialogueCallback = null;

        // 选项对应的阵形ID
        private int[] chooseDef = new int[] { 201, 201, 202, 202 };
        private int[] chooseAtk = new int[] { 101, 102, 101, 102 };
        // 触发选择阵形的对话ID
        private int[] formationDialogueID = { 4003, 4004, 4005, 4006 };

        protected override void Awake()
        {
            base.Awake();
            selections.ForEach(item => item.Btn.Sound = AudioNames.SWITCH_COL);
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            selections.ForEach(item => item.Btn.OnClick += OnSelect);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            selections.ForEach(item => item.Btn.OnClick -= OnSelect);
            autoNext = false;
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            // 清除对话气泡
            ClearDialogue();
            adapter.Calculate();
            scroll.ScroolToTop(0);
            currentDialogue = Properties.StartDialogue;
            // 随机抽取5位球员参与对话
            cards = Player.CardManager.CardList.Random(5);
            // 设置顶部栏头像
            foreach (var item in cards.Zip(portraits, (card, portrait) => (card, portrait)))
            {
                var sprite = await SpriteProxy.GetPlayerPortrait(item.card.Config.Portrait);
                item.portrait.SetIcon(sprite);
            }
            // 禁用关闭按钮
            closeBtn.gameObject.SetActive(false);
            autoNext = true;
            Anim.PlayEnter(OnNext);
        }

        private void OnSelect(BabuButton sender)
        {
            TouchManager.Instance.DisableTouch();
            // 选择的下标
            int index = selections.IndexOf(sender.transform.parent.GetComponent<GuideSelectionItem>());
            for (int i = 0; i < selections.Count; i++)
            {
                if (i != index)
                {
                    selections[i].PlayExit();
                }
                else
                {
                    selections[i].PlaySelected();
                }
            }
            Timer.Register(this.gameObject, 0.4f, () =>
            {
                TouchManager.Instance.EnableTouch();
                if (formationDialogueID.Contains(currentDialogue.Id))
                {
                    // 设置阵形
                    Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVE, formation =>
                    {
                        List<int> TacticIdList = new();
                        TacticIdList.Add(chooseAtk[index]);
                        TacticIdList.Add(chooseDef[index]);
                        formation.TacticsIdList = TacticIdList;
                        NetworkManager.Instance.SaveFormation(FormationID.PVE, formation, (_) =>
                        {
                            Player.CardManager.CheckRedDot(0, true);
                        });
                    });
                }
                currentDialogue = currentSelection[index];
                Anim.HideSelection();
                OnNext();
                autoNext = true;
            });
        }

        private void OnClose(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
            Anim.PlayExit(() =>
            {
                if (GuideManager.IsGuideDoing(GuideID.directorsTalk)) GuideManager.Finish(GuideID.directorsTalk);
                if (GuideManager.IsGuideDoing(GuideID.teamTalk)) GuideManager.Finish(GuideID.teamTalk);
                Properties.OnClose?.Invoke();
                UIController.Instance.CloseWindow<Guide2UI>();
                ClearDialogue();
            });
        }

        // 设置选项
        public void SetSelections(GuideDialogueConfig[] cfgs)
        {
            autoNext = false;
            selectionCanvas.transform.SetAsLastSibling();
            bottomEmpty.SetAsLastSibling();
            for (int i = 0; i < 4; i++)
            {
                selections[i].gameObject.SetActive(i < cfgs.Length);
                if (i < cfgs.Length)
                {
                    selections[i].SetContent(cfgs[i].Content);
                }
            }
            // 计算布局
            adapter.Calculate();
            DelayTaskService.Instance.Run(this.gameObject, 0.1f, () => scroll.ScrollToBottom(0.3f));
        }

        private void OnNext()
        {
            // 获得下一组对话
            GuideDialogueConfig[] next = GetNextDialogue(currentDialogue);
            // 创建对话气泡
            GuideDialogueItem dialogue = CreateDialogue(currentDialogue.Character);
            // 设置对话气泡数据
            SetDialogueData(dialogue, currentDialogue);
            // 如果没有下一组对话,则结束对话
            if (next == null)
            {
                // 对话结束
                OnOver();
                return;
            }
            // 跳到下一个对话
            currentDialogue = next.First();
            if (next.Length > 1)
            {
                SetSelections(next);
                dialogueCallback = Anim.ShowSelection;
                currentSelection = next;
            }
        }

        // 获得下一组对话
        private GuideDialogueConfig[] GetNextDialogue(GuideDialogueConfig cfg)
        {
            // 如果下一组对话ID=0,则没有下一组对话
            if (cfg.Next.Length == 1 && cfg.Next[0] == '0')
            {
                return null;
            }
            return cfg.Next.Split('|').Select(item => Configs.GuideDialogue.GetConfig(int.Parse(item))).ToArray();
        }

        // 设置对话气泡数据
        private async void SetDialogueData(GuideDialogueItem dialogue, GuideDialogueConfig cfg)
        {
            switch (cfg.Character)
            {
                // 董事会的对话气泡
                case (int)GuideCharacter.Board:
                    dialogue.SetData(await SpriteManager.GetSprite(AtlasNames.Guide, SpriteNames.Guide.Board), Lang.Get(LangID.BoardTxt), cfg.Content);
                    break;
                // 玩家的对话气泡
                case (int)GuideCharacter.Player:
                    dialogue.SetData(await SpriteManager.GetSprite(AtlasNames.Guide, SpriteNames.Guide.Me), Lang.Get(LangID.MeTxt), cfg.Content);
                    break;
                // 球员们的对话气泡
                case (int)GuideCharacter.Member:
                    // 随机抽取一个球员
                    int cardID = cards.Random().CardId;
                    dialogue.SetData(cardID, cfg.Content);
                    break;
                // 秘书的对话气泡
                case (int)GuideCharacter.Clerk:
                    dialogue.SetData(await SpriteManager.GetSprite(AtlasNames.Guide, SpriteNames.Guide.Clerk), Lang.Get(LangID.ClerkTxt), cfg.Content);
                    break;
            }
        }

        // 创建一个对话气泡
        private GuideDialogueItem CreateDialogue(int character)
        {
            GameObject dialogue;
            if (character == (int)GuideCharacter.Player)
            {
                dialogue = Instantiate(rightDialoguePrefab.gameObject, content);
                dialogueGameObjects.Add(dialogue);
            }
            else
            {
                dialogue = Instantiate(leftDialoguePrefab.gameObject, content);
                dialogueGameObjects.Add(dialogue);
            }
            // 设置对话气泡位置
            dialogue.transform.SetAsLastSibling();
            selectionCanvas.transform.SetAsLastSibling();
            bottomEmpty.SetAsLastSibling();

            // 计算布局
            adapter.Calculate();

            DelayTaskService.Instance.Run(this.gameObject, () => scroll.ScrollToBottom(0.3f));

            var dialogueItem = dialogue.GetComponent<GuideDialogueItem>();
            // 注册说话完成事件
            dialogueItem.OnDialogueFinished += OnDialogueItemFinished;
            return dialogueItem;
        }

        private void OnDialogueItemFinished()
        {
            // 说话完成触发事件
            if (!autoNext)
            {
                dialogueCallback?.Invoke();
                dialogueCallback = null;
                DelayTaskService.Instance.Run(this.gameObject, () => scroll.ScrollToBottom(0.3f));
                return;
            }
            Timer.Register(this.gameObject, 0.5f, OnNext);
        }

        // 清除所有对话气泡
        private void ClearDialogue()
        {
            dialogueGameObjects.ForEach(item => Destroy(item));
            Anim.HideSelection();
            adapter.Clear();
        }

        private void OnOver()
        {
            autoNext = false;
            Timer.Register(this.gameObject, 0.5f, () =>
            {
                closeBtn.gameObject.SetActive(true);
            });
        }
    }
}

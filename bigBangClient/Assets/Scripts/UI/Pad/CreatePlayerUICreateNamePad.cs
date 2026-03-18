using Babu;
using BigBang.Animation;
using GameConfig;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class CreatePlayerUICreateNamePad : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private BabuButton nextBtn;
        [SerializeField] private BabuButton randomBtn;
        [SerializeField] private CreatePlayerUICreateIconPad nextPad;

        [SerializeField] public CreateNamePadAnim Anim;

        public string ClubName { get; private set; } = string.Empty;

        private void OnEnable()
        {
            nextBtn.OnClick += OnNext;
            randomBtn.OnClick += OnRandomName;
            inputField.onValueChanged.AddListener(OnClubNameChanged);
            AudioManager.Instance.PlayMusic(AudioNames.BGM_TRAINING);
        }

        private void OnDisable()
        {
            nextBtn.OnClick -= OnNext;
            randomBtn.OnClick -= OnRandomName;
            inputField.onValueChanged.RemoveListener(OnClubNameChanged);
        }

        // 输入改变
        private void OnClubNameChanged(string input)
        {
            ClubName = input.Trim();
        }

        // 随机名称
        private void OnRandomName(BabuButton sender)
        {
            var cfgs = Configs.RandomClubName.GetConfigList();

            string name = "";
            while (true)
            {
                int random = Random.Range(0, cfgs.Count);
                name = cfgs[random].Name;
                if (IllegalCharacter.IsStringLegal(name)) break;
            }
            inputField.text = name;
        }


        // 下一步
        private void OnNext(BabuButton sender)
        {
            IllegalCharacter.IsNameCanNotUse(ClubName, true, (bool isCanNotUse) =>
            {
                if (isCanNotUse)
                {
                    return;
                }
                else
                {
                    gameObject.SetActive(false);
                    nextPad.gameObject.SetActive(true);
                    nextPad.Initialize();
                    nextPad.Anim.PlayEnter();
                    CreatePlayerUICreateIconPad.IsInit = true;
                }
            });
        }


    }
}
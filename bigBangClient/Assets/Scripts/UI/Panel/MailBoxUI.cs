using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class MailBoxUI : APanelController
    {
        [SerializeField] private Button deleteBtn;
        [SerializeField] private Button receiveBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private MailAdapter osa;
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private MailBoxUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            deleteBtn.onClick.AddListener(OnDeleteAll);
            receiveBtn.onClick.AddListener(OnReceiveAll);
            closeBtn.onClick.AddListener(OnClose);

            EventManager.Instance.Register(EventID.OnRefreshEmail, OnRefreshOsa);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            deleteBtn.onClick.RemoveListener(OnDeleteAll);
            receiveBtn.onClick.RemoveListener(OnReceiveAll);
            closeBtn.onClick.RemoveListener(OnClose);

            EventManager.Instance.Unregister(EventID.OnRefreshEmail, OnRefreshOsa);
        }

        private void OnRefreshOsa(object[] args)
        {
            var list = Player.EmailManager.GetMails();
            osa.SetItems(list);
            if (args != null && args.Length > 0 && (bool)args[0])
            {
                osa.InitAnim();
                osa.AnimIn();
            }
        }

        [SerializeField] private MailBoxUIGuide mailBoxUIGuide = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            NetworkManager.Instance.FetchServerTime((response) =>
            {
                Player.EmailManager.SetServerTime(response.ServerTime);
            });

            var list = Player.EmailManager.GetMails();
            osa.SetItems(list);
            Anim.PlayEnter();
            mailBoxUIGuide.CheckGuide();
        }

        private void OnDeleteAll()
        {
            osa.PlayDeleteAnim(Player.EmailManager.DeleteAllEmails);
        }

        private void OnReceiveAll()
        {
            Player.EmailManager.ReceiveAllEmails();
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            Anim.PlayExit(() => { UIController.Instance.HidePanel<MailBoxUI>(); });
        }
    }
}

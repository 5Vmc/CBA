using Babu.SDK;
using BigBang;
using BigBang.UI;
using UnityEngine;
using Utils;

namespace Babu
{
    public class NetworkStatusListener : MonoBehaviour
    {
        private static NetworkStatusListener _inst;

        private bool checkCloseFin = true;
        public static NetworkStatusListener Instance
        {
            get { return _inst; }
            private set { }
        }

        private void Awake()
        {
            _inst = this;
            EventManager.Instance.Register(SocketService.Event.Disconnected, OnDisconnected);
        }

        public void SetCloseFin(bool b)
        {
            this.checkCloseFin = b;
        }

        private void OnDisconnected(object[] args)
        {
            if (LoginManager.Instance.isNeedCloseClientAfterChangeAccount) return;
            if (LoginManager.Instance.IsBackByKickOff == true) return;
            if (LoginManager.Instance.isCheckingSilenceReLoginHeart == false
            && LoginManager.Instance.isDoingSilenceReLogin == false
            && LoginManager.Instance.isBeforeLoadingEnd == false)
            {
                LoginManager.Instance.DoSilenceReLogin();
                return;
            }
            if (LoginManager.Instance.isCheckingSilenceReLoginHeart == true) return;
            if (LoginManager.Instance.isDoingSilenceReLogin == true) return;
            UnityEngine.Debug.Log("OnDisconnected");
            HeartbeatManager.Instance.ClearAllSubscribe();
            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.NetworkConnectionTimeout), Lang.Get(LangID.ConfirmTxt), () =>
            {
                LoginManager.Instance.BackToLogin();
            }));
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unregister(SocketService.Event.Disconnected, OnDisconnected);
        }
    }
}

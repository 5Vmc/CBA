using Babu;
using BigBang.UI;
using Protocol;
using System;
using System.Collections.Generic;
using Utils;

namespace BigBang
{
    public enum EmailState
    {
        New = 1,
        READED = 2,
        CANDELETE = 3,
        DELETED = 4
    }
    public class MailInfo
    {
        public string id;
        public string title;
        public string content;
        public string receiver;
        public string sender;
        public string receiverTitle;
        public long sendTime;
        public int state;
        public List<GameItem> attachment = new List<GameItem>();

        public string GetSendTime()
        {
            return StringUtils.SecondToString(TimeUtils.Now() - Player.EmailManager.ServerTimeOffset - sendTime) + "前";
        }

        public string GetOverdueTime()
        {
            return StringUtils.SecondToString(sendTime + GameConst.EmailOverdueTime - TimeUtils.Now() + Player.EmailManager.ServerTimeOffset) + "后过期";
        }

        public bool CanReceive()
        {
            return HasAttachment() && state < (int)EmailState.CANDELETE;
        }

        public bool CanDelete()
        {
            return (!HasAttachment() && state != (int)EmailState.New) || (!CanReceive());
        }

        public bool HasAttachment()
        {
            return attachment.Count > 0;
        }

        public bool HadCollectAttachment()
        {
            return HasAttachment() && state == (int)EmailState.CANDELETE;
        }

        public bool IsOverDue()
        {
            return TimeUtils.Now() - Player.EmailManager.ServerTimeOffset - sendTime > GameConst.EmailOverdueTime;
        }

        public void Readed()
        {
            if (HasAttachment())
                state = (int)EmailState.READED;
            else
                state = (int)EmailState.CANDELETE;
        }

        public void Received()
        {
            state = (int)EmailState.CANDELETE;
        }

        public bool HasRedDot()
        {
            return state == (int)EmailState.New || state == (int)EmailState.READED;
        }
    }

    public class PlayerEmailManager
    {
        private Dictionary<string, MailInfo> _mailDic = new Dictionary<string, MailInfo>();
        public long ServerTimeOffset { get; private set; }

        public PlayerEmailManager()
        {

        }

        public void Init()
        {
            _mailDic.Clear();
            AddListenerOnce();
        }
        private bool isAddListener = false;
        private void AddListenerOnce()
        {
            if (isAddListener) return;
            isAddListener = true;
            EventManager.Instance.Register(EventID.OnRefreshEmail, OnRefreshEmail);
        }
        private void OnRefreshEmail(object[] _)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Mail, "");
            node.AddValue(HasRedDot ? 1 : -1);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void UnPack(ModuleEmailAllNotify data)
        {
            if (data == null) return;
            foreach (var email in data.EmailMap.Values)
            {
                if (!_mailDic.ContainsKey(email.EmailId))
                {
                    AddMail(email);
                }
            }
            EventManager.Instance.Dispatch(EventID.OnRefreshEmail);
        }

        public void SetServerTime(long serverTime)
        {
            ServerTimeOffset = TimeUtils.Now() - serverTime;
        }

        private void AddMail(EmailInfo info)
        {
            var mailInfo = new MailInfo()
            {
                id = info.EmailId,
                title = info.Title,
                content = info.Content,
                sender = info.Sender,
                state = info.State,
                sendTime = info.SendTime,
                receiverTitle = info.ReceiverTitle //收件人标题
            };
            foreach (var good in info.AttachmentMap.Values)
            {
                mailInfo.attachment.Add(good);
            }
            _mailDic.Add(info.EmailId, mailInfo);
        }

        /// <summary>
        /// 服务器推送来了新邮件
        /// </summary>
        public void Add(ModuleEmailNewNotify data)
        {
            AddMail(data.EmailInfo);
            Tips.PopTips("您收到一封新邮件");
            EventManager.Instance.Dispatch(EventID.OnRefreshEmail);
        }

        public List<MailInfo> GetMails()
        {
            List<MailInfo> list = new List<MailInfo>();
            foreach (var email in _mailDic.Values)
            {
                if (!email.IsOverDue())
                    list.Add(email);
            }
            list.Sort(Sort);
            return list;
        }

        public int Sort(MailInfo a, MailInfo b)
        {
            if (a.state.CompareTo(b.state) != 0)
                return a.state.CompareTo(b.state);
            else
                return -a.sendTime.CompareTo(b.sendTime);
        }

        public void ReadEmail(string emailId)
        {
            if (!_mailDic.ContainsKey(emailId)) return;
            if (_mailDic[emailId].state != (int)EmailState.New) return;
            NetworkManager.Instance.ReadEmail(emailId, (response) =>
            {
                string emailId = response.EmailId;
                if (_mailDic.ContainsKey(emailId))
                {
                    _mailDic[emailId].Readed();
                    EventManager.Instance.Dispatch(EventID.OnRefreshEmail);
                }
            });
        }

        public void DeleteEmail(string emailId)
        {
            if (!_mailDic.ContainsKey(emailId)) return;
            if (_mailDic[emailId].state != (int)EmailState.CANDELETE) return;
            NetworkManager.Instance.DeleteEmail(emailId, (response) =>
            {
                string emailId = response.EmailId;
                if (_mailDic.ContainsKey(emailId))
                {
                    _mailDic.Remove(emailId);
                    EventManager.Instance.Dispatch(EventID.OnRefreshEmail);
                }
            });
        }

        public void ReceiveEmail(string emailId)
        {
            if (!_mailDic.ContainsKey(emailId)) return;
            if (!_mailDic[emailId].CanReceive()) return;
            NetworkManager.Instance.ReceiveEmail(emailId, (response) =>
            {
                string emailId = response.EmailId;
                if (_mailDic.ContainsKey(emailId))
                {
                    _mailDic[response.EmailId].Received();
                    EventManager.Instance.Dispatch(EventID.OnRefreshEmail);
                    EventManager.Instance.Dispatch(EventID.OnReceiveEmailDetail);
                }
            });
        }

        public void ReceiveAllEmails()
        {
            List<string> emailList = new List<string>();
            List<GameItem> attachment = new();
            foreach (var email in _mailDic.Values)
            {
                if (email.CanReceive())
                {
                    emailList.Add(email.id);
                    attachment.AddRange(email.attachment);
                }
            }
            NetworkManager.Instance.ReceiveAllEmails(emailList, (response) =>
            {
                foreach (string emailId in response.EmailList)
                {
                    if (_mailDic.ContainsKey(emailId))
                    {
                        _mailDic[emailId].Received();
                    }
                }
                EventManager.Instance.Dispatch(EventID.OnRefreshEmail, true);
                if (attachment.Count > 0)
                {
                    var properties = new InventoryObtainedUIProperties(attachment);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                }
            });
        }

        public void DeleteAllEmails()
        {
            List<string> emailList = new List<string>();
            foreach (var email in _mailDic.Values)
            {
                if (email.CanDelete())
                {
                    emailList.Add(email.id);
                }
            }
            NetworkManager.Instance.DeleteAllEmails(emailList, (response) =>
            {
                foreach (string emailId in response.EmailList)
                {
                    if (_mailDic.ContainsKey(emailId))
                        _mailDic.Remove(emailId);
                }
                EventManager.Instance.Dispatch(EventID.OnRefreshEmail, true);
            });
        }

        public bool HasRedDot
        {
            get
            {
                foreach (var email in _mailDic.Values)
                {
                    if (email.HasRedDot() == true)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

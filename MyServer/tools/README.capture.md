# 抓包脚本

这两个脚本基于 Windows 自带的 `pktmon`，不用额外装 `Wireshark/tshark` 也能先抓到包。

## 文件

- `start-pktmon-capture.ps1`
- `stop-pktmon-capture.ps1`

## 使用方式

必须用“管理员 PowerShell”。

### 抓远程测试服 TCP

```powershell
cd E:\UGit\CBA_Card\MyServer\tools
.\start-pktmon-capture.ps1
```

然后回 Unity 里复现一次：

1. 进启动界面
2. 连测试服
3. 走到登录 / 拉角色 / 进主界面

完成后回 PowerShell：

```powershell
.\stop-pktmon-capture.ps1
```

输出文件在：

- `E:\UGit\CBA_Card\MyServer\captures`

## 自动分析

如果本机已经装过依赖，可以直接运行：

```powershell
python E:\UGit\CBA_Card\MyServer\tools\analyze-capture.py E:\UGit\CBA_Card\MyServer\captures\你的文件.pcapng
```

这个脚本会把主要 TCP 会话按远端 IP/端口汇总。

## 建议第一次怎么抓

第一次不要先只抓某个端口，因为我们还不知道远端测试服最终 TCP 端口是多少。  
所以第一次建议先抓“所有 TCP”，等我看完文件后，再缩小过滤条件。

## 你需要反馈给我的内容

优先给这两个文件之一：

- 最新的 `.txt`
- 或最新的 `.pcapng`

如果你只方便复制文本，就把 `.txt` 里与目标远端 IP 相关的那几段贴给我。

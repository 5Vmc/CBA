# CBA Compat Server

这个目录放的是一个最小兼容服务器骨架，目标不是复刻原测试服，而是先把客户端的登录链路和首批通知跑通。

## 当前包含

- `src/CbaCompatServer`：`.NET 8` 单进程服务
- HTTP 选服接口：
  - `/cba/server_selector.php`
  - `/cba/server_list.php`
- TCP 游戏服骨架：
  - `cs_login`
  - `cs_fetchAllPlayers`
  - `cs_createPlayer`
  - `cs_enterGame`
  - `cs_heart`
- 首批主动推送：
  - `sc_updatePlayerInfo`
  - `sc_updateCardInfo`
  - `sc_refreshPackageInfo`
  - `sc_refreshResource`

## 运行

```powershell
cd E:\UGit\CBA_Card\MyServer\src\CbaCompatServer
dotnet run
```

默认端口在 `appsettings.json`：

- HTTP: `5000`
- TCP: `5100`

## 客户端切换方式

先把客户端的 HTTP 选服入口改到本机，再让 `server_selector.php` / `server_list.php` 返回本机 TCP 地址。

## 当前限制

- 数据只存在内存里，重启即丢失
- HTTP JSON 结构只是第一版兼容壳，真实字段还需要抓线上返回继续校正
- 抓包已确认真实协议是“请求 `2` 字节长度头，响应 `4` 字节长度头”
- 抓包已确认 response 的 `methodName` 为空串，notify 才带名字
- 只实现了最小登录链路，进入主页后仍会继续缺通知和业务接口

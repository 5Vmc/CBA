using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using CbaCompatServer.Protocol;
using CbaCompatServer.State;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Protocol;

namespace CbaCompatServer.Net;

public sealed class TcpGameServer : BackgroundService
{
    private readonly ILogger<TcpGameServer> _logger;
    private readonly InMemoryGameState _gameState;
    private readonly ServerOptions _options;
    private TcpListener? _listener;

    public TcpGameServer(
        ILogger<TcpGameServer> logger,
        InMemoryGameState gameState,
        IOptions<ServerOptions> options)
    {
        _logger = logger;
        _gameState = gameState;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new TcpListener(IPAddress.Any, _options.TcpPort);
        _listener.Start();
        _logger.LogInformation("TCP game server listening on {Port}", _options.TcpPort);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        return base.StopAsync(cancellationToken);
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remote = client.Client.RemoteEndPoint?.ToString() ?? "<unknown>";
        _logger.LogInformation("Accepted TCP client {Remote}", remote);

        using var networkStream = client.GetStream();
        var connectionState = new ConnectionState();

        try
        {
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var body = await ReadFrameAsync(networkStream, cancellationToken);
                if (body.Length == 0)
                {
                    break;
                }

                var envelope = MessagePackEnvelopeCodec.DecodeRequest(body);
                LogRequest(remote, envelope, connectionState.Session);

                var responses = Dispatch(envelope, connectionState);
                foreach (var response in responses)
                {
                    LogOutgoing(remote, envelope, response, connectionState.Session);
                    await WriteFrameAsync(networkStream, MessagePackEnvelopeCodec.EncodeResponse(response), cancellationToken);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or InvalidDataException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Client {Remote} disconnected or sent invalid data.", remote);
        }
        finally
        {
            client.Close();
            _logger.LogInformation("Closed TCP client {Remote}", remote);
        }
    }

    private IEnumerable<MessageEnvelope> Dispatch(MessageEnvelope envelope, ConnectionState connectionState)
    {
        return envelope.MethodName switch
        {
            "cs_login" => HandleLogin(envelope, connectionState),
            "cs_fetchAllPlayers" => HandleFetchAllPlayers(envelope, connectionState),
            "cs_checkName" => HandleCheckName(envelope),
            "cs_createPlayer" => HandleCreatePlayer(envelope, connectionState),
            "cs_enterGame" => HandleEnterGame(envelope, connectionState),
            "activity_module.cs_receiveReward" => HandleReceiveSevenDayReward(envelope, connectionState.Session),
            "arena_module.cs_arenaInfo" => HandleArenaInfo(envelope, connectionState.Session),
            "card_module.cs_recruit" => HandleRecruit(envelope, connectionState.Session),
            "fight_module.cs_saveFormation" => HandleSaveFormation(envelope, connectionState.Session),
            "fight_module.cs_getDefaultFormationRequest" => HandleGetDefaultFormation(envelope, connectionState.Session),
            "pvp_module.cs_getLeagueData" => HandleGetLeagueData(envelope, connectionState.Session),
            "pvp_module.cs_leagueSignUp" => HandleGetLeagueSignUp(envelope, connectionState.Session),
            "pvp_module.cs_getLeagueHistory" => HandleGetLeagueHistory(envelope, connectionState.Session),
            "pvp_module.cs_getLeagueChampionRank" => HandleGetLeagueChampionRank(envelope, connectionState.Session),
            "pvp_module.cs_getLeagueCourse" => HandleGetLeagueCourse(envelope, connectionState.Session),
            "pvp_module.cs_getLeagueCardRank" => HandleGetLeagueCardRank(envelope, connectionState.Session),
            "train.cs_doOfflineReward" => HandleDoOfflineReward(envelope, connectionState.Session),
            "train.cs_doInviteMatch" => HandleDoInviteMatch(envelope, connectionState.Session),
            "train.cs_doInviteMatchReward" => HandleDoInviteMatchReward(envelope, connectionState.Session),
            "train.cs_fetchInviteMatchInfo" => HandleFetchInviteMatchInfo(envelope, connectionState.Session),
            "train.cs_syncTrainEvents" => HandleSyncTrainEvents(envelope, connectionState.Session),
            "shop_module.cs_purchaseDiamondSuccess" => HandlePurchaseDiamondSuccess(envelope, connectionState.Session),
            "shop_module.cs_purchaseGiftSuccess" => HandlePurchaseGiftSuccess(envelope, connectionState.Session),
            "shop_module.cs_getMonthCardReward" => HandleGetMonthCardReward(envelope, connectionState.Session),
            "shop_module.cs_purchaseMonthCardSuccess" => HandlePurchaseMonthCardSuccess(envelope, connectionState.Session),
            "shop_module.cs_consumeOrderNo" => HandleConsumeOrderNo(envelope, connectionState.Session),
            "cs_heart" => HandleHeart(envelope),
            _ => HandleUnsupported(envelope, connectionState.Session)
        };
    }

    private IEnumerable<MessageEnvelope> HandleLogin(MessageEnvelope envelope, ConnectionState connectionState)
    {
        var request = LoginRequest.Parser.ParseFrom(envelope.Payload);
        var login = _gameState.Login(request.AccountId, request.Channel, request.Uid);
        connectionState.Session = login.Session;
        var response = new LoginResponse
        {
            Session = login.Session,
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ChannelInfo = new ChannelInfo
            {
                EnableAds = 0,
                EnableQq = 0,
                EnableMail = 0
            },
            RealnameInfo = new RealnameInfo
            {
                Name = string.Empty,
                Id = string.Empty,
                State = 0,
                Year = 0,
                Month = 0,
                Day = 0
            },
            NeedUpdate = false,
            OpenPay = false
        };

        yield return CreateResponse(envelope.SessionId, envelope.MethodName, response);
    }

    private IEnumerable<MessageEnvelope> HandleFetchAllPlayers(MessageEnvelope envelope, ConnectionState connectionState)
    {
        var request = FetchAllPlayersRequest.Parser.ParseFrom(envelope.Payload);
        if (!string.IsNullOrWhiteSpace(request.Session))
        {
            connectionState.Session = request.Session;
        }
        var response = new FetchAllPlayersResponse();
        response.Players.Add(_gameState.GetPlayers(request.Session));
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, response);
    }

    private IEnumerable<MessageEnvelope> HandleCheckName(MessageEnvelope envelope)
    {
        var request = CheckNameRequest.Parser.ParseFrom(envelope.Payload);
        var isInvalid = _gameState.IsPlayerNameRejected(request.Name);

        yield return CreateResponse(envelope.SessionId, envelope.MethodName, new CheckNameResponse
        {
            // Client treats 0 as valid and non-zero as "contains sensitive words".
            Code = isInvalid ? 1 : 0
        });
    }

    private IEnumerable<MessageEnvelope> HandleCreatePlayer(MessageEnvelope envelope, ConnectionState connectionState)
    {
        var request = CreatePlayerRequest.Parser.ParseFrom(envelope.Payload);
        if (!string.IsNullOrWhiteSpace(request.Session))
        {
            connectionState.Session = request.Session;
        }
        var player = _gameState.CreatePlayer(request.Session, request);
        var response = new CreatePlayerResponse
        {
            Player = player
        };

        yield return CreateResponse(envelope.SessionId, envelope.MethodName, response);
    }

    private IEnumerable<MessageEnvelope> HandleEnterGame(MessageEnvelope envelope, ConnectionState connectionState)
    {
        var request = EnterGameRequest.Parser.ParseFrom(envelope.Payload);
        if (!string.IsNullOrWhiteSpace(request.Session))
        {
            connectionState.Session = request.Session;
        }
        var snapshot = _gameState.EnterGame(request.Session, request.Gbid);

        // The client processes several managers inside EnterGameResponse callback,
        // so state-carrying notifies need to arrive before the response.
        yield return CreateNotify("sc_notifySignActivityModule", snapshot.SignActivityInfo);
        yield return CreateNotify("sc_notifyShopModule", snapshot.ShopInfo);
        yield return CreateNotify("sc_updatePlayerInfo", snapshot.PlayerInfo);
        yield return CreateNotify("sc_refreshRecruitInfo", snapshot.RecruitInfo);
        yield return CreateNotify("sc_updatePVPInfo", snapshot.PvpInfo);
        yield return CreateNotify("sc_updateTrainInfo", snapshot.TrainInfo);
        yield return CreateNotify("sc_updateCardInfo", snapshot.CardInfo);
        yield return CreateNotify("sc_refreshPackageInfo", snapshot.PackageInfo);
        yield return CreateNotify("sc_refreshResource", snapshot.ResourceInfo);

        yield return CreateResponse(envelope.SessionId, envelope.MethodName, new EnterGameResponse
        {
            Result = true
        });
    }

    private IEnumerable<MessageEnvelope> HandlePurchaseDiamondSuccess(MessageEnvelope envelope, string connectionSession)
    {
        var request = PurchaseDiamondSuccessRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("PurchaseDiamondSuccess received before connection session was established.");
        }

        var result = _gameState.PurchaseDiamond(connectionSession, request.ShopItemId, request.OrderId);
        yield return CreateNotify("sc_notifyPurchaseDiamondSuccess", result.Notify);
        yield return CreateNotify("sc_notifyShopModule", result.ShopInfo);
        yield return CreateNotify("sc_refreshPackageInfo", result.PackageInfo);
        yield return CreateNotify("sc_refreshResource", result.ResourceInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleSyncTrainEvents(MessageEnvelope envelope, string connectionSession)
    {
        var request = SyncTrainEventsRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("SyncTrainEvents received before connection session was established.");
        }

        var result = _gameState.SyncTrainEvents(connectionSession, request.TrainEvents);
        yield return CreateNotify("sc_updateTrainInfo", result.TrainInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleArenaInfo(MessageEnvelope envelope, string connectionSession)
    {
        ArenaInfoRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("ArenaInfo received before connection session was established.");
        }

        var result = _gameState.GetArenaInfo(connectionSession);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleSaveFormation(MessageEnvelope envelope, string connectionSession)
    {
        var request = SaveFormationRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("SaveFormation received before connection session was established.");
        }

        var result = _gameState.SaveFormation(connectionSession, request.FormationId, request.Formation);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetDefaultFormation(MessageEnvelope envelope, string connectionSession)
    {
        var request = GetDefaultFormationRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetDefaultFormation received before connection session was established.");
        }

        var result = _gameState.GetDefaultFormation(connectionSession, request.FormationId);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleDoOfflineReward(MessageEnvelope envelope, string connectionSession)
    {
        var request = DoOfflineRewardRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("DoOfflineReward received before connection session was established.");
        }

        var result = _gameState.DoOfflineReward(connectionSession, request.VideoBuff);
        yield return CreateNotify("sc_updateTrainInfo", result.TrainInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueData(MessageEnvelope envelope, string connectionSession)
    {
        var request = GetLeagueDataRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueData received before connection session was established.");
        }

        var result = _gameState.GetLeagueData(connectionSession, request.LastLeagueId);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueSignUp(MessageEnvelope envelope, string connectionSession)
    {
        GetLeagueSignUpRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueSignUp received before connection session was established.");
        }

        var result = _gameState.GetLeagueSignUp(connectionSession);
        yield return CreateNotify("sc_updatePVPInfo", result.PvpInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueHistory(MessageEnvelope envelope, string connectionSession)
    {
        GetLeagueHistoryRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueHistory received before connection session was established.");
        }

        var result = _gameState.GetLeagueHistory(connectionSession);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueChampionRank(MessageEnvelope envelope, string connectionSession)
    {
        GetLeagueChampionRankRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueChampionRank received before connection session was established.");
        }

        var result = _gameState.GetLeagueChampionRank(connectionSession);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueCourse(MessageEnvelope envelope, string connectionSession)
    {
        var request = GetLeagueCourseRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueCourse received before connection session was established.");
        }

        var result = _gameState.GetLeagueCourse(connectionSession, request.CompitionId, request.LeagueId, request.Type);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetLeagueCardRank(MessageEnvelope envelope, string connectionSession)
    {
        var request = GetLeagueCardRankRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetLeagueCardRank received before connection session was established.");
        }

        var result = _gameState.GetLeagueCardRank(connectionSession, request.CompitionId, request.LeagueId);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleFetchInviteMatchInfo(MessageEnvelope envelope, string connectionSession)
    {
        FetchInviteMatchInfoRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("FetchInviteMatchInfo received before connection session was established.");
        }

        var result = _gameState.FetchInviteMatchInfo(connectionSession);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleDoInviteMatch(MessageEnvelope envelope, string connectionSession)
    {
        var request = DoInviteMatchRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("DoInviteMatch received before connection session was established.");
        }

        var result = _gameState.DoInviteMatch(connectionSession, request.Id);
        yield return CreateNotify("sc_updateTrainInfo", result.TrainInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleDoInviteMatchReward(MessageEnvelope envelope, string connectionSession)
    {
        var request = DoInviteMatchRewardRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("DoInviteMatchReward received before connection session was established.");
        }

        var result = _gameState.DoInviteMatchReward(connectionSession, request.Id, request.VideoBuff);
        yield return CreateNotify("sc_updateTrainInfo", result.TrainInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleRecruit(MessageEnvelope envelope, string connectionSession)
    {
        var request = RecruitRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("Recruit received before connection session was established.");
        }

        var result = _gameState.Recruit(connectionSession, request.PoolId, request.RecruitCountType, request.CostType);
        yield return CreateNotify("sc_refreshRecruitInfo", result.RecruitInfo);
        yield return CreateNotify("sc_updateCardInfo", result.CardInfo);
        yield return CreateNotify("sc_refreshPackageInfo", result.PackageInfo);
        yield return CreateNotify("sc_refreshResource", result.ResourceInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandlePurchaseMonthCardSuccess(MessageEnvelope envelope, string connectionSession)
    {
        var request = PurchaseMonthCardSuccessRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("PurchaseMonthCardSuccess received before connection session was established.");
        }

        var result = _gameState.PurchaseMonthCard(connectionSession, request.ShopItemId, request.OrderId);
        yield return CreateNotify("sc_notifyPurchaseMonthCardSuccess", result.Notify);
        yield return CreateNotify("sc_notifyShopModule", result.ShopInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandlePurchaseGiftSuccess(MessageEnvelope envelope, string connectionSession)
    {
        var request = PurchaseGiftSuccessRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("PurchaseGiftSuccess received before connection session was established.");
        }

        var result = _gameState.PurchaseGift(connectionSession, request.ShopItemId, request.OrderId);
        yield return CreateNotify("sc_notifyPurchaseGiftSuccess", result.Notify);
        yield return CreateNotify("sc_notifyShopModule", result.ShopInfo);
        yield return CreateNotify("sc_refreshPackageInfo", result.PackageInfo);
        yield return CreateNotify("sc_refreshResource", result.ResourceInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleGetMonthCardReward(MessageEnvelope envelope, string connectionSession)
    {
        var request = GetMonthCardRewardRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("GetMonthCardReward received before connection session was established.");
        }

        var result = _gameState.ReceiveMonthCardReward(connectionSession, request.ShopItemId);
        yield return CreateNotify("sc_notifyShopModule", result.ShopInfo);
        yield return CreateNotify("sc_refreshPackageInfo", result.PackageInfo);
        yield return CreateNotify("sc_refreshResource", result.ResourceInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleReceiveSevenDayReward(MessageEnvelope envelope, string connectionSession)
    {
        ReceiveRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("ReceiveReward received before connection session was established.");
        }

        var result = _gameState.ReceiveSevenDayReward(connectionSession);
        yield return CreateNotify("sc_notifySignActivityModule", result.SignActivityInfo);
        if (result.CardInfo is not null)
        {
            yield return CreateNotify("sc_updateCardInfo", result.CardInfo);
        }

        if (result.PackageInfo is not null)
        {
            yield return CreateNotify("sc_refreshPackageInfo", result.PackageInfo);
        }

        if (result.ResourceInfo is not null)
        {
            yield return CreateNotify("sc_refreshResource", result.ResourceInfo);
        }

        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleConsumeOrderNo(MessageEnvelope envelope, string connectionSession)
    {
        var request = ConsumeOrderNoRequest.Parser.ParseFrom(envelope.Payload);
        if (string.IsNullOrWhiteSpace(connectionSession))
        {
            throw new InvalidOperationException("ConsumeOrderNo received before connection session was established.");
        }

        var result = _gameState.ConsumeOrderNo(connectionSession, request.OrderNo);
        yield return CreateNotify("sc_notifyShopModule", result.ShopInfo);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, result.Response);
    }

    private IEnumerable<MessageEnvelope> HandleHeart(MessageEnvelope envelope)
    {
        HeartRequest.Parser.ParseFrom(envelope.Payload);
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, new HeartResponse
        {
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }

    private IEnumerable<MessageEnvelope> HandleUnsupported(MessageEnvelope envelope, string connectionSession)
    {
        var context = ResolveContext(envelope, connectionSession);
        _logger.LogWarning(
            "UNSUPPORTED method={MethodName} sid={SessionId} bytes={PayloadBytes} account={AccountId} channel={Channel} uid={Uid} session={Session} gbid={Gbid} player={PlayerName}",
            envelope.MethodName,
            envelope.SessionId,
            envelope.Payload.Length,
            context.AccountId ?? "-",
            context.Channel ?? "-",
            context.Uid ?? "-",
            context.Session ?? "-",
            context.Gbid ?? "-",
            context.PlayerName ?? "-");
        yield return CreateResponse(envelope.SessionId, envelope.MethodName, new HeartResponse
        {
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }

    private void LogRequest(string remote, MessageEnvelope envelope, string connectionSession)
    {
        var context = ResolveContext(envelope, connectionSession);
        _logger.LogInformation(
            "REQ remote={Remote} sid={SessionId} method={MethodName} bytes={PayloadBytes} account={AccountId} channel={Channel} uid={Uid} session={Session} gbid={Gbid} player={PlayerName}",
            remote,
            envelope.SessionId,
            envelope.MethodName,
            envelope.Payload.Length,
            context.AccountId ?? "-",
            context.Channel ?? "-",
            context.Uid ?? "-",
            context.Session ?? "-",
            context.Gbid ?? "-",
            context.PlayerName ?? "-");
    }

    private void LogOutgoing(string remote, MessageEnvelope request, MessageEnvelope outgoing, string connectionSession)
    {
        var context = ResolveContext(request, connectionSession);
        var kind = string.IsNullOrEmpty(outgoing.MethodName) ? "RESP" : "NOTIFY";
        var methodName = string.IsNullOrEmpty(outgoing.MethodName) ? request.MethodName : outgoing.MethodName;

        _logger.LogInformation(
            "{Kind} remote={Remote} sid={SessionId} method={MethodName} bytes={PayloadBytes} account={AccountId} channel={Channel} uid={Uid} session={Session} gbid={Gbid} player={PlayerName}",
            kind,
            remote,
            outgoing.SessionId,
            methodName,
            outgoing.Payload.Length,
            context.AccountId ?? "-",
            context.Channel ?? "-",
            context.Uid ?? "-",
            context.Session ?? "-",
            context.Gbid ?? "-",
            context.PlayerName ?? "-");
    }

    private InMemoryGameState.RequestContext ResolveContext(MessageEnvelope envelope, string connectionSession)
    {
        return envelope.MethodName switch
        {
            "cs_login" => ResolveLoginContext(envelope),
            "cs_fetchAllPlayers" => ResolveSessionContext(FetchAllPlayersRequest.Parser.ParseFrom(envelope.Payload).Session),
            "cs_createPlayer" => ResolveSessionContext(CreatePlayerRequest.Parser.ParseFrom(envelope.Payload).Session),
            "cs_enterGame" => ResolveEnterGameContext(envelope),
            "activity_module.cs_receiveReward" => ResolveSessionContext(connectionSession),
            "card_module.cs_recruit" => ResolveSessionContext(connectionSession),
            "shop_module.cs_purchaseDiamondSuccess" => ResolveSessionContext(connectionSession),
            "shop_module.cs_purchaseGiftSuccess" => ResolveSessionContext(connectionSession),
            "shop_module.cs_getMonthCardReward" => ResolveSessionContext(connectionSession),
            "shop_module.cs_purchaseMonthCardSuccess" => ResolveSessionContext(connectionSession),
            "shop_module.cs_consumeOrderNo" => ResolveSessionContext(connectionSession),
            "cs_heart" => ResolveSessionContext(connectionSession),
            _ => string.IsNullOrWhiteSpace(connectionSession)
                ? InMemoryGameState.RequestContext.Empty
                : ResolveSessionContext(connectionSession)
        };
    }

    private InMemoryGameState.RequestContext ResolveLoginContext(MessageEnvelope envelope)
    {
        var request = LoginRequest.Parser.ParseFrom(envelope.Payload);
        return new InMemoryGameState.RequestContext(
            string.IsNullOrWhiteSpace(request.AccountId) ? null : request.AccountId,
            string.IsNullOrWhiteSpace(request.Channel) ? null : request.Channel,
            string.IsNullOrWhiteSpace(request.Uid) ? null : request.Uid,
            null,
            null,
            null);
    }

    private InMemoryGameState.RequestContext ResolveSessionContext(string? session)
    {
        return _gameState.DescribeRequest(session: session);
    }

    private InMemoryGameState.RequestContext ResolveEnterGameContext(MessageEnvelope envelope)
    {
        var request = EnterGameRequest.Parser.ParseFrom(envelope.Payload);
        return _gameState.DescribeRequest(request.Session, request.Gbid);
    }

    private sealed class ConnectionState
    {
        public string Session { get; set; } = string.Empty;
    }

    private static MessageEnvelope CreateResponse(uint sessionId, string methodName, IMessage message)
    {
        _ = methodName;
        return new MessageEnvelope(sessionId, string.Empty, message.ToByteArray());
    }

    private static MessageEnvelope CreateNotify(string methodName, IMessage message)
    {
        return new MessageEnvelope(0, methodName, message.ToByteArray());
    }

    private async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var prefixBuffer = await ReadExactAsync(stream, _options.RequestLengthPrefixBytes, cancellationToken);
        if (prefixBuffer.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var bodyLength = _options.RequestLengthPrefixBytes switch
        {
            2 => BinaryPrimitives.ReadUInt16BigEndian(prefixBuffer),
            4 => BinaryPrimitives.ReadInt32BigEndian(prefixBuffer),
            _ => throw new InvalidOperationException($"Unsupported RequestLengthPrefixBytes={_options.RequestLengthPrefixBytes}.")
        };

        if (bodyLength <= 0)
        {
            throw new InvalidDataException($"Invalid body length {bodyLength}.");
        }

        return await ReadExactAsync(stream, bodyLength, cancellationToken);
    }

    private async Task WriteFrameAsync(NetworkStream stream, byte[] body, CancellationToken cancellationToken)
    {
        var prefix = _options.ResponseLengthPrefixBytes switch
        {
            2 => new byte[]
            {
                (byte)((body.Length >> 8) & 0xFF),
                (byte)(body.Length & 0xFF)
            },
            4 => new byte[]
            {
                (byte)((body.Length >> 24) & 0xFF),
                (byte)((body.Length >> 16) & 0xFF),
                (byte)((body.Length >> 8) & 0xFF),
                (byte)(body.Length & 0xFF)
            },
            _ => throw new InvalidOperationException($"Unsupported ResponseLengthPrefixBytes={_options.ResponseLengthPrefixBytes}.")
        };

        await stream.WriteAsync(prefix.AsMemory(0, _options.ResponseLengthPrefixBytes), cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, length - offset), cancellationToken);
            if (read == 0)
            {
                if (offset == 0)
                {
                    return Array.Empty<byte>();
                }

                throw new EndOfStreamException($"Expected {length} bytes, received {offset}.");
            }

            offset += read;
        }

        return buffer;
    }
}

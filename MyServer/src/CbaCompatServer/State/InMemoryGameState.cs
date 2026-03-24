using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Protocol;

namespace CbaCompatServer.State;

public sealed class InMemoryGameState
{
    private readonly object _gate = new();
    private readonly Dictionary<string, AccountState> _accountsById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AccountState> _accountsBySession = new(StringComparer.Ordinal);
    private readonly string _databasePath;
    private readonly string[] _devAccountPrefixes;
    private readonly int _devPlayerLevel;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly char[] InvalidNameChars = ['<', '>', '"', '\'', '\\', '/', '\r', '\n', '\t'];
    private static readonly Dictionary<int, int> DiamondShopRewards = new()
    {
        [1001] = 120,
        [1002] = 600,
        [1003] = 1360,
        [1004] = 2560,
        [1005] = 6560,
        [1006] = 12960
    };
    private static readonly int[] GiftShopIds = [3001, 3002, 3003, 3004];
    private static readonly int[] MonthCardShopIds = [5001, 5002];
    private static readonly Dictionary<int, GiftReward[]> SevenDayRewards = new()
    {
        [1] = [new GiftReward(1, 1, 300)],
        [2] = [new GiftReward(3, 104014, 1)],
        [3] = [new GiftReward(1, 1, 3000)],
        [4] = [new GiftReward(2, 400201, 10)],
        [5] = [new GiftReward(1, 1, 3000)],
        [6] = [new GiftReward(2, 400201, 10)],
        [7] = [new GiftReward(3, 104002, 1)]
    };
    private static readonly Dictionary<int, GiftReward[]> MonthCardDailyRewards = new()
    {
        [5001] = [new GiftReward(2, 200232, 3), new GiftReward(1, 1, 100)],
        [5002] = [new GiftReward(2, 200231, 3), new GiftReward(1, 1, 300)]
    };
    private static readonly Dictionary<int, GiftReward[]> GiftShopRewards = new()
    {
        [3001] = [new GiftReward(1, 6, 100)],
        [3002] = [new GiftReward(2, 100108, 20)],
        [3003] = [new GiftReward(2, 400154, 20)],
        [3004] = [new GiftReward(2, 400207, 50)]
    };
    private static readonly int[] RecruitCardPool =
    [
        104001, 104002, 104003, 104004, 104005, 104006, 104007, 104008,
        104009, 104010, 104011, 104012, 104013, 104014, 104015, 104016
    ];

    public InMemoryGameState(IOptions<ServerOptions> options)
    {
        var serverOptions = options.Value;
        _databasePath = Path.IsPathRooted(serverOptions.DatabasePath)
            ? serverOptions.DatabasePath
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, serverOptions.DatabasePath));
        _devAccountPrefixes = serverOptions.DevAccountPrefixes ?? [];
        _devPlayerLevel = Math.Max(1, serverOptions.DevPlayerLevel);

        EnsureDatabase();
        LoadSnapshot();
    }

    public LoginContext Login(string? accountId, string? channel, string? uid)
    {
        lock (_gate)
        {
            var normalizedAccountId = string.IsNullOrWhiteSpace(accountId)
                ? $"local-{uid ?? "player"}"
                : accountId;

            if (!_accountsById.TryGetValue(normalizedAccountId, out var account))
            {
                account = new AccountState
                {
                    AccountId = normalizedAccountId,
                    Channel = channel ?? string.Empty,
                    Uid = uid ?? string.Empty
                };
                _accountsById[normalizedAccountId] = account;
            }

            account.Channel = channel ?? account.Channel;
            account.Uid = uid ?? account.Uid;
            if (IsDevAccount(account.AccountId) && account.Players.Count > 0)
            {
                foreach (var player in account.Players)
                {
                    NormalizeDevPlayer(player);
                }
            }
            account.Session = Guid.NewGuid().ToString("N");
            _accountsBySession[account.Session] = account;
            SaveSnapshotLocked();

            return new LoginContext(account.AccountId, account.Session);
        }
    }

    public IReadOnlyList<BasicPlayerInfoNotify> GetPlayers(string session)
    {
        lock (_gate)
        {
            return GetAccount(session).Players
                .Select(ToBasicPlayerInfo)
                .ToList();
        }
    }

    public bool IsPlayerNameRejected(string? name)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            var normalized = name.Trim();
            if (normalized.Length is < 2 or > 12)
            {
                return true;
            }

            if (normalized.IndexOfAny(InvalidNameChars) >= 0)
            {
                return true;
            }

            return _accountsById.Values
                .SelectMany(account => account.Players)
                .Any(player => string.Equals(player.Name, normalized, StringComparison.OrdinalIgnoreCase));
        }
    }

    public BasicPlayerInfoNotify CreatePlayer(string session, CreatePlayerRequest request)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = new PlayerState
            {
                Gbid = $"gbid-{account.Players.Count + 1:D4}",
                Name = string.IsNullOrWhiteSpace(request.Name) ? $"Club-{account.Players.Count + 1}" : request.Name,
                Icon = request.ClubIcon,
                HomeJersey = request.HomeJersey,
                AwayJersey = request.AwayJersey,
                AlternativeJersey = request.AlternativeJersey,
                CreateTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Level = 1,
                Exp = 0,
                Strength = 1280,
                JerseyBreak = 0
            };

            // Mark starter guide complete so the client can enter Home without
            // requiring the full onboarding dialogue / guide battle flow.
            player.GuideIds.Add(100);
            player.Jerseys.Add(player.HomeJersey);
            player.Jerseys.Add(player.AwayJersey);
            player.Jerseys.Add(player.AlternativeJersey);

            foreach (var card in SeedCards())
            {
                player.Cards[card.CardId] = card;
            }

            // GoodsId.ContractFragment in client GameConst.cs
            player.Goods[400502] = 300;
            player.Diamond = 8888;
            player.Money = 999999;
            player.Energy = 120;
            player.EnergyLastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (IsDevAccount(account.AccountId))
            {
                ApplyDevPlayerTemplate(player);
            }

            account.Players.Add(player);
            SaveSnapshotLocked();
            return ToBasicPlayerInfo(player);
        }
    }

    public EnterGameSnapshot EnterGame(string session, string gbid)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault(x => x.Gbid == gbid)
                ?? throw new InvalidOperationException($"Unknown player '{gbid}'.");

            return new EnterGameSnapshot(
                ToSignActivityInfo(player),
                ToShopInfo(player, includePendingOrders: true),
                ToBasicPlayerInfo(player),
                ToRecruitInfo(player),
                ToCardInfo(player),
                ToPackageInfo(player),
                ToResourceInfo(player));
        }
    }

    public PurchaseDiamondResult PurchaseDiamond(string session, int shopItemId, string? orderId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var reward = DiamondShopRewards.TryGetValue(shopItemId, out var amount) ? amount : 0;
            if (reward > 0)
            {
                player.Diamond += reward;
            }

            var normalizedOrderId = string.IsNullOrWhiteSpace(orderId)
                ? Guid.NewGuid().ToString("N")
                : orderId;

            player.BuyCount[shopItemId] = player.BuyCount.TryGetValue(shopItemId, out var buyCount) ? buyCount + 1 : 1;
            player.SumCount[shopItemId] = player.SumCount.TryGetValue(shopItemId, out var sumCount) ? sumCount + 1 : 1;
            player.WeekCount[shopItemId] = player.WeekCount.TryGetValue(shopItemId, out var weekCount) ? weekCount + 1 : 1;
            player.FirstCharge = true;
            player.PendingOrders.Add(new PendingOrderState
            {
                ShopItemId = shopItemId,
                OrderNo = normalizedOrderId
            });
            SaveSnapshotLocked();

            return new PurchaseDiamondResult(
                new PurchaseDiamondSuccessResponse
                {
                    State = 1
                },
                new PurchaseDiamondSuccessNotify
                {
                    ShopItemId = shopItemId,
                    OrderNo = normalizedOrderId
                },
                ToShopInfo(player, includePendingOrders: false),
                ToPackageInfo(player),
                ToResourceInfo(player));
        }
    }

    public PurchaseMonthCardResult PurchaseMonthCard(string session, int shopItemId, string? orderId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var normalizedOrderId = string.IsNullOrWhiteSpace(orderId)
                ? Guid.NewGuid().ToString("N")
                : orderId;

            player.BuyCount[shopItemId] = player.BuyCount.TryGetValue(shopItemId, out var buyCount) ? buyCount + 1 : 1;
            player.SumCount[shopItemId] = player.SumCount.TryGetValue(shopItemId, out var sumCount) ? sumCount + 1 : 1;
            player.WeekCount[shopItemId] = player.WeekCount.TryGetValue(shopItemId, out var weekCount) ? weekCount + 1 : 1;

            switch (shopItemId)
            {
                case 5001:
                    player.MonthCard1Days += 30;
                    player.GetMonthCard1 = false;
                    break;
                case 5002:
                    player.MonthCard2Days += 30;
                    player.GetMonthCard2 = false;
                    break;
            }

            player.PendingOrders.Add(new PendingOrderState
            {
                ShopItemId = shopItemId,
                OrderNo = normalizedOrderId
            });
            SaveSnapshotLocked();

            return new PurchaseMonthCardResult(
                new PurchaseMonthCardSuccessResponse
                {
                    State = 1
                },
                new PurchaseMonthCardSuccessNotify
                {
                    ShopItemId = shopItemId,
                    OrderNo = normalizedOrderId
                },
                ToShopInfo(player, includePendingOrders: false));
        }
    }

    public PurchaseGiftResult PurchaseGift(string session, int shopItemId, string? orderId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var normalizedOrderId = string.IsNullOrWhiteSpace(orderId)
                ? Guid.NewGuid().ToString("N")
                : orderId;

            player.BuyCount[shopItemId] = player.BuyCount.TryGetValue(shopItemId, out var buyCount) ? buyCount + 1 : 1;
            player.SumCount[shopItemId] = player.SumCount.TryGetValue(shopItemId, out var sumCount) ? sumCount + 1 : 1;
            player.WeekCount[shopItemId] = player.WeekCount.TryGetValue(shopItemId, out var weekCount) ? weekCount + 1 : 1;

            if (GiftShopRewards.TryGetValue(shopItemId, out var rewards))
            {
                ApplyGiftRewards(player, rewards);
            }

            player.PendingOrders.Add(new PendingOrderState
            {
                ShopItemId = shopItemId,
                OrderNo = normalizedOrderId
            });
            SaveSnapshotLocked();

            return new PurchaseGiftResult(
                new PurchaseGiftSuccessResponse
                {
                    State = 1
                },
                new PurchaseGiftSuccessNotify
                {
                    ShopItemId = shopItemId,
                    OrderNo = normalizedOrderId
                },
                ToShopInfo(player, includePendingOrders: false),
                ToPackageInfo(player),
                ToResourceInfo(player));
        }
    }

    public ConsumeOrderResult ConsumeOrderNo(string session, IEnumerable<string> orderNos)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var orderSet = new HashSet<string>(orderNos.Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.Ordinal);
            player.PendingOrders.RemoveAll(x => orderSet.Contains(x.OrderNo));
            SaveSnapshotLocked();

            return new ConsumeOrderResult(
                new ConsumeOrderNoResponse
                {
                    Succeed = true
                },
                ToShopInfo(player, includePendingOrders: false));
        }
    }

    public ReceiveRewardResult ReceiveSevenDayReward(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var rewardId = Enumerable.Range(1, Math.Min(player.LoginDay, 7))
                .FirstOrDefault(id => !player.ReceivedSevenDayRewards.Contains(id));

            if (rewardId == 0)
            {
                return new ReceiveRewardResult(
                    new ReceiveResponse
                    {
                        ReceiveSucceed = false
                    },
                    null,
                    null,
                    null,
                    ToSignActivityInfo(player));
            }

            player.ReceivedSevenDayRewards.Add(rewardId);

            if (SevenDayRewards.TryGetValue(rewardId, out var rewards))
            {
                ApplyGiftRewards(player, rewards);
            }
            SaveSnapshotLocked();

            return new ReceiveRewardResult(
                new ReceiveResponse
                {
                    ReceiveSucceed = true,
                    ReceiveList = { rewardId }
                },
                ToPackageInfo(player),
                ToResourceInfo(player),
                ToCardInfo(player),
                ToSignActivityInfo(player));
        }
    }

    public MonthCardRewardResult ReceiveMonthCardReward(string session, int shopItemId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var succeed = shopItemId switch
            {
                5001 when player.MonthCard1Days > 0 && !player.GetMonthCard1 => ClaimMonthCardReward(player, shopItemId, isNormal: true),
                5002 when player.MonthCard2Days > 0 && !player.GetMonthCard2 => ClaimMonthCardReward(player, shopItemId, isNormal: false),
                _ => false
            };

            if (succeed)
            {
                SaveSnapshotLocked();
            }

            return new MonthCardRewardResult(
                new GetMonthCardRewardResponse
                {
                    Succeed = succeed
                },
                ToShopInfo(player, includePendingOrders: false),
                ToPackageInfo(player),
                ToResourceInfo(player));
        }
    }

    public RecruitResult Recruit(string session, int poolId, int recruitCountType, int costType)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var recruitCount = recruitCountType == 2 ? 10 : 1;
            var pool = GetOrCreateRecruitPool(player, poolId);

            if (costType == 1)
            {
                var diamondCost = recruitCount == 10 ? 2700 : 300;
                player.Diamond = Math.Max(0, player.Diamond - diamondCost);
            }
            else
            {
                var goodsId = poolId == 1 ? 400201 : 400210;
                if (player.Goods.TryGetValue(goodsId, out var current))
                {
                    player.Goods[goodsId] = Math.Max(0, current - recruitCount);
                }
            }

            pool.TodayCount += recruitCount;
            pool.TotalRecruitCount += recruitCount;
            pool.ContinueCount += recruitCount;

            var result = new RecruitResponse
            {
                Succeed = true,
                RecruitCountType = recruitCountType,
                CostType = costType,
                PoolInfo = ToRecruitPoolInfo(pool)
            };

            foreach (var cardId in RollRecruitCards(recruitCount))
            {
                EnsurePlayerHasCard(player, cardId);
                result.ResultList.Add(new GameItem
                {
                    Type = 3,
                    Id = cardId,
                    Count = 1
                });
            }

            SaveSnapshotLocked();

            return new RecruitResult(
                result,
                new RefreshRecruitInfoNotify
                {
                    RecruitController = ToRecruitController(player)
                },
                ToCardInfo(player),
                ToPackageInfo(player),
                ToResourceInfo(player));
        }
    }

    public RequestContext DescribeRequest(string? session = null, string? gbid = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(session) && _accountsBySession.TryGetValue(session, out var account))
            {
                var player = string.IsNullOrWhiteSpace(gbid)
                    ? account.Players.FirstOrDefault()
                    : account.Players.FirstOrDefault(x => x.Gbid == gbid);

                return new RequestContext(
                    account.AccountId,
                    account.Channel,
                    account.Uid,
                    account.Session,
                    player?.Gbid,
                    player?.Name);
            }

            if (!string.IsNullOrWhiteSpace(gbid))
            {
                foreach (var accountState in _accountsById.Values)
                {
                    var player = accountState.Players.FirstOrDefault(x => x.Gbid == gbid);
                    if (player is null)
                    {
                        continue;
                    }

                    return new RequestContext(
                        accountState.AccountId,
                        accountState.Channel,
                        accountState.Uid,
                        accountState.Session,
                        player.Gbid,
                        player.Name);
                }
            }

            return RequestContext.Empty;
        }
    }

    private AccountState GetAccount(string session)
    {
        if (!_accountsBySession.TryGetValue(session, out var account))
        {
            throw new InvalidOperationException($"Unknown session '{session}'.");
        }

        return account;
    }

    private void EnsureDatabase()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS server_state (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                snapshot_json TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private void LoadSnapshot()
    {
        lock (_gate)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT snapshot_json FROM server_state WHERE id = 1 LIMIT 1;";
            var snapshotJson = command.ExecuteScalar() as string;
            if (string.IsNullOrWhiteSpace(snapshotJson))
            {
                return;
            }

            var snapshot = JsonSerializer.Deserialize<ServerStateSnapshot>(snapshotJson, _jsonOptions);
            if (snapshot?.Accounts is null)
            {
                return;
            }

            _accountsById.Clear();
            _accountsBySession.Clear();

            foreach (var account in snapshot.Accounts)
            {
                account.Session = string.Empty;
                _accountsById[account.AccountId] = account;
            }
        }
    }

    private void SaveSnapshotLocked()
    {
        var snapshot = new ServerStateSnapshot
        {
            Accounts = _accountsById.Values
                .Select(CloneForPersistence)
                .ToList()
        };
        var snapshotJson = JsonSerializer.Serialize(snapshot, _jsonOptions);

        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO server_state (id, snapshot_json)
            VALUES (1, $snapshotJson)
            ON CONFLICT(id) DO UPDATE SET snapshot_json = excluded.snapshot_json;
            """;
        command.Parameters.AddWithValue("$snapshotJson", snapshotJson);
        command.ExecuteNonQuery();
    }

    private static AccountState CloneForPersistence(AccountState account)
    {
        return new AccountState
        {
            AccountId = account.AccountId,
            Session = string.Empty,
            Channel = account.Channel,
            Uid = account.Uid,
            Players = account.Players.Select(ClonePlayerState).ToList()
        };
    }

    private static PlayerState ClonePlayerState(PlayerState player)
    {
        return new PlayerState
        {
            Gbid = player.Gbid,
            Name = player.Name,
            Icon = player.Icon,
            CreateTime = player.CreateTime,
            Strength = player.Strength,
            HomeJersey = player.HomeJersey,
            AwayJersey = player.AwayJersey,
            AlternativeJersey = player.AlternativeJersey,
            Level = player.Level,
            Exp = player.Exp,
            JerseyBreak = player.JerseyBreak,
            EnergyLastUpdateTime = player.EnergyLastUpdateTime,
            Diamond = player.Diamond,
            Money = player.Money,
            Energy = player.Energy,
            MonthCost = player.MonthCost,
            SumCost = player.SumCost,
            FirstCharge = player.FirstCharge,
            GetMonthCard1 = player.GetMonthCard1,
            GetMonthCard2 = player.GetMonthCard2,
            MonthCard1Days = player.MonthCard1Days,
            MonthCard2Days = player.MonthCard2Days,
            BuyEnergyCount = player.BuyEnergyCount,
            LoginDay = player.LoginDay,
            GuideIds = player.GuideIds.ToList(),
            Jerseys = player.Jerseys.ToList(),
            ReceivedSevenDayRewards = player.ReceivedSevenDayRewards.ToHashSet(),
            Cards = player.Cards.ToDictionary(x => x.Key, x => CloneCardState(x.Value)),
            Goods = new Dictionary<int, int>(player.Goods),
            RecruitPools = player.RecruitPools.ToDictionary(x => x.Key, x => CloneRecruitPoolState(x.Value)),
            BuyCount = new Dictionary<int, int>(player.BuyCount),
            SumCount = new Dictionary<int, int>(player.SumCount),
            WeekCount = new Dictionary<int, int>(player.WeekCount),
            PendingOrders = player.PendingOrders.Select(ClonePendingOrderState).ToList()
        };
    }

    private static CardState CloneCardState(CardState card)
    {
        return new CardState
        {
            CardId = card.CardId,
            PlayerCardNumber = card.PlayerCardNumber,
            Quality = card.Quality,
            Star = card.Star,
            Level = card.Level,
            Exp = card.Exp,
            Strength = card.Strength,
            Status = card.Status
        };
    }

    private static PendingOrderState ClonePendingOrderState(PendingOrderState order)
    {
        return new PendingOrderState
        {
            ShopItemId = order.ShopItemId,
            OrderNo = order.OrderNo
        };
    }

    private static RecruitPoolState CloneRecruitPoolState(RecruitPoolState pool)
    {
        return new RecruitPoolState
        {
            PoolId = pool.PoolId,
            ContinueCount = pool.ContinueCount,
            CanRecruit = pool.CanRecruit,
            TodayCount = pool.TodayCount,
            TotalRecruitCount = pool.TotalRecruitCount,
            Rewards = pool.Rewards.ToList()
        };
    }

    private bool IsDevAccount(string accountId)
    {
        return _devAccountPrefixes.Any(prefix =>
            !string.IsNullOrWhiteSpace(prefix) &&
            accountId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private void ApplyDevPlayerTemplate(PlayerState player)
    {
        player.Level = _devPlayerLevel;
        player.Exp = 99999;
        player.Strength = 18888;
        player.LoginDay = 1;
        player.Diamond = 99999;
        player.Money = 9_999_999;
        player.Energy = 200;
        player.EnergyLastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        player.MonthCard1Days = 30;
        player.MonthCard2Days = 30;
        player.GetMonthCard1 = false;
        player.GetMonthCard2 = false;

        player.Goods[400201] = 50;
        player.Goods[400210] = 50;
        player.Goods[400202] = 20;
        player.Goods[400208] = 20;
        player.Goods[400206] = 20;
        player.Goods[400207] = 20;
        player.Goods[400502] = 1000;

        player.ReceivedSevenDayRewards.Clear();

        foreach (var extraCard in SeedDevCards())
        {
            player.Cards[extraCard.CardId] = extraCard;
        }
    }

    private void NormalizeDevPlayer(PlayerState player)
    {
        player.LoginDay = Math.Max(1, Math.Min(player.LoginDay, player.ReceivedSevenDayRewards.Count + 1));
    }

    private static IEnumerable<CardState> SeedCards()
    {
        yield return new CardState { CardId = 104001, PlayerCardNumber = 1, Quality = 1, Star = 1, Level = 1, Exp = 0, Strength = 80, Status = 1 };
        yield return new CardState { CardId = 104002, PlayerCardNumber = 2, Quality = 1, Star = 1, Level = 1, Exp = 0, Strength = 76, Status = 1 };
        yield return new CardState { CardId = 104003, PlayerCardNumber = 3, Quality = 2, Star = 2, Level = 5, Exp = 0, Strength = 120, Status = 1 };
    }

    private static IEnumerable<CardState> SeedDevCards()
    {
        yield return new CardState { CardId = 104014, PlayerCardNumber = 101, Quality = 3, Star = 3, Level = 20, Exp = 0, Strength = 420, Status = 1 };
        yield return new CardState { CardId = 104036, PlayerCardNumber = 102, Quality = 4, Star = 4, Level = 30, Exp = 0, Strength = 680, Status = 1 };
        yield return new CardState { CardId = 104038, PlayerCardNumber = 103, Quality = 4, Star = 4, Level = 30, Exp = 0, Strength = 700, Status = 1 };
    }

    private static SignActivityModuleNotify ToSignActivityInfo(PlayerState player)
    {
        var notify = new SignActivityModuleNotify
        {
            LoginDay = player.LoginDay,
            SignDay = player.LoginDay,
            OpenServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AllStarStrength = player.Strength,
            AllStarMaxStrength = player.Strength,
            AllStarGroup = 0
        };

        foreach (var rewardId in Enumerable.Range(1, 7))
        {
            notify.SevenLoginList[rewardId] = player.ReceivedSevenDayRewards.Contains(rewardId) ? 3 : 0;
        }

        return notify;
    }

    private static ShopModuleNotify ToShopInfo(PlayerState player, bool includePendingOrders)
    {
        var notify = new ShopModuleNotify
        {
            MonthCost = player.MonthCost,
            SumCost = player.SumCost,
            FirstCharge = player.FirstCharge,
            GetMonthCard1 = player.GetMonthCard1,
            GetMonthCard2 = player.GetMonthCard2,
            MonthCard1Days = player.MonthCard1Days,
            MonthCard2Days = player.MonthCard2Days,
            GetEnergyTimes = player.BuyEnergyCount
        };

        foreach (var itemId in DiamondShopRewards.Keys.Concat(GiftShopIds).Concat(MonthCardShopIds))
        {
            notify.BuyCount.Add(new BuyCountData
            {
                ShopItemId = itemId,
                Count = player.BuyCount.TryGetValue(itemId, out var buyCount) ? buyCount : 0
            });
            notify.SumCount.Add(new BuyCountData
            {
                ShopItemId = itemId,
                Count = player.SumCount.TryGetValue(itemId, out var sumCount) ? sumCount : 0
            });
            notify.WeekCount.Add(new BuyCountData
            {
                ShopItemId = itemId,
                Count = player.WeekCount.TryGetValue(itemId, out var weekCount) ? weekCount : 0
            });
        }

        if (includePendingOrders)
        {
            foreach (var order in player.PendingOrders)
            {
                notify.OrderData.Add(new ConsumeOrderData
                {
                    ShopItemId = order.ShopItemId,
                    OrderNo = order.OrderNo
                });
            }
        }

        return notify;
    }

    private static BasicPlayerInfoNotify ToBasicPlayerInfo(PlayerState player)
    {
        var message = new BasicPlayerInfoNotify
        {
            Gbid = player.Gbid,
            Name = player.Name,
            Icon = player.Icon,
            CreateTime = player.CreateTime,
            Strength = player.Strength,
            HomeJersey = player.HomeJersey,
            AwayJersey = player.AwayJersey,
            AlternativeJersey = player.AlternativeJersey,
            Level = player.Level,
            Exp = player.Exp,
            JerseyBreak = player.JerseyBreak,
            Developer = 1
        };

        message.GuideId.Add(player.GuideIds);
        message.Jerseys.Add(player.Jerseys);
        return message;
    }

    private static ModuleCardInfoNotify ToCardInfo(PlayerState player)
    {
        var notify = new ModuleCardInfoNotify();
        foreach (var card in player.Cards.Values)
        {
            notify.PlayerCardMap[card.CardId] = new PlayerCardInfo
            {
                CardId = card.CardId,
                PlayerCardNumber = card.PlayerCardNumber,
                Quality = card.Quality,
                Star = card.Star,
                Level = card.Level,
                Exp = card.Exp,
                Strength = card.Strength,
                Status = card.Status,
                PerformanceData = new PerformanceData
                {
                    Court = 1,
                    Point = 0,
                    Rebound = 0,
                    Assist = 0,
                    Steal = 0,
                    Block = 0
                },
                Energy = 100,
                EnergyLastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            notify.PlayerCardMap[card.CardId].Jerseys.Add(0);
            notify.PlayerCardMap[card.CardId].Jerseys.Add(0);
            notify.PlayerCardMap[card.CardId].Jerseys.Add(0);
            notify.PlayerCardMap[card.CardId].Jerseys.Add(0);
        }

        return notify;
    }

    private static RefreshRecruitInfoNotify ToRecruitInfo(PlayerState player)
    {
        return new RefreshRecruitInfoNotify
        {
            RecruitController = ToRecruitController(player)
        };
    }

    private static RecruitControllerInfo ToRecruitController(PlayerState player)
    {
        var controller = new RecruitControllerInfo
        {
            TotalRecruitCount = player.RecruitPools.Values.Sum(x => x.TotalRecruitCount)
        };

        foreach (var pool in player.RecruitPools.Values.OrderBy(x => x.PoolId))
        {
            controller.RecruitPoolList.Add(ToRecruitPoolInfo(pool));
        }

        if (controller.RecruitPoolList.Count == 0)
        {
            controller.RecruitPoolList.Add(ToRecruitPoolInfo(GetOrCreateRecruitPool(player, 1)));
        }

        return controller;
    }

    private static RecruitPoolInfo ToRecruitPoolInfo(RecruitPoolState pool)
    {
        var info = new RecruitPoolInfo
        {
            PoolId = pool.PoolId,
            ContinueCount = pool.ContinueCount,
            CanRecruit = pool.CanRecruit,
            TodayCount = pool.TodayCount,
            TotalRecruitCount = pool.TotalRecruitCount
        };
        info.Rewards.Add(pool.Rewards);
        return info;
    }

    private static RefreshPackageInfoNotify ToPackageInfo(PlayerState player)
    {
        var packageInfo = new ModulePackageInfo
        {
            Diamond = player.Diamond,
            Money = player.Money,
            Energy = player.Energy,
            EnergyLastUpdateTime = player.EnergyLastUpdateTime
        };

        foreach (var goods in player.Goods)
        {
            packageInfo.GoodsMap[goods.Key] = new Goods
            {
                Id = goods.Key,
                Count = goods.Value,
                IsNew = false
            };
        }

        return new RefreshPackageInfoNotify
        {
            PackageInfo = packageInfo
        };
    }

    private static RefreshResourceNotify ToResourceInfo(PlayerState player)
    {
        return new RefreshResourceNotify
        {
            Diamond = player.Diamond,
            Money = player.Money,
            Energy = player.Energy,
            EnergyLastUpdateTime = player.EnergyLastUpdateTime
        };
    }

    private static void ApplyGiftRewards(PlayerState player, IEnumerable<GiftReward> rewards)
    {
        foreach (var reward in rewards)
        {
            if (reward.Type == 1)
            {
                switch (reward.Id)
                {
                    case 1:
                        player.Money += reward.Count;
                        break;
                    case 2:
                        player.Diamond += reward.Count;
                        break;
                    case 6:
                        player.Energy += reward.Count;
                        player.EnergyLastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        break;
                }

                continue;
            }

            if (reward.Type == 2)
            {
                player.Goods[reward.Id] = player.Goods.TryGetValue(reward.Id, out var count)
                    ? count + reward.Count
                    : reward.Count;
                continue;
            }

            if (reward.Type == 3)
            {
                if (!player.Cards.ContainsKey(reward.Id))
                {
                    var nextCardNumber = player.Cards.Count == 0 ? 1 : player.Cards.Values.Max(x => x.PlayerCardNumber) + 1;
                    player.Cards[reward.Id] = new CardState
                    {
                        CardId = reward.Id,
                        PlayerCardNumber = nextCardNumber,
                        Quality = 1,
                        Star = 1,
                        Level = 1,
                        Exp = 0,
                        Strength = 80,
                        Status = 1
                    };
                }
            }
        }
    }

    private static bool ClaimMonthCardReward(PlayerState player, int shopItemId, bool isNormal)
    {
        if (!MonthCardDailyRewards.TryGetValue(shopItemId, out var rewards))
        {
            return false;
        }

        ApplyGiftRewards(player, rewards);
        if (isNormal)
        {
            player.GetMonthCard1 = true;
        }
        else
        {
            player.GetMonthCard2 = true;
        }

        return true;
    }

    private static RecruitPoolState GetOrCreateRecruitPool(PlayerState player, int poolId)
    {
        if (!player.RecruitPools.TryGetValue(poolId, out var pool))
        {
            pool = new RecruitPoolState
            {
                PoolId = poolId,
                CanRecruit = true
            };
            player.RecruitPools[poolId] = pool;
        }

        return pool;
    }

    private static IEnumerable<int> RollRecruitCards(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return RecruitCardPool[Random.Shared.Next(RecruitCardPool.Length)];
        }
    }

    private static void EnsurePlayerHasCard(PlayerState player, int cardId)
    {
        if (player.Cards.ContainsKey(cardId))
        {
            return;
        }

        var nextCardNumber = player.Cards.Count == 0 ? 1 : player.Cards.Values.Max(x => x.PlayerCardNumber) + 1;
        player.Cards[cardId] = new CardState
        {
            CardId = cardId,
            PlayerCardNumber = nextCardNumber,
            Quality = 1,
            Star = 1,
            Level = 1,
            Exp = 0,
            Strength = 80,
            Status = 1
        };
    }

    public sealed record LoginContext(string AccountId, string Session);

    public sealed record RequestContext(
        string? AccountId,
        string? Channel,
        string? Uid,
        string? Session,
        string? Gbid,
        string? PlayerName)
    {
        public static RequestContext Empty { get; } = new(null, null, null, null, null, null);
    }

    public sealed record EnterGameSnapshot(
        SignActivityModuleNotify SignActivityInfo,
        ShopModuleNotify ShopInfo,
        BasicPlayerInfoNotify PlayerInfo,
        RefreshRecruitInfoNotify RecruitInfo,
        ModuleCardInfoNotify CardInfo,
        RefreshPackageInfoNotify PackageInfo,
        RefreshResourceNotify ResourceInfo);

    public sealed record PurchaseDiamondResult(
        PurchaseDiamondSuccessResponse Response,
        PurchaseDiamondSuccessNotify Notify,
        ShopModuleNotify ShopInfo,
        RefreshPackageInfoNotify PackageInfo,
        RefreshResourceNotify ResourceInfo);

    public sealed record PurchaseMonthCardResult(
        PurchaseMonthCardSuccessResponse Response,
        PurchaseMonthCardSuccessNotify Notify,
        ShopModuleNotify ShopInfo);

    public sealed record PurchaseGiftResult(
        PurchaseGiftSuccessResponse Response,
        PurchaseGiftSuccessNotify Notify,
        ShopModuleNotify ShopInfo,
        RefreshPackageInfoNotify PackageInfo,
        RefreshResourceNotify ResourceInfo);

    public sealed record ConsumeOrderResult(
        ConsumeOrderNoResponse Response,
        ShopModuleNotify ShopInfo);

    public sealed record ReceiveRewardResult(
        ReceiveResponse Response,
        RefreshPackageInfoNotify? PackageInfo,
        RefreshResourceNotify? ResourceInfo,
        ModuleCardInfoNotify? CardInfo,
        SignActivityModuleNotify SignActivityInfo);

    public sealed record MonthCardRewardResult(
        GetMonthCardRewardResponse Response,
        ShopModuleNotify ShopInfo,
        RefreshPackageInfoNotify PackageInfo,
        RefreshResourceNotify ResourceInfo);

    public sealed record RecruitResult(
        RecruitResponse Response,
        RefreshRecruitInfoNotify RecruitInfo,
        ModuleCardInfoNotify CardInfo,
        RefreshPackageInfoNotify PackageInfo,
        RefreshResourceNotify ResourceInfo);

    private sealed class AccountState
    {
        public string AccountId { get; set; } = string.Empty;
        public string Session { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public List<PlayerState> Players { get; set; } = [];
    }

    private sealed class PlayerState
    {
        public string Gbid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Icon { get; set; }
        public int CreateTime { get; set; }
        public int Strength { get; set; }
        public int HomeJersey { get; set; }
        public int AwayJersey { get; set; }
        public int AlternativeJersey { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int JerseyBreak { get; set; }
        public long EnergyLastUpdateTime { get; set; }
        public int Diamond { get; set; }
        public int Money { get; set; }
        public int Energy { get; set; }
        public float MonthCost { get; set; }
        public float SumCost { get; set; }
        public bool FirstCharge { get; set; }
        public bool GetMonthCard1 { get; set; }
        public bool GetMonthCard2 { get; set; }
        public int MonthCard1Days { get; set; }
        public int MonthCard2Days { get; set; }
        public int BuyEnergyCount { get; set; }
        public int LoginDay { get; set; } = 1;
        public List<int> GuideIds { get; set; } = [];
        public List<int> Jerseys { get; set; } = [];
        public HashSet<int> ReceivedSevenDayRewards { get; set; } = [];
        public Dictionary<int, CardState> Cards { get; set; } = [];
        public Dictionary<int, int> Goods { get; set; } = [];
        public Dictionary<int, RecruitPoolState> RecruitPools { get; set; } = [];
        public Dictionary<int, int> BuyCount { get; set; } = [];
        public Dictionary<int, int> SumCount { get; set; } = [];
        public Dictionary<int, int> WeekCount { get; set; } = [];
        public List<PendingOrderState> PendingOrders { get; set; } = [];
    }

    private sealed class CardState
    {
        public int CardId { get; init; }
        public int PlayerCardNumber { get; init; }
        public int Quality { get; init; }
        public int Star { get; init; }
        public int Level { get; init; }
        public int Exp { get; init; }
        public int Strength { get; init; }
        public int Status { get; init; }
    }

    private sealed class PendingOrderState
    {
        public int ShopItemId { get; init; }
        public string OrderNo { get; init; } = string.Empty;
    }

    private sealed class RecruitPoolState
    {
        public int PoolId { get; set; }
        public int ContinueCount { get; set; }
        public bool CanRecruit { get; set; } = true;
        public int TodayCount { get; set; }
        public int TotalRecruitCount { get; set; }
        public List<int> Rewards { get; set; } = [];
    }

    private sealed record GiftReward(int Type, int Id, int Count);

    private sealed class ServerStateSnapshot
    {
        public List<AccountState> Accounts { get; set; } = [];
    }
}

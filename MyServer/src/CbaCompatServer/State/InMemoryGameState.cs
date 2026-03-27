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
    private static readonly int[] TrainElementIds = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    private static readonly int[] DefaultStarterBoardIds = [101, 102, 103, 201, 202];

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
            else
            {
                EnsureTrainState(player);
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
            EnsureTrainState(player);
            AccrueTrainIncome(player, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            SaveSnapshotLocked();

            return new EnterGameSnapshot(
                ToSignActivityInfo(player),
                ToShopInfo(player, includePendingOrders: true),
                ToBasicPlayerInfo(player),
                ToRecruitInfo(player),
                ToUpdatePvpInfo(player),
                ToTrainInfo(player),
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

    public SyncTrainEventsResult SyncTrainEvents(string session, IEnumerable<TrainEvent> events)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            EnsureTrainState(player);

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            AccrueTrainIncome(player, nowMs);

            var accepted = false;
            foreach (var trainEvent in events.OrderBy(x => x.Time <= 0 ? long.MaxValue : x.Time))
            {
                accepted |= TryApplyTrainEvent(player, trainEvent, nowMs);
            }

            if (accepted)
            {
                SaveSnapshotLocked();
            }

            return new SyncTrainEventsResult(
                new SyncTrainEventsResponse
                {
                    Result = true
                },
                ToTrainInfo(player));
        }
    }

    public DoOfflineRewardResult DoOfflineReward(string session, int videoBuff)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            EnsureTrainState(player);
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            AccrueTrainIncome(player, nowMs);

            var train = player.Train!;
            var rewardValue = Math.Max(0d, train.OfflineExpValue);
            if (videoBuff != 0)
            {
                rewardValue *= 2d;
            }

            if (rewardValue > 0)
            {
                train.ExpValue += rewardValue;
                train.TotalExpValue += rewardValue;
            }

            train.OfflineExpValue = 0;
            train.OfflineExpBeginTime = 0;
            SaveSnapshotLocked();

            return new DoOfflineRewardResult(
                new DoOfflineRewardResponse
                {
                    RewardExp = ToBigNumberInfo(rewardValue, 0)
                },
                ToTrainInfo(player));
        }
    }

    public FetchInviteMatchInfoResult FetchInviteMatchInfo(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var train = EnsureTrainState(player);
            EnsureInviteMatchState(train);
            SaveSnapshotLocked();

            return new FetchInviteMatchInfoResult(
                new FetchInviteMatchInfoResponse
                {
                    Info = ToInviteMatchControllerInfo(train)
                });
        }
    }

    public GetArenaInfoResult GetArenaInfo(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var response = new ArenaInfoResponse
            {
                Succeed = true,
                Info = BuildArenaInfo(player),
                JoinSeason = false
            };

            foreach (var top in BuildArenaTopRanks(player))
            {
                response.Tops.Add(top);
            }

            foreach (var opponent in BuildArenaOpponents(player))
            {
                response.Opponents.Add(opponent);
            }

            return new GetArenaInfoResult(response);
        }
    }

    public SaveFormationResult SaveFormation(string session, int formationId, FormationInfo? formation)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var normalized = NormalizeFormation(formationId, formation, player);
            player.Formations[formationId] = normalized;
            SaveSnapshotLocked();

            return new SaveFormationResult(new SaveFormationResponse
            {
                Success = true
            });
        }
    }

    public GetDefaultFormationResult GetDefaultFormation(string session, int formationId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var formation = EnsureFormationState(player, formationId);
            return new GetDefaultFormationResult(new GetDefaultFormationResponse
            {
                Formation = ToFormationInfo(formation)
            });
        }
    }

    public GetLeagueDataResult GetLeagueData(string session, int lastLeagueId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var leagueId = lastLeagueId > 0 ? lastLeagueId : 1;
            var leagueState = GetDisplayedLeagueTeamState(player);

            var response = new GetLeagueDataResponse
            {
                LeagueInfo = ToPlayerLeagueInfo(player, leagueId),
                TeamState = leagueState
            };

            if (leagueState == 3)
            {
                var preview = BuildLeagueGamePreview(player, leagueId);
                response.GamePerviewData = preview;
                response.LeagueScorebarTeamList.Add(BuildLeagueScorebar(player, preview.HomeTeam, session: 1, obtain: 102, lost: 95));
                response.LeagueScorebarTeamList.Add(BuildLeagueScorebar(player, preview.AwayTeam, session: 1, obtain: 95, lost: 102));
            }

            return new GetLeagueDataResult(response);
        }
    }

    public GetLeagueSignUpResult GetLeagueSignUp(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var currentState = GetLeagueTeamState(player);
            var canSign = currentState is 1 or 4;
            if (canSign)
            {
                player.LeagueTeamState = 2;
                SaveSnapshotLocked();
            }

            return new GetLeagueSignUpResult(
                new GetLeagueSignUpResponse
                {
                    Success = canSign
                },
                ToUpdatePvpInfo(player));
        }
    }

    public GetLeagueHistoryResult GetLeagueHistory(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var response = new GetLeagueHistoryResponse();
            response.LeagueHistoryDataList.Add(new LeagueHistoryData
            {
                StartTime = DateTimeOffset.UtcNow.AddDays(-14).ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds(),
                Rank = 2,
                LeagueLevel = 1,
                Win = 5,
                Failed = 2,
                Point = 128,
                Rebound = 54,
                Assist = 33,
                Steal = 11,
                Block = 7
            });

            return new GetLeagueHistoryResult(response);
        }
    }

    public GetLeagueChampionRankResult GetLeagueChampionRank(string session)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var response = new GetLeagueChampionRankResponse();
            response.RankList.Add(new ChampionTeamData
            {
                Rank = 1,
                Team = ToPlayerTeamData(player.Gbid, player.Name, player.Icon),
                Champion = 3,
                Time = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds()
            });
            response.RankList.Add(new ChampionTeamData
            {
                Rank = 2,
                Team = ToPlayerTeamData("league-ai-1", "先锋队", 2),
                Champion = 2,
                Time = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds()
            });
            response.RankList.Add(new ChampionTeamData
            {
                Rank = 3,
                Team = ToPlayerTeamData("league-ai-2", "雷霆会", 3),
                Champion = 1,
                Time = DateTimeOffset.UtcNow.AddDays(-60).ToUnixTimeSeconds()
            });

            return new GetLeagueChampionRankResult(response);
        }
    }

    public GetLeagueCourseResult GetLeagueCourse(string session, int compitionId, int leagueId, int type)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var preview = BuildLeagueGamePreview(player, leagueId);
            var response = new GetLeagueCourseResponse
            {
                LeagueLevel = 1
            };

            foreach (var item in BuildLeagueCourseItems(player, preview, type))
            {
                response.LeagueCourseItemList.Add(item);
            }

            return new GetLeagueCourseResult(response);
        }
    }

    public GetLeagueCardRankResult GetLeagueCardRank(string session, int compitionId, int leagueId)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var response = new GetLeagueCardRankResponse();
            foreach (var rank in BuildLeagueCardRanks(player))
            {
                response.GoalsScoredRank.Add(rank.Clone());
                response.AssistsRank.Add(rank.Clone());
                response.StealRank.Add(rank.Clone());
                response.BlockRank.Add(rank.Clone());
                response.ReboundRank.Add(rank.Clone());
            }

            return new GetLeagueCardRankResult(response);
        }
    }

    public DoInviteMatchResult DoInviteMatch(string session, int id)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var train = EnsureTrainState(player);
            EnsureInviteMatchState(train);

            var match = train.InviteMatches.FirstOrDefault(x => x.Id == id)
                ?? throw new InvalidOperationException($"Invite match '{id}' was not found.");

            if (!train.InviteMatchUnlocked)
            {
                throw new InvalidOperationException("Invite match is not unlocked.");
            }

            if (match.State != 1)
            {
                return new DoInviteMatchResult(
                    new DoInviteMatchResponse
                    {
                        MatchInfo = ToInviteMatchInfo(match)
                    },
                    ToTrainInfo(player));
            }

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            match.MineScore = Random.Shared.Next(82, 121);
            match.OpponentScore = Random.Shared.Next(70, Math.Max(match.MineScore, 71));
            if (match.OpponentScore >= match.MineScore)
            {
                match.OpponentScore = match.MineScore - 1;
            }

            match.State = 2;
            match.CdEndTime = nowMs;
            SaveSnapshotLocked();

            return new DoInviteMatchResult(
                new DoInviteMatchResponse
                {
                    MatchInfo = ToInviteMatchInfo(match)
                },
                ToTrainInfo(player));
        }
    }

    public DoInviteMatchRewardResult DoInviteMatchReward(string session, int id, int videoBuff)
    {
        lock (_gate)
        {
            var account = GetAccount(session);
            var player = account.Players.FirstOrDefault()
                ?? throw new InvalidOperationException($"Account '{account.AccountId}' has no player.");

            var train = EnsureTrainState(player);
            EnsureInviteMatchState(train);
            AccrueTrainIncome(player, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            var match = train.InviteMatches.FirstOrDefault(x => x.Id == id)
                ?? throw new InvalidOperationException($"Invite match '{id}' was not found.");

            if (match.State == 2)
            {
                var rewardValue = match.BaseRewardValue;
                if (videoBuff != 0)
                {
                    rewardValue *= 10d;
                }

                train.ExpValue += rewardValue;
                train.TotalExpValue += rewardValue;
                match.State = 3;
                match.CdEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            SaveSnapshotLocked();

            return new DoInviteMatchRewardResult(
                new DoInviteMatchRewardResponse
                {
                    Result = true
                },
                ToTrainInfo(player));
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
            PendingOrders = player.PendingOrders.Select(ClonePendingOrderState).ToList(),
            LeagueTeamState = player.LeagueTeamState,
            Formations = player.Formations.ToDictionary(x => x.Key, x => CloneFormationState(x.Value)),
            Train = CloneTrainState(player.Train)
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

    private static FormationState CloneFormationState(FormationState formation)
    {
        return new FormationState
        {
            FormationId = formation.FormationId,
            BaseFormationName = formation.BaseFormationName,
            FormationName = formation.FormationName,
            StarterBoardCardMap = new Dictionary<int, int>(formation.StarterBoardCardMap),
            SubstituteBoardCardMap = new Dictionary<int, int>(formation.SubstituteBoardCardMap),
            TacticsIdList = formation.TacticsIdList.ToList(),
            IsInitialized = formation.IsInitialized,
            LineupShowTime = formation.LineupShowTime,
            TacticsLevels = new Dictionary<int, int>(formation.TacticsLevels)
        };
    }

    private static TrainState? CloneTrainState(TrainState? train)
    {
        if (train is null)
        {
            return null;
        }

        return new TrainState
        {
            ExpValue = train.ExpValue,
            ExpUnitId = train.ExpUnitId,
            TotalExpValue = train.TotalExpValue,
            TotalExpUnitId = train.TotalExpUnitId,
            ForceValue = train.ForceValue,
            ForceUnitId = train.ForceUnitId,
            ForceAdd = train.ForceAdd,
            UpLevelType = train.UpLevelType,
            OfflineExpBeginTime = train.OfflineExpBeginTime,
            OfflineExpValue = train.OfflineExpValue,
            OfflineExpUnitId = train.OfflineExpUnitId,
            LastAcceptedEventTime = train.LastAcceptedEventTime,
            StrengthenTrainedList = train.StrengthenTrainedList.ToList(),
            BigBangTimes = train.BigBangTimes,
            ClearCdTimes = train.ClearCdTimes,
            LastCdTime = train.LastCdTime,
            InviteMatchUnlocked = train.InviteMatchUnlocked,
            InviteMatches = train.InviteMatches.Select(CloneInviteMatchState).ToList(),
            Elements = train.Elements.ToDictionary(x => x.Key, x => CloneTrainElementState(x.Value))
        };
    }

    private static TrainElementState CloneTrainElementState(TrainElementState element)
    {
        return new TrainElementState
        {
            Id = element.Id,
            Level = element.Level,
            RewardLevel = element.RewardLevel,
            IncomeAddValue = element.IncomeAddValue,
            IncomeAddUnitId = element.IncomeAddUnitId,
            TimeReduceValue = element.TimeReduceValue,
            TimeReduceUnitId = element.TimeReduceUnitId,
            ConsumeReduceValue = element.ConsumeReduceValue,
            ConsumeReduceUnitId = element.ConsumeReduceUnitId,
            BreakIndex = element.BreakIndex,
            LastIncomeTime = element.LastIncomeTime
        };
    }

    private static InviteMatchState CloneInviteMatchState(InviteMatchState match)
    {
        return new InviteMatchState
        {
            Id = match.Id,
            MineScore = match.MineScore,
            OpponentScore = match.OpponentScore,
            State = match.State,
            CdEndTime = match.CdEndTime,
            BaseRewardValue = match.BaseRewardValue,
            BaseRewardUnitId = match.BaseRewardUnitId,
            OpponentName = match.OpponentName,
            OpponentIcon = match.OpponentIcon,
            Organizer = match.Organizer,
            Place = match.Place,
            Content = match.Content,
            OrganizerIcon = match.OrganizerIcon
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

        EnsureTrainState(player);
    }

    private void NormalizeDevPlayer(PlayerState player)
    {
        player.LoginDay = Math.Max(1, Math.Min(player.LoginDay, player.ReceivedSevenDayRewards.Count + 1));
        EnsureTrainState(player);
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

    private static TrainInfoNotify ToTrainInfo(PlayerState player)
    {
        var train = EnsureTrainState(player);
        EnsureInviteMatchState(train);
        var notify = new TrainInfoNotify
        {
            Exp = ToBigNumberInfo(train.ExpValue, train.ExpUnitId),
            TotalExp = ToBigNumberInfo(train.TotalExpValue, train.TotalExpUnitId),
            Force = ToBigNumberInfo(train.ForceValue, train.ForceUnitId),
            ForceAdd = train.ForceAdd,
            UpLevelType = train.UpLevelType,
            OfflineExpBeginTime = train.OfflineExpBeginTime,
            OfflineExp = ToBigNumberInfo(train.OfflineExpValue, train.OfflineExpUnitId),
            Strengthen = new StrengthenControllerInfo(),
            BigBang = new BigBangControllerInfo
            {
                BigBangTimes = train.BigBangTimes,
                ClearCdTimes = train.ClearCdTimes,
                LastCdTime = train.LastCdTime
            },
            InviteMatch = ToInviteMatchControllerInfo(train)
        };

        notify.Strengthen.TrainedList.Add(train.StrengthenTrainedList);

        foreach (var elementId in TrainElementIds)
        {
            var element = train.Elements[elementId];
            notify.TrainElements.Add(new TrainElementInfo
            {
                Id = element.Id,
                Level = element.Level,
                RewardLevel = element.RewardLevel,
                IncomeAdd = ToBigNumberInfo(element.IncomeAddValue, element.IncomeAddUnitId),
                TimeReduce = ToBigNumberInfo(element.TimeReduceValue, element.TimeReduceUnitId),
                ConsumeReduce = ToBigNumberInfo(element.ConsumeReduceValue, element.ConsumeReduceUnitId),
                BreakIndex = element.BreakIndex,
                LastIncomeTime = element.LastIncomeTime
            });
        }

        return notify;
    }

    private static InviteMatchControllerInfo ToInviteMatchControllerInfo(TrainState train)
    {
        EnsureInviteMatchState(train);
        var info = new InviteMatchControllerInfo
        {
            IsUnlock = train.InviteMatchUnlocked
        };

        foreach (var match in train.InviteMatches.OrderBy(x => x.Id))
        {
            info.MatchList.Add(new InviteMatchInfo
            {
                Id = match.Id,
                MineScore = match.MineScore,
                OpponentScore = match.OpponentScore,
                State = match.State,
                CdEndTime = match.CdEndTime,
                BaseReward = ToBigNumberInfo(match.BaseRewardValue, match.BaseRewardUnitId),
                OpponentName = match.OpponentName,
                OpponentIcon = match.OpponentIcon,
                Organizer = match.Organizer,
                Place = match.Place,
                Content = match.Content,
                OrganizerIcon = match.OrganizerIcon
            });
        }

        return info;
    }

    private static InviteMatchInfo ToInviteMatchInfo(InviteMatchState match)
    {
        return new InviteMatchInfo
        {
            Id = match.Id,
            MineScore = match.MineScore,
            OpponentScore = match.OpponentScore,
            State = match.State,
            CdEndTime = match.CdEndTime,
            BaseReward = ToBigNumberInfo(match.BaseRewardValue, match.BaseRewardUnitId),
            OpponentName = match.OpponentName,
            OpponentIcon = match.OpponentIcon,
            Organizer = match.Organizer,
            Place = match.Place,
            Content = match.Content,
            OrganizerIcon = match.OrganizerIcon
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

    private static UpdatePVPInfoNotify ToUpdatePvpInfo(PlayerState player)
    {
        return new UpdatePVPInfoNotify
        {
            LeagueTeamState = GetDisplayedLeagueTeamState(player),
            LeagueSettle = false
        };
    }

    private static PlayerLeagueInfo ToPlayerLeagueInfo(PlayerState player, int lastLeagueId)
    {
        var leagueId = lastLeagueId > 0 ? lastLeagueId : 1;
        return new PlayerLeagueInfo
        {
            State = GetDisplayedLeagueTeamState(player),
            LeagueId = leagueId,
            LeagueLevel = 1,
            LeagueRoundId = GetDisplayedLeagueTeamState(player) == 3 ? 1 : 0,
            SeasonId = 1,
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    private static int GetDisplayedLeagueTeamState(PlayerState player)
    {
        var storedState = GetLeagueTeamState(player);
        return storedState == 2 ? 3 : storedState;
    }

    private static int GetLeagueTeamState(PlayerState player)
    {
        return player.LeagueTeamState is >= 1 and <= 4 ? player.LeagueTeamState : 1;
    }

    private static GamePerviewData BuildLeagueGamePreview(PlayerState player, int leagueId)
    {
        var homeTeam = BuildCourseTeamData(
            ToPlayerTeamData(player.Gbid, player.Name, player.Icon),
            player.Strength,
            1,
            1,
            0,
            CreateMiniCards(player, boardStart: 1, boardCount: 5, substituteStart: 11, substituteCount: 3));
        var awayTeam = BuildCourseTeamData(
            ToPlayerTeamData("league-ai-1", "先锋队", 2),
            Math.Max(900, player.Strength - 40),
            2,
            0,
            1,
            CreateAiMiniCards());

        var matchTime = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        return new GamePerviewData
        {
            CompitionId = 1,
            LeagueId = leagueId,
            LeagueLevel = 1,
            LeagueRoundId = 1,
            CourseId = 1001,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            Time = matchTime,
            StandardTime = matchTime,
            ChangeTimeTimes = 0
        };
    }

    private static IEnumerable<LeagueCourseItemData> BuildLeagueCourseItems(PlayerState player, GamePerviewData preview, int type)
    {
        var allItems = new List<LeagueCourseItemData>
        {
            new()
            {
                CourseId = 901,
                Time = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
                Round = 1,
                HomeTeam = preview.HomeTeam.Team,
                AwayTeam = ToPlayerTeamData("league-ai-2", "雷霆会", 3),
                HomeGoal = 108,
                AwayGoal = 99,
                FightId = "0"
            },
            new()
            {
                CourseId = 902,
                Time = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
                Round = 1,
                HomeTeam = ToPlayerTeamData("league-ai-1", "先锋队", 2),
                AwayTeam = ToPlayerTeamData("league-ai-2", "雷霆会", 3),
                HomeGoal = 97,
                AwayGoal = 101,
                FightId = "0"
            },
            new()
            {
                CourseId = preview.CourseId,
                Time = preview.Time,
                Round = preview.LeagueRoundId,
                HomeTeam = preview.HomeTeam.Team,
                AwayTeam = preview.AwayTeam.Team,
                HomeGoal = -1,
                AwayGoal = -1,
                FightId = "0"
            }
        };

        return type == 1
            ? allItems.Where(item => item.HomeTeam.TeamId == player.Gbid || item.AwayTeam.TeamId == player.Gbid)
            : allItems;
    }

    private static IEnumerable<LeagueCardRankData> BuildLeagueCardRanks(PlayerState player)
    {
        return
        [
            new LeagueCardRankData
            {
                CardId = player.Cards.Values.FirstOrDefault()?.CardId ?? 104001,
                Team = ToPlayerTeamData(player.Gbid, player.Name, player.Icon),
                Point = 32,
                Rebound = 12,
                Assist = 9,
                Steal = 3,
                Block = 2
            },
            new LeagueCardRankData
            {
                CardId = 104002,
                Team = ToPlayerTeamData("league-ai-1", "先锋队", 2),
                Point = 28,
                Rebound = 10,
                Assist = 11,
                Steal = 4,
                Block = 1
            },
            new LeagueCardRankData
            {
                CardId = 104003,
                Team = ToPlayerTeamData("league-ai-2", "雷霆会", 3),
                Point = 24,
                Rebound = 13,
                Assist = 7,
                Steal = 2,
                Block = 3
            }
        ];
    }

    private static LeagueScorebarTeam BuildLeagueScorebar(PlayerState player, CourseTeamData courseTeam, int session, int obtain, int lost)
    {
        var team = new LeagueScorebarTeam
        {
            BaseData = courseTeam.Team,
            Session = session,
            Win = courseTeam.Win,
            Deuce = courseTeam.Deuce,
            Failed = courseTeam.Failed,
            Obtain = obtain,
            Lost = lost,
            Net = obtain - lost
        };
        team.Record.Add(courseTeam.Record);
        return team;
    }

    private static CourseTeamData BuildCourseTeamData(PlayerTeamData team, int strength, int rank, int win, int failed, (Dictionary<int, PlayerCardMiniInfo> Board, Dictionary<int, PlayerCardMiniInfo> Substitute) cards)
    {
        var result = new CourseTeamData
        {
            Team = team,
            Rank = rank,
            Strength = strength,
            Attack = Math.Max(100, strength / 10),
            Defence = Math.Max(100, strength / 10 - 8),
            FormationName = "均衡阵容",
            Win = win,
            Deuce = 0,
            Failed = failed
        };
        result.Record.Add(1);
        result.Record.Add(1);
        result.Record.Add(2);
        result.Record.Add(1);
        result.Record.Add(2);
        result.TacticsIdList.Add(101);
        result.TacticsIdList.Add(102);
        result.TacticsIdList.Add(103);
        foreach (var item in cards.Board)
        {
            result.BoardCardMap[item.Key] = item.Value;
        }

        foreach (var item in cards.Substitute)
        {
            result.SubstituteCardMap[item.Key] = item.Value;
        }

        return result;
    }

    private static (Dictionary<int, PlayerCardMiniInfo> Board, Dictionary<int, PlayerCardMiniInfo> Substitute) CreateMiniCards(PlayerState player, int boardStart, int boardCount, int substituteStart, int substituteCount)
    {
        var ordered = player.Cards.Values.OrderBy(card => card.CardId).ToList();
        var board = new Dictionary<int, PlayerCardMiniInfo>();
        var substitute = new Dictionary<int, PlayerCardMiniInfo>();

        for (var i = 0; i < boardCount; i++)
        {
            var card = ordered[i % ordered.Count];
            board[boardStart + i] = ToPlayerCardMiniInfo(card, boardStart + i, energy: 42f - i);
        }

        for (var i = 0; i < substituteCount; i++)
        {
            var card = ordered[(boardCount + i) % ordered.Count];
            substitute[substituteStart + i] = ToPlayerCardMiniInfo(card, substituteStart + i, energy: 39f - i);
        }

        return (board, substitute);
    }

    private static (Dictionary<int, PlayerCardMiniInfo> Board, Dictionary<int, PlayerCardMiniInfo> Substitute) CreateAiMiniCards()
    {
        var board = new Dictionary<int, PlayerCardMiniInfo>();
        var substitute = new Dictionary<int, PlayerCardMiniInfo>();

        for (var i = 0; i < 5; i++)
        {
            board[i + 1] = new PlayerCardMiniInfo
            {
                CardId = RecruitCardPool[i],
                Star = 3,
                Energy = 40f - i,
                Number = i + 1,
                CombatEffectiveness = 90 - i * 3,
                Quality = 2,
                BoardId = i + 1,
                Status = 4,
                InjuryType = 0
            };
        }

        for (var i = 0; i < 3; i++)
        {
            substitute[i + 11] = new PlayerCardMiniInfo
            {
                CardId = RecruitCardPool[i + 5],
                Star = 2,
                Energy = 37f - i,
                Number = i + 6,
                CombatEffectiveness = 78 - i * 2,
                Quality = 2,
                BoardId = i + 11,
                Status = 3,
                InjuryType = 1
            };
        }

        return (board, substitute);
    }

    private static PlayerCardMiniInfo ToPlayerCardMiniInfo(CardState card, int boardId, float energy)
    {
        return new PlayerCardMiniInfo
        {
            CardId = card.CardId,
            Star = Math.Max(1, card.Star),
            Energy = energy,
            Number = card.PlayerCardNumber,
            CombatEffectiveness = Math.Max(60, card.Strength),
            Quality = Math.Max(1, card.Quality),
            BoardId = boardId,
            Status = 4,
            InjuryType = 0
        };
    }

    private static PlayerTeamData ToPlayerTeamData(string teamId, string teamName, int teamIcon)
    {
        return new PlayerTeamData
        {
            TeamId = teamId,
            TeamName = string.IsNullOrWhiteSpace(teamName) ? "未命名战队" : teamName,
            TeamIcon = teamIcon,
            TeamType = 1,
            ServerId = 1,
            Support = 0
        };
    }

    private static FormationState EnsureFormationState(PlayerState player, int formationId)
    {
        if (player.Formations.TryGetValue(formationId, out var existing))
        {
            existing.FormationId = formationId;
            if (!HasValidStarterBoardIds(existing.StarterBoardCardMap))
            {
                existing.StarterBoardCardMap = NormalizeStarterBoardMap(existing.StarterBoardCardMap);
                if (existing.StarterBoardCardMap.Count == 0)
                {
                    existing.StarterBoardCardMap = CreateDefaultFormationState(player, formationId).StarterBoardCardMap;
                }
            }
            if (existing.TacticsIdList.Count != 2)
            {
                existing.TacticsIdList = [101, 201];
            }

            if (string.IsNullOrWhiteSpace(existing.FormationName))
            {
                existing.FormationName = GetDefaultFormationName(formationId);
            }

            if (string.IsNullOrWhiteSpace(existing.BaseFormationName))
            {
                existing.BaseFormationName = existing.FormationName;
            }

            return existing;
        }

        var created = CreateDefaultFormationState(player, formationId);
        player.Formations[formationId] = created;
        return created;
    }

    private static FormationState NormalizeFormation(int formationId, FormationInfo? source, PlayerState player)
    {
        var normalized = new FormationState
        {
            FormationId = formationId,
            BaseFormationName = string.IsNullOrWhiteSpace(source?.BaseFormationName) ? GetDefaultFormationName(formationId) : source.BaseFormationName,
            FormationName = string.IsNullOrWhiteSpace(source?.FormationName) ? GetDefaultFormationName(formationId) : source.FormationName,
            IsInitialized = source?.IsInitialized ?? true,
            LineupShowTime = source?.LineupShowTime ?? 0
        };

        if (source is not null)
        {
            foreach (var pair in source.StarterBoardCardMap)
            {
                normalized.StarterBoardCardMap[pair.Key] = pair.Value;
            }

            foreach (var pair in source.SubstituteBoardCardMap)
            {
                normalized.SubstituteBoardCardMap[pair.Key] = pair.Value;
            }

            normalized.TacticsIdList = source.TacticsIdList.ToList();
            foreach (var pair in source.TacticsLevels)
            {
                normalized.TacticsLevels[pair.Key] = pair.Value;
            }
        }

        if (normalized.StarterBoardCardMap.Count == 0)
        {
            var fallback = CreateDefaultFormationState(player, formationId);
            normalized.StarterBoardCardMap = fallback.StarterBoardCardMap;
            normalized.SubstituteBoardCardMap = fallback.SubstituteBoardCardMap;
            if (normalized.TacticsIdList.Count == 0)
            {
                normalized.TacticsIdList = fallback.TacticsIdList;
            }
        }

        if (!HasValidStarterBoardIds(normalized.StarterBoardCardMap))
        {
            normalized.StarterBoardCardMap = NormalizeStarterBoardMap(normalized.StarterBoardCardMap);
            if (normalized.StarterBoardCardMap.Count == 0)
            {
                normalized.StarterBoardCardMap = CreateDefaultFormationState(player, formationId).StarterBoardCardMap;
            }
        }

        if (normalized.TacticsIdList.Count != 2)
        {
            normalized.TacticsIdList = [101, 201];
        }

        return normalized;
    }

    private static FormationState CreateDefaultFormationState(PlayerState player, int formationId)
    {
        var formation = new FormationState
        {
            FormationId = formationId,
            BaseFormationName = GetDefaultFormationName(formationId),
            FormationName = GetDefaultFormationName(formationId),
            TacticsIdList = [101, 201],
            IsInitialized = true,
            LineupShowTime = 0
        };

        var cards = player.Cards.Values.OrderBy(card => card.CardId).ToList();
        for (var i = 0; i < Math.Min(5, cards.Count); i++)
        {
            formation.StarterBoardCardMap[DefaultStarterBoardIds[i]] = cards[i].CardId;
        }

        for (var i = 0; i < Math.Min(3, Math.Max(0, cards.Count - 5)); i++)
        {
            formation.SubstituteBoardCardMap[i + 1] = cards[i + 5].CardId;
        }

        return formation;
    }

    private static bool HasValidStarterBoardIds(Dictionary<int, int> starterBoardCardMap)
    {
        return starterBoardCardMap.Count > 0
            && starterBoardCardMap.Keys.All(boardId => DefaultStarterBoardIds.Contains(boardId));
    }

    private static Dictionary<int, int> NormalizeStarterBoardMap(Dictionary<int, int> source)
    {
        var normalized = new Dictionary<int, int>();
        var orderedCardIds = source
            .OrderBy(pair => pair.Key)
            .Select(pair => pair.Value)
            .Where(cardId => cardId != 0)
            .ToList();

        for (var i = 0; i < Math.Min(DefaultStarterBoardIds.Length, orderedCardIds.Count); i++)
        {
            normalized[DefaultStarterBoardIds[i]] = orderedCardIds[i];
        }

        return normalized;
    }

    private static FormationInfo ToFormationInfo(FormationState formation)
    {
        var info = new FormationInfo
        {
            FormationId = formation.FormationId,
            BaseFormationName = formation.BaseFormationName,
            FormationName = formation.FormationName,
            IsInitialized = formation.IsInitialized,
            LineupShowTime = formation.LineupShowTime
        };
        foreach (var pair in formation.StarterBoardCardMap)
        {
            info.StarterBoardCardMap[pair.Key] = pair.Value;
        }

        foreach (var pair in formation.SubstituteBoardCardMap)
        {
            info.SubstituteBoardCardMap[pair.Key] = pair.Value;
        }

        info.TacticsIdList.Add(formation.TacticsIdList);
        foreach (var pair in formation.TacticsLevels)
        {
            info.TacticsLevels[pair.Key] = pair.Value;
        }

        return info;
    }

    private static string GetDefaultFormationName(int formationId)
    {
        return formationId switch
        {
            1 => "联赛阵容",
            2 => "副本阵容",
            3 => "竞技场阵容",
            4 => "经典赛阵容",
            _ => $"阵容{formationId}"
        };
    }

    private static ArenaInfo BuildArenaInfo(PlayerState player)
    {
        var info = new ArenaInfo
        {
            ArenaScore = 1280,
            ArenaStage = 1,
            ArenaRank = 32,
            SeasonId = 1,
            DailyClaim = true,
            BattleTimesLeft = 5,
            RefreshTimesLeft = 3,
            BattleTimesBuy = 0,
            ShopRefreshTime = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeSeconds()
        };
        info.ShopList.Add(new ShopInfo { Sid = 1, Stock = 1 });
        info.ShopList.Add(new ShopInfo { Sid = 2, Stock = 1 });
        info.ShopList.Add(new ShopInfo { Sid = 3, Stock = 1 });
        return info;
    }

    private static IEnumerable<ArenaRankInfo> BuildArenaTopRanks(PlayerState player)
    {
        return
        [
            new ArenaRankInfo
            {
                Rank = 1,
                Gbid = player.Gbid,
                Name = player.Name,
                Icon = player.Icon,
                Record = 7
            },
            new ArenaRankInfo
            {
                Rank = 2,
                Gbid = "arena-ai-1",
                Name = "先锋队",
                Icon = 2,
                Record = 5
            },
            new ArenaRankInfo
            {
                Rank = 3,
                Gbid = "arena-ai-2",
                Name = "烈火队",
                Icon = 3,
                Record = -2
            }
        ];
    }

    private static IEnumerable<ArenaTeamData> BuildArenaOpponents(PlayerState player)
    {
        return
        [
            BuildArenaOpponent("arena-op-1", "北境队", 4, 1, 28, Math.Max(900, player.Strength - 30)),
            BuildArenaOpponent("arena-op-2", "星海队", 5, 2, 18, Math.Max(920, player.Strength - 10)),
            BuildArenaOpponent("arena-op-3", "赤霄队", 6, 1, 0, Math.Max(950, player.Strength + 20))
        ];
    }

    private static ArenaTeamData BuildArenaOpponent(string id, string name, int icon, int stage, int rank, int combatEffectiveness)
    {
        var data = new ArenaTeamData
        {
            Type = 1,
            Id = id,
            Name = name,
            Icon = icon,
            Stage = stage,
            Rank = rank,
            CombatEffectiveness = combatEffectiveness,
            AddScore = 15
        };

        for (var i = 0; i < 5; i++)
        {
            data.StarterPlayerList.Add(new PlayerCardMiniInfo
            {
                CardId = RecruitCardPool[i],
                Star = 2,
                Energy = 45f - i,
                Number = i + 1,
                CombatEffectiveness = Math.Max(60, combatEffectiveness / 10 - i * 2),
                Quality = 2,
                BoardId = i + 1,
                Status = 4,
                InjuryType = 0
            });
        }

        data.TacticsIdList.Add(101);
        data.TacticsIdList.Add(102);
        data.TacticsIdList.Add(103);
        return data;
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

    private static BigNumberInfo ToBigNumberInfo(double value, int unitId)
    {
        return new BigNumberInfo
        {
            Value = value,
            UnitId = unitId
        };
    }

    private static double FromBigNumberInfo(BigNumberInfo? info)
    {
        return info?.Value ?? 0d;
    }

    private static TrainState EnsureTrainState(PlayerState player)
    {
        player.Train ??= CreateDefaultTrainState();

        foreach (var elementId in TrainElementIds)
        {
            if (!player.Train.Elements.ContainsKey(elementId))
            {
                player.Train.Elements[elementId] = CreateDefaultTrainElementState(elementId);
            }
        }

        return player.Train;
    }

    private static void EnsureInviteMatchState(TrainState train)
    {
        if (!train.InviteMatchUnlocked)
        {
            train.InviteMatches.Clear();
            return;
        }

        if (train.InviteMatches.Count > 0)
        {
            return;
        }

        train.InviteMatches.Add(new InviteMatchState
        {
            Id = 1,
            State = 1,
            BaseRewardValue = 120,
            OpponentName = "北京疾风",
            OpponentIcon = 1,
            Organizer = "城市邀请赛",
            Place = "首都球馆",
            Content = "{Organizer} 邀请 {OpponentName} 在 {Place} 对决",
            OrganizerIcon = "invite_1"
        });
        train.InviteMatches.Add(new InviteMatchState
        {
            Id = 2,
            State = 1,
            BaseRewardValue = 180,
            OpponentName = "申城先锋",
            OpponentIcon = 2,
            Organizer = "精英挑战赛",
            Place = "东方体育馆",
            Content = "{Organizer} 邀请 {OpponentName} 在 {Place} 对决",
            OrganizerIcon = "invite_2"
        });
    }

    private static TrainState CreateDefaultTrainState()
    {
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var train = new TrainState
        {
            ExpValue = 1000,
            TotalExpValue = 1000,
            OfflineExpBeginTime = nowMs
        };

        foreach (var elementId in TrainElementIds)
        {
            train.Elements[elementId] = CreateDefaultTrainElementState(elementId);
        }

        return train;
    }

    private static TrainElementState CreateDefaultTrainElementState(int elementId)
    {
        return new TrainElementState
        {
            Id = elementId,
            IncomeAddValue = 1,
            TimeReduceValue = 1,
            ConsumeReduceValue = 1
        };
    }

    private static void AccrueTrainIncome(PlayerState player, long nowMs)
    {
        var train = EnsureTrainState(player);
        if (nowMs <= 0)
        {
            return;
        }

        double totalDelta = 0;
        foreach (var element in train.Elements.Values)
        {
            if (element.Level <= 0)
            {
                element.LastIncomeTime = nowMs;
                continue;
            }

            if (element.LastIncomeTime <= 0)
            {
                element.LastIncomeTime = nowMs;
                continue;
            }

            var elapsedSeconds = Math.Max(0d, (nowMs - element.LastIncomeTime) / 1000d);
            if (elapsedSeconds <= 0)
            {
                continue;
            }

            totalDelta += elapsedSeconds * element.Level * Math.Max(1d, element.IncomeAddValue);
            element.LastIncomeTime = nowMs;
        }

        if (totalDelta > 0)
        {
            train.ExpValue += totalDelta;
            train.TotalExpValue += totalDelta;
            train.OfflineExpValue = totalDelta;
            train.OfflineExpBeginTime = nowMs;
        }
        else if (train.OfflineExpBeginTime <= 0)
        {
            train.OfflineExpBeginTime = nowMs;
        }
    }

    private static bool TryApplyTrainEvent(PlayerState player, TrainEvent trainEvent, long nowMs)
    {
        var train = EnsureTrainState(player);
        var eventTime = trainEvent.Time > 0 ? trainEvent.Time : nowMs;
        if (eventTime > nowMs + 10_000)
        {
            return false;
        }

        if (eventTime < train.LastAcceptedEventTime)
        {
            return false;
        }

        var accepted = trainEvent.Event switch
        {
            1 => ApplyTrainUpgradeEvent(train, trainEvent, nowMs),
            2 => ApplyTrainStrengthenEvent(train, trainEvent),
            3 => ApplyTrainBreakEvent(train, trainEvent, player, nowMs),
            4 => ApplyTrainBigBangEvent(train, nowMs),
            _ => false
        };

        if (accepted)
        {
            train.LastAcceptedEventTime = eventTime;
        }

        return accepted;
    }

    private static bool ApplyTrainUpgradeEvent(TrainState train, TrainEvent trainEvent, long nowMs)
    {
        if (!train.Elements.TryGetValue(trainEvent.Arg1, out var element))
        {
            return false;
        }

        var upgradeLevels = Math.Max(1, trainEvent.Arg2);
        element.Level += upgradeLevels;
        if (element.LastIncomeTime <= 0)
        {
            element.LastIncomeTime = nowMs;
        }

        var cost = Math.Max(0d, FromBigNumberInfo(trainEvent.Cost));
        train.ExpValue = Math.Max(0d, train.ExpValue - cost);
        return true;
    }

    private static bool ApplyTrainStrengthenEvent(TrainState train, TrainEvent trainEvent)
    {
        if (trainEvent.Arg1 <= 0 || train.StrengthenTrainedList.Contains(trainEvent.Arg1))
        {
            return false;
        }

        train.StrengthenTrainedList.Add(trainEvent.Arg1);
        return true;
    }

    private static bool ApplyTrainBreakEvent(TrainState train, TrainEvent trainEvent, PlayerState player, long nowMs)
    {
        if (!train.Elements.TryGetValue(trainEvent.Arg1, out var element))
        {
            return false;
        }

        element.BreakIndex += 1;
        element.RewardLevel += 1;
        player.Strength += 50;
        if (trainEvent.Arg1 == 5)
        {
            train.InviteMatchUnlocked = true;
        }

        if (element.LastIncomeTime <= 0)
        {
            element.LastIncomeTime = nowMs;
        }

        return true;
    }

    private static bool ApplyTrainBigBangEvent(TrainState train, long nowMs)
    {
        train.BigBangTimes += 1;
        train.LastCdTime = nowMs;
        train.ForceValue += 1;
        return true;
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
        UpdatePVPInfoNotify PvpInfo,
        TrainInfoNotify TrainInfo,
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

    public sealed record SyncTrainEventsResult(
        SyncTrainEventsResponse Response,
        TrainInfoNotify TrainInfo);

    public sealed record DoOfflineRewardResult(
        DoOfflineRewardResponse Response,
        TrainInfoNotify TrainInfo);

    public sealed record FetchInviteMatchInfoResult(
        FetchInviteMatchInfoResponse Response);

    public sealed record GetArenaInfoResult(
        ArenaInfoResponse Response);

    public sealed record SaveFormationResult(
        SaveFormationResponse Response);

    public sealed record GetDefaultFormationResult(
        GetDefaultFormationResponse Response);

    public sealed record GetLeagueDataResult(
        GetLeagueDataResponse Response);

    public sealed record GetLeagueSignUpResult(
        GetLeagueSignUpResponse Response,
        UpdatePVPInfoNotify PvpInfo);

    public sealed record GetLeagueHistoryResult(
        GetLeagueHistoryResponse Response);

    public sealed record GetLeagueChampionRankResult(
        GetLeagueChampionRankResponse Response);

    public sealed record GetLeagueCourseResult(
        GetLeagueCourseResponse Response);

    public sealed record GetLeagueCardRankResult(
        GetLeagueCardRankResponse Response);

    public sealed record DoInviteMatchResult(
        DoInviteMatchResponse Response,
        TrainInfoNotify TrainInfo);

    public sealed record DoInviteMatchRewardResult(
        DoInviteMatchRewardResponse Response,
        TrainInfoNotify TrainInfo);

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
        public int LeagueTeamState { get; set; } = 1;
        public Dictionary<int, FormationState> Formations { get; set; } = [];
        public TrainState? Train { get; set; }
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

    private sealed class FormationState
    {
        public int FormationId { get; set; }
        public string BaseFormationName { get; set; } = string.Empty;
        public string FormationName { get; set; } = string.Empty;
        public Dictionary<int, int> StarterBoardCardMap { get; set; } = [];
        public Dictionary<int, int> SubstituteBoardCardMap { get; set; } = [];
        public List<int> TacticsIdList { get; set; } = [];
        public bool IsInitialized { get; set; } = true;
        public long LineupShowTime { get; set; }
        public Dictionary<int, int> TacticsLevels { get; set; } = [];
    }

    private sealed class TrainState
    {
        public double ExpValue { get; set; }
        public int ExpUnitId { get; set; }
        public double TotalExpValue { get; set; }
        public int TotalExpUnitId { get; set; }
        public double ForceValue { get; set; }
        public int ForceUnitId { get; set; }
        public double ForceAdd { get; set; }
        public int UpLevelType { get; set; }
        public long OfflineExpBeginTime { get; set; }
        public double OfflineExpValue { get; set; }
        public int OfflineExpUnitId { get; set; }
        public long LastAcceptedEventTime { get; set; }
        public List<int> StrengthenTrainedList { get; set; } = [];
        public int BigBangTimes { get; set; }
        public int ClearCdTimes { get; set; }
        public long LastCdTime { get; set; }
        public bool InviteMatchUnlocked { get; set; }
        public List<InviteMatchState> InviteMatches { get; set; } = [];
        public Dictionary<int, TrainElementState> Elements { get; set; } = [];
    }

    private sealed class TrainElementState
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int RewardLevel { get; set; }
        public double IncomeAddValue { get; set; } = 1;
        public int IncomeAddUnitId { get; set; }
        public double TimeReduceValue { get; set; } = 1;
        public int TimeReduceUnitId { get; set; }
        public double ConsumeReduceValue { get; set; } = 1;
        public int ConsumeReduceUnitId { get; set; }
        public int BreakIndex { get; set; }
        public long LastIncomeTime { get; set; }
    }

    private sealed class InviteMatchState
    {
        public int Id { get; set; }
        public int MineScore { get; set; }
        public int OpponentScore { get; set; }
        public int State { get; set; }
        public long CdEndTime { get; set; }
        public double BaseRewardValue { get; set; }
        public int BaseRewardUnitId { get; set; }
        public string OpponentName { get; set; } = string.Empty;
        public int OpponentIcon { get; set; }
        public string Organizer { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string OrganizerIcon { get; set; } = string.Empty;
    }

    private sealed record GiftReward(int Type, int Id, int Count);

    private sealed class ServerStateSnapshot
    {
        public List<AccountState> Accounts { get; set; } = [];
    }
}

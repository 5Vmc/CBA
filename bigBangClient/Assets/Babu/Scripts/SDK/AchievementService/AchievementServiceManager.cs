using System;

namespace Babu.SDK
{
    class AchievementServiceManager : BabuSingleton<AchievementServiceManager>
    {
        protected AchievementService _achievementService = new AchievementServiceDefault();

        public void SetAchievementService(AchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        public void ReportAchievementProgress(string achievementId, double progress, Action<bool> callback)
        {
            _achievementService.ReportAchievementProgress(achievementId, progress, callback);
        }

        public void ReportLeaderboardScore(string leaderboardId, long score, Action<bool> callback)
        {
            _achievementService.ReportLeaderboardScore(leaderboardId, score, callback);
        }

        public void ShowAchievementsUI()
        {
            _achievementService.ShowAchievementsUI();
        }

        public void ShowLeaderboardUI()
        {
            _achievementService.ShowLeaderboardUI();
        }
    }
}

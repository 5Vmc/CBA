using System;

namespace Babu.SDK
{
    public abstract class AchievementService
    {
        public abstract void ReportAchievementProgress(string achievementId, double progress, Action<bool> callback);
        public abstract void ReportLeaderboardScore(string leaderboardId, long score, Action<bool> callback);

        public abstract void ShowAchievementsUI();
        public abstract void ShowLeaderboardUI();
    }
}

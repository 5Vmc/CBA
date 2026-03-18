using System;
using UnityEngine;

namespace Babu.SDK
{
    class AchievementServiceDefault : AchievementService
    {
        public override void ReportAchievementProgress(string achievementId, double progress, Action<bool> callback)
        {
            Debug.Log($"ReportAchievementProgress: {achievementId}, {progress}");
            callback(true);
        }

        public override void ReportLeaderboardScore(string leaderboardId, long score, Action<bool> callback)
        {
            Debug.Log($"ReportScore: {leaderboardId}, {score}");
            callback(true);
        }

        public override void ShowAchievementsUI()
        {
            Debug.Log("ShowAchievementsUI");
        }

        public override void ShowLeaderboardUI()
        {
            Debug.Log("ShowLeaderboardUI");
        }
    }
}

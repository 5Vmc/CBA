using System.Collections.Generic;
using UnityEngine;

namespace BigBang.UI
{
    public class StarListItem : MonoBehaviour
    {
        [SerializeField] private List<StarItem> stars;

        // 当前星级
        private int currentLevel;
        private int quality;

        // 设置星星图片
        public void SetImage(int quality)
        {
            this.quality = quality;
            stars.ForEach(item => item.SetStar(quality));
        }


        public void SetStarAndHideLeftForeground(int level)
        {
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(true);
                stars[i].SetStarAsNormal();
                stars[i].SetState(i + 1 <= level);
                if (level <= 5)
                    stars[i].SetState(i + 1 <= level);
                else
                {
                    stars[i].SetState(i + 1 <= level - 5);
                }
                stars[i].StopFlash();
            }
            currentLevel = level;
            if (level > 5)
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    if (i + 1 <= level - 5)
                        stars[i].SetStarAsColorful();
                }
            }

        }
        // 设置星级(level从0开始,最大值为10)
        public void SetLevel(int level, bool hideLeftStar = false)
        {
            currentLevel = level;

            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(true);

                stars[i].SetStarAsNormal();
                stars[i].SetState(i + 1 <= level);

                stars[i].StopFlash();
            }

            if (level > 5)
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    if (i + 1 <= level - 5)
                        stars[i].SetStarAsColorful();

                    /*if (level != 10)
                    {
                        stars[i].SetState(i + 1 <= level % stars.Count);
                    }*/
                }
            }

            if (hideLeftStar)
            {
                int start = level;
                if (level > 5)
                    start = level - 5;
                for (int i = start; i < stars.Count; i++)
                {

                    stars[i].gameObject.SetActive(false);
                }
            }

        }

        public void HideAllStar()
        {
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(false);
            }
        }


        // 获得当前星级
        public int GetLevel()
        {
            return currentLevel;
        }

        // 升级并播放动画
        /// <summary>
        /// 
        /// </summary>
        /// <param name="showNextStarFrame">是否展示下一个虚影</param>
        public void UpgradeAndPlayAnim(bool showNextStarFrame = false)
        {
            var item = GetNextLevelStar();
            // 星星砸入动画
            item?.PlayStar();
            currentLevel++;

            if (currentLevel != stars.Count)
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    if (currentLevel != 10)
                    {
                        stars[i].SetState(i + 1 <= currentLevel % stars.Count);
                    }
                }
            }

            if (showNextStarFrame && currentLevel % stars.Count != 0)
            {
                // 等待数值滚动完成后显示下一星星的虚影效果
                Babu.DelayTaskService.Instance.Run(this.gameObject, 3f, () =>
                {
                    item = GetNextLevelStar();
                    // 下一个星星的虚影效果
                    item?.PlayFlash();
                });
            }
        }

        // 获得下一个星级的星星
        public StarItem GetNextLevelStar()
        {
            var star = stars[currentLevel % stars.Count];
            if (currentLevel >= stars.Count)
            {
                star.SetStarAsColorful();
            }
            else
            {
                star.SetStarAsNormal();
            }
            return stars[currentLevel % stars.Count];
        }

        // 获得不发光的星星
        public IEnumerable<StarItem> GetNoLightStar()
        {
            for (int i = 0; i < stars.Count; i++)
            {
                if (i != currentLevel % stars.Count)
                {
                    yield return stars[i];
                }
            }
        }

        // 获得当前星级的星星
        public StarItem GetCurrentStar()
        {
            if (currentLevel - 1 < 0) return null;
            return stars[currentLevel - 1];
        }
    }
}
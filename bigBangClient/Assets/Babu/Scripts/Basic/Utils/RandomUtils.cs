using System;
using System.Collections.Generic;
using System.Linq;

namespace Babu
{
    public static class RandomUtils
    {
        private static Random s_random = new Random(GetRandomSeed());

        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int Random()
        {
            return s_random.Next();
        }

        // [0, limit)
        public static int Random(int limit)
        {
            return Random() % limit;
        }

        // [left, right]
        public static int Range(int left, int right)
        {
            return s_random.Next(left, right + 1);
        }

        public static bool PercentageResult(int percent)
        {
            if (percent <= 0)
            {
                return false;
            }

            if (percent >= 100)
            {
                return true;
            }

            return Range(0, 99) < percent;
        }

        /// <summary>
        /// 按加权随机算法抽取1个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">可迭代数据源</param>
        /// <param name="predicate">权重谓语</param>
        /// <returns>抽取结果</returns>
        public static T WeightedRandom<T>(this IEnumerable<T> source, Func<T, float> predicate, Random random = null)
        {
            random ??= new Random();
            // 总权重
            float sumWeight = source.Sum(item => predicate(item));
            // 游标
            float cursor = 0;
            // 随机值[0,sumWeight)
            float value = (float)random.NextDouble() * sumWeight;
            // 遍历1次
            foreach (var item in source)
            {
                // 当前元素权重
                float weight = predicate(item);
                // 如果权重<=0，则不会抽中
                if (weight <= 0) continue;
                // 移动游标
                cursor += weight;
                // 如果游标>=随机值,或游标>=总权重,则被抽中
                if (cursor >= value || cursor >= sumWeight) return item;
            }
            return default;
        }

        /// <summary>
        /// 按加权随机算法抽取n个不重复的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">可迭代数据源</param>
        /// <param name="n">抽取个数</param>
        /// <param name="predicate">权重谓语</param>
        /// <returns>抽取结果</returns>
        public static IEnumerable<T> WeightedRandom<T>(this IEnumerable<T> source, int n, Func<T, float> predicate, Random random = null)
        {
            random ??= new Random();
            // 总权重
            float sumWeight = source.Sum(item => predicate(item));
            // 抽取n次
            for (int i = 0; i < n; i++)
            {
                // 游标
                float cursor = 0;
                // 随机值[0,sumWeight)
                float value = (float)random.NextDouble() * sumWeight;
                // 抽取结果
                T result = default;
                // 遍历1次
                foreach (var item in source)
                {
                    // 当前元素权重
                    float weight = predicate(item);
                    // 如果权重<=0，则不会抽中
                    if (weight <= 0) continue;
                    // 移动游标
                    cursor += weight;
                    // 如果游标>=随机值,或游标>=总权重,则被抽中
                    if (cursor >= value || cursor >= sumWeight)
                    {
                        // 总权重减小
                        sumWeight -= weight;
                        // 保存抽中的元素
                        result = item;
                        // 返回抽取的结果
                        yield return result;
                        // 结束当前抽取
                        break;
                    }
                }
                // 将抽取过的元素排除在迭代过程中
                source = source.Where(item => !Equals(item, result));
            }
        }

        /// <summary>
        /// 随机抽取1个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns>抽取结果</returns>
        public static T Random<T>(this IList<T> source, Random random = null)
        {
            random ??= new Random();
            if (source == null || source.Count <= 0) return default;
            return source[random.Next(0, source.Count)];
        }

        /// <summary>
        /// 随机抽取1个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">可迭代数据源</param>
        /// <returns>抽取结果</returns>
        public static T Random<T>(this IEnumerable<T> source, Random random = null)
        {
            random ??= new Random();
            return source.ElementAtOrDefault(random.Next(0, source.Count()));
        }

        /// <summary>
        /// 随机抽取n个不重复的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">可迭代数据源</param>
        /// <param name="n">抽取个数</param>
        /// <returns>抽取结果</returns>
        public static IEnumerable<T> Random<T>(this IEnumerable<T> source, int n, Random random = null)
        {
            random ??= new Random();
            var copy = source.ToArray();
            for (int i = 0; i < n; i++)
            {
                if (i >= copy.Length) continue;
                var j = random.Next(i, copy.Length);
                (copy[i], copy[j]) = (copy[j], copy[i]);
                yield return copy[i];
            }
        }

        /// <summary>
        /// 打乱元素顺序
        /// </summary>
        /// <param name="source">数据源</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>打乱结果</returns>
        public static IEnumerable<T> Disorder<T>(this IEnumerable<T> source, Random random = null)
        {
            random ??= new Random();
            var copy = source.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                var j = random.Next(i, copy.Length);
                // 交换元素
                (copy[j], copy[i]) = (copy[i], copy[j]);
                yield return copy[i];
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TotalCommander
{
    /// <summary>
    /// 자주 사용되는 아이콘을 캐싱하여 성능 향상을 위한 클래스
    /// </summary>
    public static class LoadedIconsCache
    {
        // 아이콘 캐시 (파일 확장자/경로 -> 아이콘)
        private static readonly Dictionary<string, Icon> IconCache = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// 캐시에서 아이콘을 가져오거나, 없으면 시스템에서 로드합니다.
        /// </summary>
        /// <param name="key">파일 확장자 또는 전체 경로</param>
        /// <param name="loadFunc">아이콘을 로드하는 함수</param>
        /// <returns>캐시된 아이콘 또는 새로 로드된 아이콘</returns>
        public static Icon GetOrLoadIcon(string key, Func<Icon> loadFunc)
        {
            if (string.IsNullOrEmpty(key))
                return null;
                
            // 캐시에 아이콘이 있으면 바로 반환
            if (IconCache.TryGetValue(key, out Icon cachedIcon))
            {
                return cachedIcon;
            }
            
            // 캐시에 없으면 로드 함수를 사용하여 로드
            try
            {
                Icon newIcon = loadFunc();
                if (newIcon != null)
                {
                    // 로드된 아이콘을 캐시에 저장
                    IconCache[key] = newIcon;
                    return newIcon;
                }
            }
            catch (Exception ex)
            {
                // 로깅 추가
                Logger.Error(ex, $"아이콘 로드 실패: {key}");
            }
            
            return null;
        }
        
        /// <summary>
        /// 캐시에서 아이콘을 제거합니다.
        /// </summary>
        /// <param name="key">제거할 아이콘의 키</param>
        public static void RemoveFromCache(string key)
        {
            if (IconCache.ContainsKey(key))
            {
                IconCache.Remove(key);
            }
        }
        
        /// <summary>
        /// 캐시를 초기화합니다.
        /// </summary>
        public static void ClearCache()
        {
            // 모든 아이콘 리소스 해제
            foreach (var icon in IconCache.Values)
            {
                icon.Dispose();
            }
            
            // 캐시 비우기
            IconCache.Clear();
        }
    }
} 
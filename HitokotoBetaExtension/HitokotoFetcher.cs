using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HitokotoBetaExtension
{
    internal static class HitokotoFetcher
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<HitokotoItem> FetchAsync(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<HitokotoItem>(json);
                return item;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取一言失败: {ex.Message}");
                return new HitokotoItem
                {
                    Hitokoto = "获取一言失败，请检查网络后重试",
                    From = "系统",
                    FromWho = ""
                };
            }
        }
    }
}
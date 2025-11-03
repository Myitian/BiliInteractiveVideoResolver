using BiliInteractiveVideoResolver;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibBiliInteractiveVideo.API;

/// <summary>
/// x/web-interface/view/detail
/// </summary>
public static class XWebInterfaceViewDetail
{
    public static event Action<string>? RequestReady;
    public static event Action<string>? RawJsonReceived;

    public struct Root
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("View")]
        public View View { get; set; }
    }

    public struct View
    {
        [JsonPropertyName("aid")]
        public ulong Aid { get; set; }

        [JsonPropertyName("cid")]
        public ulong Cid { get; set; }
    }

    public static async Task<Root> GetAsync(
        HttpClient client,
        ulong? aid = null,
        string? bvid = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string url = $"https://api.bilibili.com/x/web-interface/view/detail?{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}";
        RequestReady?.Invoke(url);
        if (RawJsonReceived is not null)
        {
            string json = await client.GetStringAsync(url, cancellationToken);
            RawJsonReceived?.Invoke(json);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XWebInterfaceViewDetail_Root);
        }
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XWebInterfaceViewDetail_Root, cancellationToken);
    }
}

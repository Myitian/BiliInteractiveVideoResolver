using BiliInteractiveVideoResolver;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibBiliInteractiveVideo.API;

/// <summary>
/// x/player/v2
/// </summary>
public class XPlayerV2
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
        [JsonPropertyName("interaction")]
        public Interaction Interaction { get; set; }
    }

    public struct Interaction
    {
        [JsonPropertyName("graph_version")]
        public ulong GraphVersion { get; set; }
    }

    public static async Task<Root> GetAsync(
        HttpClient client,
        ulong cid,
        ulong? aid = null,
        string? bvid = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string url = $"https://api.bilibili.com/x/player/v2?cid={cid}{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}";
        RequestReady?.Invoke(url);
        if (RawJsonReceived is not null)
        {
            string json = await client.GetStringAsync(url, cancellationToken);
            RawJsonReceived?.Invoke(json);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XPlayerV2_Root);
        }
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XPlayerV2_Root, cancellationToken);
    }
}
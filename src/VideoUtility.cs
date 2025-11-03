using LibBiliInteractiveVideo.API;
using System.Runtime.CompilerServices;

namespace LibBiliInteractiveVideo;

public static class VideoUtility
{
    public static async Task<(ulong GraphVersion, ulong Aid, ulong Cid)> GetGraphVersion(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default)
    {
        if (id.StartsWith("av", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(id.AsSpan(2), out ulong aid)
            || ulong.TryParse(id, out aid))
            return await GetGraphVersion(httpClient, aid: aid, cancellationToken: cancellationToken);
        else
            return await GetGraphVersion(httpClient, bvid: id, cancellationToken: cancellationToken);
    }

    public static async Task<(ulong GraphVersion, ulong Aid, ulong Cid)> GetGraphVersion(
        HttpClient httpClient,
        ulong? aid = null,
        string? bvid = null,
        CancellationToken cancellationToken = default)
    {
        XWebInterfaceViewDetail.Root detail = await XWebInterfaceViewDetail.GetAsync(httpClient, aid, bvid, cancellationToken);
        if (detail.Data is null)
            throw new Exception(detail.Message);
        aid = detail.Data.View.Aid;
        ulong cid = detail.Data.View.Cid;
        XPlayerV2.Root player = await XPlayerV2.GetAsync(httpClient, cid, aid, cancellationToken: cancellationToken);
        if (player.Data is null)
            throw new Exception(player.Message);
        return (player.Data.Interaction.GraphVersion, aid.Value, cid);
    }

    /// <param name="cid">only used to return this value on the first iteration.</param>
    public static async IAsyncEnumerable<(XSteinEdgeinfoV2.Data Edge, ulong cid)> ResolveAllEdges(
        HttpClient httpClient,
        ulong graphVersion,
        ulong cid = 0,
        ulong? aid = null,
        string? bvid = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        XSteinEdgeinfoV2.Root edge = await XSteinEdgeinfoV2.GetAsync(httpClient, graphVersion, aid, bvid, cancellationToken: cancellationToken);
        if (edge.Data is null)
            throw new Exception(edge.Message);
        HashSet<ulong> eids = [];
        Stack<(XSteinEdgeinfoV2.Choice[], int)> stack = [];
        stack.Push(([new() { Id = edge.Data.EdgeId, Cid = cid }], 0));
        while (stack.Count > 0)
        {
            (XSteinEdgeinfoV2.Choice[] choices, int index) = stack.Pop();
            if (index >= choices.Length)
                continue;
            XSteinEdgeinfoV2.Choice choice = choices[index++];
            stack.Push((choices, index));
            if (eids.Add(choice.Id))
            {
                edge = await XSteinEdgeinfoV2.GetAsync(httpClient, graphVersion, aid, bvid, choice.Id, cancellationToken);
                if (edge.Data is null)
                    throw new Exception(edge.Message);
                yield return (edge.Data, choice.Cid);
                XSteinEdgeinfoV2.Choice[]? a = edge.Data.Edges.Questions?.SelectMany(it => (IEnumerable<XSteinEdgeinfoV2.Choice>?)it.Choices ?? []).ToArray();
                stack.Push((a ?? [], 0));
            }
        }
    }
}
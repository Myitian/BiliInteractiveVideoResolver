using LibBiliInteractiveVideo;
using LibBiliInteractiveVideo.API;

namespace ResolveAllEdges;

class Program
{
    static async Task Main(string[] args)
    {
#if DEBUG
        XWebInterfaceViewDetail.RequestReady += Console.Error.WriteLine;
        XWebInterfaceViewDetail.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XWebInterfaceViewDetail.json", json);
        XPlayerV2.RequestReady += Console.Error.WriteLine;
        XPlayerV2.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XPlayerV2.json", json);
        XSteinEdgeinfoV2.RequestReady += Console.Error.WriteLine;
        XSteinEdgeinfoV2.RawJsonReceived += json => File.WriteAllText($"{DateTime.UtcNow.Ticks}.XSteinEdgeinfoV2.json", json);
#endif
        using HttpClient httpClient = new();
        if (args.Length == 0)
        {
            while (true)
            {
                Console.Error.WriteLine("AID/BVID:");
                string? line = Console.In.ReadLine();
                if (line is null)
                    break;
                await ProcessId(httpClient, line);
                Console.Out.WriteLine();
            }
        }
        else
        {
            foreach (string line in args)
            {
                await ProcessId(httpClient, line);
                Console.Out.WriteLine();
            }
        }
    }

    static async Task ProcessId(HttpClient httpClient, string id)
    {
        try
        {
            (ulong graphVersion, ulong aid, ulong cid0) = await VideoUtility.GetGraphVersion(httpClient, id);
            await foreach ((XSteinEdgeinfoV2.Data edge, ulong cid) in VideoUtility.ResolveAllEdges(httpClient, graphVersion, cid0, aid))
                Console.Out.WriteLine($"{edge.EdgeId}:{cid}:{edge.Title?.ReplaceLineEndings("")}");
        }
        catch (Exception ex)
        {
            string msg = ex?.Message ?? "";
            Console.Out.Write('!');
            Console.Out.WriteLine(msg.ReplaceLineEndings(""));
            Console.Error.WriteLine(ex);
        }
    }
}
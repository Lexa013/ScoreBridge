using ScoreBridge.Server.Listeners;
using ScoreBridge.Server.Parsers;
using ScoreBridge.Server.SeedWork;

namespace ScoreBridge.Server.Scoreboards.Scorepad;

public class ScorepadScoreboard : Scoreboard
{
    public IListener? Listener { get; set; }
    public IParser? Parser { get; set; }

    public ScorepadScoreboard(TcpListenerOptions options)
    {
        Parser = new BodetParser();
        Listener = new TcpListener(options, Parser);
    }
}
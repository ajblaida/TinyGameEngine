namespace TinyGameEngine.Core.Engine.Services;

using TinyGameEngine.Core.Engine.Interfaces;

public class GameIdProvider : IGameIdProvider
{
    public string GetGameId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 12).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
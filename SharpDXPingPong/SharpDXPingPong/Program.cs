namespace SharpDXPingPong
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var game = new PingPongGame("Ping Pong v.0.0.1");
            game.Run();
        }
    }
}

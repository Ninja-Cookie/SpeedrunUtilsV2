using System.Diagnostics;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpeedrunUtilsV2
{
    internal static class ConnectionManager
    {
        private const   int             BUFFER      = 1024;
        private const   string          TimeFormat  = @"hh\:mm\:ss\.fff";

        private static  NetworkStream   Stream;
        internal static bool            IsConnected => Stream != null;

        internal static bool StartingNewGame = false;

        internal static void SetStream(NetworkStream stream)
        {
            Stream = stream;
        }

        private static async Task<(int, byte[])> SendAndReceiveResponse(string data)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes($"{data}\r\n");
            if (!(await WriteToStream(commandBytes)))
                return default;

            var response = await ReadCurrentStream();
            return response;
        }

        private static async Task<bool> SendDataToStream(string data)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes($"{data}\r\n");
            return await WriteToStream(commandBytes);
        }

        private static async Task<bool> WriteToStream(byte[] bytes, bool force = false)
        {
            if (IsConnected)
            {
                await Stream.WriteAsync(bytes, 0, bytes.Length);
                await Stream.FlushAsync();
                return true;
            }
            return false;
        }

        private static async Task<(int, byte[])> ReadCurrentStream()
        {
            if (IsConnected)
            {
                var responseBuffer = new byte[BUFFER];
                return (await Stream.ReadAsync(responseBuffer, 0, responseBuffer.Length), responseBuffer);
            }
            return default;
        }

        internal static async void StartAddingCutsceneTime(TimeSpan time)
        {
            if (IsConnected)
                await AddCutsceneTime(time);
        }

        private static async Task AddCutsceneTime(TimeSpan time)
        {
            Stopwatch responseTimer = new Stopwatch();
            responseTimer.Start();

            var gameTimeResponse = await SendAndReceiveResponse("getcurrentgametime");

            responseTimer.Stop();

            if (gameTimeResponse == default)
                return;

            string response = Encoding.ASCII.GetString(gameTimeResponse.Item2, 0, gameTimeResponse.Item1).Trim();
            if (TimeSpan.TryParse(response, out TimeSpan currentGameTime))
            {
                TimeSpan newTime = (currentGameTime + time) - TimeSpan.FromMilliseconds(responseTimer.ElapsedMilliseconds);
                await SendDataToStream($"setgametime {newTime.ToString(TimeFormat)}");
            }
        }

        internal static async void StartPausingTimer()
        {
            if (IsConnected)
                await PauseTimer();
        }

        private static async Task PauseTimer()
        {
            await SendDataToStream("pausegametime");
        }

        internal static async void StartUnpausingTimer()
        {
            if (IsConnected)
            {
                await UnpauseTimer();

                if (StartingNewGame)
                {
                    StartingNewGame = false;
                    await StartNewGame();
                }
            }
        }

        private static async Task UnpauseTimer()
        {
            await SendDataToStream("unpausegametime");
        }

        private static async Task StartNewGame()
        {
            if (IsConnected)
                await NewGame();
        }

        private static async Task NewGame()
        {
            await SendDataToStream("reset");
            await SendDataToStream("starttimer");
        }

        internal static async void StartSplit(LiveSplitConfig.Splits split)
        {
            if (IsConnected)
                await Split(split);
        }

        private static async Task Split(LiveSplitConfig.Splits split)
        {
            await SendDataToStream("split");
        }
    }
}

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
        private const   string          TimeFormat  = @"hh\:mm\:ss\.fffffff";

        private static  NetworkStream   Stream;
        internal static bool            IsConnected => Stream != null && Plugin.liveSplitManager.ConnectionStatus == LiveSplitManager.Status.Connected;

        private enum Commands
        {
            GetCurrentGameTime,
            SetGameTime,
            PauseGameTime,
            UnpauseGameTime,
            Reset,
            StartTimer,
            Split
        }

        internal static bool StartingNewGame = false;

        internal static void SetStream(NetworkStream stream)
        {
            Stream = stream;
        }

        private static string GetCommand(Commands command, string parameter)
        {
            return parameter != null ? $"{command.ToString().ToLower()} {parameter}" : command.ToString().ToLower();
        }

        private static async Task<(int, byte[])> SendAndReceiveResponse(Commands command, string parameter = null)
        {
            string data = GetCommand(command, parameter);
            return await SendAndReceiveResponse(data);
        }

        private static async Task<bool> SendWithNoResponse(Commands command, string parameter = null)
        {
            string data = GetCommand(command, parameter);
            return await SendWithNoResponse(data);
        }

        private static async Task<(int, byte[])> SendAndReceiveResponse(string data)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes($"{data}\r\n");
            if (!(await WriteToStream(commandBytes)))
                return default;

            var response = await ReadCurrentStream();
            return response;
        }

        private static async Task<bool> SendWithNoResponse(string data)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes($"{data}\r\n");
            return await WriteToStream(commandBytes);
        }

        private static async Task<bool> WriteToStream(byte[] bytes, bool force = false)
        {
            if (IsConnected)
            {
                try
                {
                    await Stream.WriteAsync(bytes, 0, bytes.Length);
                    await Stream.FlushAsync();
                    return true;
                } catch { return false; }
            }
            return false;
        }

        private static async Task<(int, byte[])> ReadCurrentStream()
        {
            try
            {
                if (IsConnected && Stream.DataAvailable)
                {
                    var responseBuffer = new byte[BUFFER];
                    return (await Stream.ReadAsync(responseBuffer, 0, responseBuffer.Length), responseBuffer);
                }
            } catch { return default; }
            return default;
        }

        internal static async void StartAddingCutsceneTime(TimeSpan time)
        {
            if (IsConnected)
                await AddCutsceneTime(time);
        }

        private static async Task AddCutsceneTime(TimeSpan time)
        {
            Stopwatch responseTimer = Stopwatch.StartNew();
            var gameTimeResponse = await SendAndReceiveResponse(Commands.GetCurrentGameTime);
            responseTimer.Stop();

            if (gameTimeResponse == default)
                return;

            string response = Encoding.ASCII.GetString(gameTimeResponse.Item2, 0, gameTimeResponse.Item1).Trim();
            if (TimeSpan.TryParse(response, out TimeSpan currentGameTime))
            {
                TimeSpan newTime = (currentGameTime + time) - responseTimer.Elapsed;
                await SendWithNoResponse(Commands.SetGameTime, newTime.ToString(TimeFormat));
            }
        }

        internal static async void StartPausingTimer()
        {
            if (IsConnected)
                await PauseTimer();
        }

        private static async Task PauseTimer()
        {
            await SendWithNoResponse(Commands.PauseGameTime);
        }

        internal static async void StartUnpausingTimer()
        {
            // Putting this here just ensures StartingNewGame will always be set to false again if a connection failed.
            bool newGame    = StartingNewGame;
            StartingNewGame = false;

            if (IsConnected)
            {
                await UnpauseTimer();
                if (newGame)
                    await StartNewGame();
            }
        }

        private static async Task UnpauseTimer()
        {
            await SendWithNoResponse(Commands.UnpauseGameTime);
        }

        private static async Task StartNewGame()
        {
            if (IsConnected)
                await NewGame();
        }

        private static async Task NewGame()
        {
            await SendWithNoResponse(Commands.Reset);
            await SendWithNoResponse(Commands.StartTimer);
        }

        internal static async void StartSplit(LiveSplitConfig.Splits split)
        {
            if (IsConnected)
                await Split();
        }

        private static async Task Split()
        {
            await SendWithNoResponse(Commands.Split);
        }



        // Debug -----------------------------------------------------------------------------------------------

        internal static async Task<TimeSpan> StartGettingGameTime()
        {
            if (IsConnected)
                return await GetGameTimeFull();
            return default(TimeSpan);
        }

        private static async Task<TimeSpan> GetGameTimeFull()
        {
            var gameTime = await SendAndReceiveResponse(Commands.GetCurrentGameTime);
            if (gameTime == default)
                return default;

            string response = Encoding.ASCII.GetString(gameTime.Item2, 0, gameTime.Item1).Trim();
            if (TimeSpan.TryParse(response, out TimeSpan currentGameTime))
                return currentGameTime;
            return default(TimeSpan);
        }
    }
}

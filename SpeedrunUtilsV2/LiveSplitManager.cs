using Reptile;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedrunUtilsV2
{
    internal class LiveSplitManager
    {
        private const string    IP              = "127.0.0.1";
        private const int       PORT            = 16834;
        private const float     RefreshRate     = 2f;
        private NetworkStream   Stream;

        private CancellationTokenSource cancelRefresh;

        internal enum Status
        {
            Disconnected,
            Disconnecting,
            Connected,
            Connecting
        }

        private Status _connectionStatus = Status.Disconnected;
        internal Status ConnectionStatus
        {
            get => _connectionStatus;
            private set
            {
                if (value != _connectionStatus)
                {
                    switch (value)
                    {
                        case Status.Connected:
                            Patches.Patch_CutsceneSkip  .OnSkippedCutscene  += ConnectionManager.StartAddingCutsceneTime;
                            Patches.Patch_Loading       .OnEnteredLoading   += ConnectionManager.StartPausingTimer;
                            Patches.Patch_Loading       .OnExitedLoading    += ConnectionManager.StartUnpausingTimer;
                            StageManager                .OnStageInitialized += GameStatus.UpdateStage;
                            Patches.Patch_Objective     .OnObjectiveChanged += GameStatus.UpdateObjective;
                        break;

                        case Status.Disconnected:
                            Patches.Patch_CutsceneSkip  .OnSkippedCutscene  -= ConnectionManager.StartAddingCutsceneTime;
                            Patches.Patch_Loading       .OnEnteredLoading   -= ConnectionManager.StartPausingTimer;
                            Patches.Patch_Loading       .OnExitedLoading    -= ConnectionManager.StartUnpausingTimer;
                            StageManager                .OnStageInitialized -= GameStatus.UpdateStage;
                            Patches.Patch_Objective     .OnObjectiveChanged -= GameStatus.UpdateObjective;
                        break;
                    }
                }

                if (value == Status.Disconnected)
                    ConnectionManager.SetStream(null);
                _connectionStatus = value;
            }
        }

        internal LiveSplitManager()
        {
            LiveSplitConfig.ManageConfigFiles();
            ConnectToLiveSplit();
        }

        internal async void ConnectToLiveSplit()
        {
            if (ConnectionStatus != Status.Disconnected)
                return;

            ConnectionStatus = Status.Connecting;

            using (var client = new TcpClient())
            {
                if (TryConnectToLiveSplit(client))
                {
                    using (Stream = client.GetStream())
                    {
                        ConnectionManager.SetStream(Stream);
                        cancelRefresh = new CancellationTokenSource();
                        ConnectionStatus = Status.Connected;

                        UnityEngine.Debug.Log("Connection to LiveSplit was open!");

                        while (ConnectionStatus == Status.Connected)
                        {
                            try { await Task.Delay(TimeSpan.FromSeconds(RefreshRate), cancelRefresh.Token); } catch { break; }
                            if (ConnectionStatus != Status.Connected || !(await TryPingClientAsync(client)))
                                break;
                        }
                    }
                }
            }
            ConnectionStatus = Status.Disconnected;
            UnityEngine.Debug.LogWarning("Connection to LiveSplit was closed.");
        }

        internal void DisconnectFromLiveSplit()
        {
            if (ConnectionStatus == Status.Connected)
            {
                ConnectionStatus = Status.Disconnecting;
                cancelRefresh?.Cancel();
            }
        }

        private bool TryConnectToLiveSplit(TcpClient client)
        {
            try
            {
                client?.Connect(IP, PORT);
                return TryPingClient(client);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to connect to LiveSplit: {e}");
                return false;
            }
        }

        private async Task<bool> TryPingClientAsync(TcpClient client)
        {
            return await TryPingClient(client, true);
        }

        private bool TryPingClient(TcpClient client)
        {
            return TryPingClient(client, false).GetAwaiter().GetResult();
        }

        private async Task<bool> TryPingClient(TcpClient client, bool isAsync)
        {
            if (client == null || client.Client == null)
                return false;

            try
            {
                if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                {
                    byte[] buffer = new byte[1];
                    if (isAsync)
                        return await client.Client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.Peek) != 0;
                    else
                        return client.Client.Receive(buffer, SocketFlags.Peek) != 0;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

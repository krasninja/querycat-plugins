using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace QueryCat.Plugins.VStarCam.Domain;

/// <summary>
/// Find cameras in the current network.
/// </summary>
public sealed class CamerasFinder : IDisposable
{
    private const int OutboundPort = 8600;
    private const int InboundPort = 8601;
    private static readonly byte[] DiscoverMessage = { 0x44, 0x48, 0x01, 0x01 };

    /// <summary>
    /// The await time before receiving info about cameras.
    /// </summary>
    public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The number of requests to send to broadcast for available cameras.
    /// </summary>
    public int Retries { get; set; } = 2;

    /*
     * Command to watch traffic: sudo tcpdump -s0 -t udp -n 'port 8601' -X
     */

    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    /// <summary>
    /// Constructor.
    /// </summary>
    public CamerasFinder()
    {
        var endpoint = new IPEndPoint(IPAddress.Broadcast, InboundPort);
        _socket.EnableBroadcast = true;
        _socket.Bind(endpoint);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BroadcastResponse
    {
#pragma warning disable SA1307
        public uint unknown1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ip;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ipMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ipGateway;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string primaryDns;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string secondaryDns;

        public uint unknown2;

        public byte unknown3;

        public ushort port;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string uid;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 + (16 * 18) + 8)]
        public byte[] unknown4;
#pragma warning restore SA1307
    }

    /// <summary>
    /// The method tries to find cameras in the current network.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to monitor request cancellation.</param>
    /// <returns>Cameras.</returns>
    public async Task<IEnumerable<Camera>> FindAsync(CancellationToken cancellationToken)
    {
        var endpoint = new IPEndPoint(IPAddress.Broadcast, OutboundPort);
        await _socket.SendToAsync(DiscoverMessage, SocketFlags.None, endpoint);
        var camerasList = new Collection<Camera>();

        for (int i = 0; i < Retries; i++)
        {
            // Send broadcast request.
            var buffer = new byte[1024 * 10];
            CancellationTokenSource cts = new();
            var socketTask = _socket.ReceiveAsync(buffer, SocketFlags.None, cts.Token).AsTask();
            var maxWaitTask = Task.Delay(WaitTime, cts.Token);
            await Task.WhenAny(socketTask, maxWaitTask);
            if (!socketTask.IsCompleted)
            {
                cts.Cancel(throwOnFirstException: false);
                break;
            }

            // Analyze buffer and create camera entities.
            var bufferSize = socketTask.Result;
            var offset = 0;
            while (bufferSize > offset)
            {
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<BroadcastResponse>());
                Marshal.Copy(buffer, offset, ptr, Marshal.SizeOf<BroadcastResponse>());
                var camera = Marshal.PtrToStructure<BroadcastResponse>(ptr);
                AddToListUnique(camerasList, new Camera(camera.uid.Trim())
                {
                    Ip = camera.ip.Trim(),
                    IpGateway = camera.ipGateway.Trim(),
                    IpMask = camera.ipMask.Trim(),
                    PrimaryDns = camera.primaryDns.Trim(),
                    SecondaryDns = camera.secondaryDns.Trim(),
                    Port = camera.port,
                    Name = camera.name.Trim(),
                    FirmwareVersion = camera.version.Trim()
                });
                Marshal.FreeHGlobal(ptr);

                offset += Marshal.SizeOf<BroadcastResponse>();
            }
        }

        return camerasList;
    }

    private static void AddToListUnique(ICollection<Camera> cameras, Camera camera)
    {
        if (cameras.All(c => c.Id != camera.Id))
        {
            cameras.Add(camera);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _socket.Dispose();
    }
}

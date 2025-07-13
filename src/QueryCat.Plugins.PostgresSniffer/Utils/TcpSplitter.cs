using System.Collections.Concurrent;
using System.Net;
using PacketDotNet;
using QueryCat.Backend.Core.Utils;
using SharpPcap;

namespace QueryCat.Plugins.PostgresSniffer.Utils;

/// <summary>
/// TCP sessions splitter.
/// </summary>
/// <remarks>
/// https://github.com/dotpcap/sharppcap/blob/master/Examples/QueuingPacketsForBackgroundProcessing/Program.cs.
/// </remarks>
public class TcpSplitter : IDisposable
{
    public record struct Session(
        IPAddress SourceAddress,
        ushort SourcePort,
        IPAddress DestinationAddress,
        ushort DestinationPort);

    public sealed class SessionBuffer
    {
        private readonly Lock _objLock = new();
        private readonly DynamicBuffer<byte> _buffer = new();

        public Session Session { get; }

        /// <summary>
        /// Buffer size.
        /// </summary>
        public long Size
        {
            get
            {
                lock (_objLock)
                {
                    return _buffer.Size;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SessionBuffer(Session session)
        {
            Session = session;
        }

        /// <summary>
        /// Write data into the buffer.
        /// </summary>
        /// <param name="data">Data to write.</param>
        public void Write(ReadOnlySpan<byte> data)
        {
            lock (_objLock)
            {
                _buffer.Write(data);
            }
        }

        /// <summary>
        /// Attempt to copy exact buffer size items.
        /// </summary>
        /// <param name="buffer">Output buffer.</param>
        /// <param name="advance">Should advance dynamic buffer.</param>
        /// <returns><c>True</c> if all data was read, <c>false</c> otherwise.</returns>
        public bool TryCopyExact(Span<byte> buffer, bool advance = true)
        {
            lock (_objLock)
            {
                return _buffer.TryCopyExact(buffer, advance);
            }
        }

        /// <summary>
        /// Attempt to read exact buffer size items.
        /// </summary>
        /// <param name="count">Items to read.</param>
        /// <param name="buffer">Output buffer.</param>
        /// <param name="advance">Should advance dynamic buffer.</param>
        /// <returns><c>True</c> if all data was read, <c>false</c> otherwise.</returns>
        public bool TryReadExact(int count, out ReadOnlySpan<byte> buffer, bool advance = true)
        {
            lock (_objLock)
            {
                return _buffer.TryReadExact(count, out buffer, advance);
            }
        }

        /// <summary>
        /// Move the cursor by certain amount of elements.
        /// </summary>
        /// <param name="sizeToAdvance">Number of elements to move on.</param>
        public void Advance(long sizeToAdvance)
        {
            lock (_objLock)
            {
                _buffer.Advance(sizeToAdvance);
            }
        }

        /// <summary>
        /// Moves the cursor to the end of the sequence.
        /// </summary>
        public void AdvanceToEnd()
        {
            lock (_objLock)
            {
                _buffer.AdvanceToEnd();
            }
        }

        /// <summary>
        /// Get the first index any of specified delimiters.
        /// </summary>
        /// <param name="delimiters">The delimiters to look for.</param>
        /// <param name="foundDelimiter">Found delimiter.</param>
        /// <param name="skip">Start index to search from. Default is 0.</param>
        /// <returns>The delimiter index or -1 if not found.</returns>
        public int IndexOfAny(ReadOnlySpan<byte> delimiters, out byte foundDelimiter, int skip = 0)
        {
            lock (_objLock)
            {
                return _buffer.IndexOfAny(delimiters, out foundDelimiter, skip);
            }
        }
    }

    private readonly ConcurrentDictionary<Session, SessionBuffer> _sessions = new();
    private readonly ConcurrentQueue<TcpPacket> _queue = new();
    private readonly Thread _thread;
    private bool _threadShouldStop;

    public TcpSplitter()
    {
        _thread = new Thread(BackgroundThread)
        {
            IsBackground = true,
            Name = "TCPSplitterThread",
            Priority = ThreadPriority.BelowNormal,
        };
        _thread.Start();
    }

    public void ProcessTcpPacket(TcpPacket packet)
    {
        var ipPacket = (IPPacket)packet.ParentPacket;
        var session = new Session(
            ipPacket.SourceAddress,
            packet.SourcePort,
            ipPacket.DestinationAddress,
            packet.DestinationPort);
        var buffer = _sessions.GetOrAdd(session, _ => new SessionBuffer(session));

        var payload = GetPayloadFromTcpPacket(packet);
        if (payload.Length < 1)
        {
            return;
        }
        lock (buffer)
        {
            buffer.Write(payload);
        }
    }

    public void CapturePacket(PacketCapture capture)
    {
        var packet = capture.GetPacket();
        var parsedPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);
        var tcpPacket = parsedPacket.Extract<TcpPacket>();
        if (tcpPacket != null)
        {
            _queue.Enqueue(tcpPacket);
        }
    }

    public IEnumerable<SessionBuffer> GetSessionBuffers()
    {
        foreach (var session in _sessions)
        {
            yield return session.Value;
        }
    }

    private void BackgroundThread()
    {
        while (!_threadShouldStop)
        {
            Thread.Sleep(250);
            while (_queue.TryDequeue(out var packet))
            {
                if (_threadShouldStop)
                {
                    return;
                }
                ProcessTcpPacket(packet);
            }
        }
    }

    private static ReadOnlySpan<byte> GetPayloadFromTcpPacket(TcpPacket packet)
        => packet.PayloadDataSegment.Bytes.AsSpan(packet.PayloadDataSegment.Offset, packet.PayloadDataSegment.Length);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _threadShouldStop = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming
{
    public class ParallelMultiTypeStream
    {
        private SortedSet<short> forwardingExclusionList = new SortedSet<short>();
        public ParallelMultiTypeStream JoinedStream { get; private set; }
        private IEnumerable<ParallelMultiTypeStream> broadcastList;
        public IEnumerable<ParallelMultiTypeStream> BroadcastList
        {
            set
            {
                broadcastList = value;
            }
        }
        private short broadcastedType = -1;
        public bool BroadcastEverything { get; set; } = false;
        
        private ParallelBinaryStream binaryStream = new ParallelBinaryStream();
        private ParallelStringStream stringStream = new ParallelStringStream();
        private ParallelInternalMessageStream serverMessageStream = new ParallelInternalMessageStream();

        private StreamTypeNumberer identifier;
        private TypedStreamSerializer serializer;

        private List<VisitableDataProcessingStream> substreams = new List<VisitableDataProcessingStream>();
        private Dictionary<short, IDataProcessor> receivedDataProcessors = new Dictionary<short, IDataProcessor>();

        private Action<byte[]> onWrite;
        public event Action<byte[]> OnWrite
        {
            add => onWrite += value;
            remove => onWrite -= value;
        }
        internal void SetOnWriteToNull()
        {
            onWrite = null;
        }

        public ParallelMultiTypeStream() : this(new StreamTypeNumberer(), new TypedStreamSerializer())
        {
        }
        protected ParallelMultiTypeStream(StreamTypeNumberer identifier, TypedStreamSerializer serializer)
        {
            this.identifier = identifier;
            this.serializer = serializer;
            RegisterSubstream(binaryStream);
            RegisterSubstream(stringStream);
            RegisterSubstream(serverMessageStream);
            ExcludeFromForwarding<InternalMessage>();
        }
        protected void RegisterSubstream(VisitableDataProcessingStream substream)
        {
            substreams.Add(substream);
            receivedDataProcessors.Add(identifier.Visit(substream), substream);
        }
        protected ParallelStream<T> GetSubstream<T>()
        {
            foreach (var substream in substreams)
            {
                if (typeof(ParallelStream<T>).IsAssignableFrom(substream.GetType()))
                {
                    return (ParallelStream<T>)substream;
                }
            }
            return null;
        }
        public short GetTypeId<T>() => identifier.Visit(GetSubstream<T>());

        public void Open(Stream stream)
        {
            foreach (var substream in substreams)
            {
                substream.Open(stream);
            }
        }

        public void Join(ParallelMultiTypeStream stream)
        {
            JoinedStream = stream;
        }
        public void Disjoin()
        {
            JoinedStream = null;
        }

        public void SetBroadcastedType<T>() => SetBroadcastedType(GetTypeId<T>());
        public void SetBroadcastedType(short typeId)
        {
            CancelBroadcasting();
            broadcastedType = typeId;
        }
        public void CancelBroadcasting()
        {
            broadcastedType = -1;
            BroadcastEverything = false;
        }

        protected void ExcludeFromForwarding<TypeToExclude>() => forwardingExclusionList.Add(identifier.Visit(GetSubstream<TypeToExclude>()));

        protected void Write<T>(ParallelStream<T> stream, T data) => Write(identifier.Visit(stream), serializer.Visit(stream, data));
        protected void Write(short typeId, byte[] serializedData)
        {
            byte[] serializedId = BitConverter.GetBytes(typeId);
            byte[] identifiedSerializedData = new byte[2 + serializedData.Length];
            serializedId.CopyTo(identifiedSerializedData, 0);
            serializedData.CopyTo(identifiedSerializedData, 2);
            if (onWrite != null && !forwardingExclusionList.Contains(typeId) && typeId != broadcastedType)
            {
                onWrite(identifiedSerializedData);
            }
            else
            {
                binaryStream.Write(identifiedSerializedData);
            }
        }
        public void Write<T>(T data)
        {
            throw new ArgumentException($"Cannot write data of type {typeof(T).FullName} to {nameof(ParallelMultiTypeStream)}.");
        }
        public void Write(byte[] data) => Write(binaryStream, data);
        public void Write(string data) => Write(stringStream, data);
        public void Write(InternalMessage message) => Write(serverMessageStream, message);

        public void ReadNextData() => ProcessReadData(binaryStream.Read());
        public void ProcessReadData(byte[] readData)
        {
            if (readData == null || readData.Length <= 2)
            {
                return;
            }
            byte[] serializedId = new byte[2];
            for (int i = 0; i < serializedId.Length; i++)
            {
                serializedId[i] = readData[i];
            }
            short typeId = BitConverter.ToInt16(serializedId, 0);
            
            if (!forwardingExclusionList.Contains(typeId))//contains is O(ln(n))
            {
                if (JoinedStream != null)
                {
                    new Thread(() => JoinedStream.binaryStream.Write(readData)).Start();
                    return;
                }
                if (broadcastList != null && (BroadcastEverything || typeId == broadcastedType))
                {
                    foreach (var stream in broadcastList)
                    {
                        if (stream == JoinedStream)
                        {
                            continue;
                        }
                        new Thread(() => stream.binaryStream.Write(readData)).Start();
                    }
                    return;
                }
            }
            if (!receivedDataProcessors.TryGetValue(typeId, out IDataProcessor receivedDataProcessor))
            {
                return;
            }
            byte[] data = new byte[readData.Length - serializedId.Length];
            Array.Copy(readData, 2, data, 0, data.Length);
            receivedDataProcessor.Process(data);
        }
        public void AddReceivedDataHandler<T>(Action<T> handler) => GetSubstream<T>().OnDataReceived += handler;
        public void RemoveReceivedDataHandler<T>(Action<T> handler) => GetSubstream<T>().OnDataReceived -= handler;
    }
}

using Droid.Core;
using Droid.Sys;

namespace Droid.Framework.Async
{
    public class MsgQueue
    {
        public MsgQueue();

        public void Init(int sequence);

        public bool Add(byte[] data, int size);
        public bool Get(byte[] data, out int size);
        public int GetTotalSize();
        public int GetSpaceLeft();
        public int GetFirst => first;
        public int GetLast => last;
        public void CopyToBuffer(byte[] buf);

        byte[] buffer = new byte[MAX_MSG_QUEUE_SIZE];
        int first;          // sequence number of first message in queue
        int last;           // sequence number of last message in queue
        int startIndex;     // index pointing to the first byte of the first message
        int endIndex;       // index pointing to the first byte after the last message

        void WriteByte(byte b);
        byte ReadByte();
        void WriteShort(int s);
        int ReadShort();
        void WriteInt(int l);
        int ReadInt();
        void WriteData(byte[] data, int size);
        void ReadData(byte[] data, int size);
    }

    public class MsgChannel
    {
        public int MAX_MESSAGE_SIZE = 16384;      // max length of a message, which may
                                                  // be fragmented into multiple packets
        public int CONNECTIONLESS_MESSAGE_ID = -1;            // id for connectionless messages
        public int CONNECTIONLESS_MESSAGE_ID_MASK = 0x7FFF;       // value to mask away connectionless message id

        public int MAX_MSG_QUEUE_SIZE = 16384;      // must be a power of 2

        public MsgChannel();

        public void Init(Netadr adr, int id);
        public void Shutdown();
        public void ResetRate();

        // Gets or Sets the maximum outgoing rate.
        public int MaxOutgoingRate
        {
            get => maxRate;
            set => maxRate = value;
        }

        // Returns the address of the entity at the other side of the channel.
        public Netadr GetRemoteAddress() => remoteAddress;

        // Returns the average outgoing rate over the last second.
        public int GetOutgoingRate() => outgoingRateBytes;

        // Returns the average incoming rate over the last second.
        public int GetIncomingRate() => incomingRateBytes;

        // Returns the average outgoing compression ratio over the last second.
        public float GetOutgoingCompression() => outgoingCompression;

        // Returns the average incoming compression ratio over the last second.
        public float GetIncomingCompression() => incomingCompression;

        // Returns the average incoming packet loss over the last 5 seconds.
        public float GetIncomingPacketLoss();

        // Returns true if the channel is ready to send new data based on the maximum rate.
        public bool ReadyToSend(int time);

        // Sends an unreliable message, in order and without duplicates.
        public int SendMessage(Port port, int time, BitMsg msg);

        // Sends the next fragment if the last message was too large to send at once.
        public void SendNextFragment(Port port, int time);

        // Returns true if there are unsent fragments left.
        public bool UnsentFragmentsLeft() => unsentFragments;

        // Processes the incoming message. Returns true when a complete message is ready for further processing. In that case the read pointer of msg
        // points to the first byte ready for reading, and sequence is set to the sequence number of the message.
        public bool Process(Netadr from, int time, BitMsg msg, out int sequence);

        // Sends a reliable message, in order and without duplicates.
        public bool SendReliableMessage(BitMsg msg);

        // Returns true if a new reliable message is available and stores the message.
        public bool GetReliableMessage(BitMsg msg);

        // Removes any pending outgoing or incoming reliable messages.
        public void ClearReliableMessages();

        Netadr remoteAddress;   // address of remote host
        int id;             // our identification used instead of port number
        int maxRate;        // maximum number of bytes that may go out per second
        Compressor compressor;      // compressor used for data compression

        // variables to control the outgoing rate
        int lastSendTime;   // last time data was sent out
        int lastDataBytes;  // bytes left to send at last send time

        // variables to keep track of the rate
        int outgoingRateTime;
        int outgoingRateBytes;
        int incomingRateTime;
        int incomingRateBytes;

        // variables to keep track of the compression ratio
        float outgoingCompression;
        float incomingCompression;

        // variables to keep track of the incoming packet loss
        float incomingReceivedPackets;
        float incomingDroppedPackets;
        int incomingPacketLossTime;

        // sequencing variables
        int outgoingSequence;
        int incomingSequence;

        // outgoing fragment buffer
        bool unsentFragments;
        int unsentFragmentStart;
        byte[] unsentBuffer = new byte[MAX_MESSAGE_SIZE];
        BitMsg unsentMsg;

        // incoming fragment assembly buffer
        int fragmentSequence;
        int fragmentLength;
        byte[] fragmentBuffer = new byte[MAX_MESSAGE_SIZE];

        // reliable messages
        MsgQueue reliableSend;
        MsgQueue reliableReceive;

        void WriteMessageData(BitMsg o, BitMsg msg);
        bool ReadMessageData(BitMsg o, BitMsg msg);

        void UpdateOutgoingRate(int time, int size);
        void UpdateIncomingRate(int time, int size);

        void UpdatePacketLoss(int time, int numReceived, int numDropped);
    }
}

using System;
using System.Diagnostics;
using System.NumericsX.Core;
using static System.NumericsX.Lib;

namespace Gengine.Render
{
    public abstract class VCompressor : VFile
    {
        // compressor allocation
        public static VCompressor AllocNoCompression();
        public static VCompressor AllocBitStream();
        public static VCompressor AllocRunLength();
        public static VCompressor AllocRunLength_ZeroBased();
        public static VCompressor AllocHuffman();
        public static VCompressor AllocArithmetic();
        public static VCompressor AllocLZSS();
        public static VCompressor AllocLZSS_WordAligned();
        public static VCompressor AllocLZW();

        // initialization
        public abstract void Init(VFile f, bool compress, int wordLength);
        public abstract void FinishCompress();
        public abstract float CompressionRatio { get; }
    }

    class VCompressor_None : VCompressor
    {
        protected VFile file;
        protected bool compress;

        public VCompressor_None()
        {
            file = null;
            compress = true;
        }

        public override void Init(VFile f, bool compress, int wordLength)
        {
            this.file = f;
            this.compress = compress;
        }
        public override void FinishCompress() { }
        public override float CompressionRatio
            => 0f;
        public override string Name
            => file != null ? file.Name : string.Empty;
        public override string FullPath
            => file != null ? file.FullPath : string.Empty;
        public override int Read(byte[] outData, int outLength)
            => compress == true || outLength <= 0 ? 0 : file.Read(outData, outLength);
        public override int Write(byte[] inData, int inLength)
            => compress == false || inLength <= 0 ? 0 : file.Write(inData, inLength);
        public override int Length
            => file != null ? file.Length : 0;
        public override DateTime Timestamp
            => file != null ? file.Timestamp : DateTime.MinValue;
        public override int Tell
            => file != null ? file.Tell : 0;
        public override void ForceFlush()
           => file?.ForceFlush();
        public override void Flush()
            => file?.ForceFlush();
        public override int Seek(long offset, FS_SEEK origin)
        {
            common.Error("cannot seek on idCompressor");
            return -1;
        }
    }

    /// <summary>
    /// Base class for bit stream compression.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    class VCompressor_BitStream : VCompressor_None
    {
        public override void Init(VFile f, bool compress, int wordLength)
        {
            Debug.Assert(wordLength >= 1 && wordLength <= 32);

            this.file = f;
            this.compress = compress;
            this.wordLength = wordLength;

            readTotalBytes = 0;
            readLength = 0;
            readByte = 0;
            readBit = 0;
            readData = null;

            writeTotalBytes = 0;
            writeLength = 0;
            writeByte = 0;
            writeBit = 0;
            writeData = null;
        }
        public override void FinishCompress()
        {
            if (!compress)
                return;

            if (writeByte == 0)
                file.Write(buffer, writeByte);
            writeLength = 0;
            writeByte = 0;
            writeBit = 0;
        }

        public override float CompressionRatio
            => compress
                ? (readTotalBytes - writeTotalBytes) * 100.0f / readTotalBytes
                : (writeTotalBytes - readTotalBytes) * 100.0f / writeTotalBytes;

        public override int Write(byte[] inData, int inLength)
        {
            if (compress == false || inLength <= 0)
                return 0;

            InitCompress(inData, inLength);

            int i;
            for (i = 0; i < inLength; i++)
                WriteBits(ReadBits(8), 8);
            return i;
        }
        public override int Read(byte[] outData, int outLength)
        {
            if (compress == true || outLength <= 0)
                return 0;

            InitDecompress(outData, outLength);

            int i;
            for (i = 0; i < outLength && readLength >= 0; i++)
                WriteBits(ReadBits(8), 8);
            return i;
        }

        protected byte[] buffer = new byte[65536];
        protected int wordLength;

        protected int readTotalBytes;
        protected int readLength;
        protected int readByte;
        protected int readBit;
        protected byte[] readData;

        protected int writeTotalBytes;
        protected int writeLength;
        protected int writeByte;
        protected int writeBit;
        protected byte[] writeData;

        protected void InitCompress(byte[] inData, int inLength)
        {
            readLength = inLength;
            readByte = 0;
            readBit = 0;
            readData = inData;

            if (writeLength == 0)
            {
                writeLength = buffer.Length;
                writeByte = 0;
                writeBit = 0;
                writeData = buffer;
            }
        }
        protected void InitDecompress(byte[] outData, int outLength)
        {
            if (readLength == 0)
            {
                readLength = file.Read(buffer, buffer.Length);
                readByte = 0;
                readBit = 0;
                readData = buffer;
            }

            writeLength = outLength;
            writeByte = 0;
            writeBit = 0;
            writeData = outData;
        }
        protected void WriteBits(int value, int numBits)
        {
            int put, fraction;

            // Short circuit for writing single bytes at a time
            if (writeBit == 0 && numBits == 8 && writeByte < writeLength)
            {
                writeByte++;
                writeTotalBytes++;
                writeData[writeByte - 1] = (byte)value;
                return;
            }

            while (numBits != 0)
            {
                if (writeBit == 0)
                {
                    if (writeByte >= writeLength)
                        if (writeData == buffer)
                        {
                            file.Write(buffer, writeByte);
                            writeByte = 0;
                        }
                        else
                        {
                            put = numBits;
                            writeBit = put & 7;
                            writeByte += (put >> 3) + (writeBit != 0 ? 1 : 0);
                            writeTotalBytes += (put >> 3) + (writeBit != 0 ? 1 : 0);
                            return;
                        }
                    writeData[writeByte] = 0;
                    writeByte++;
                    writeTotalBytes++;
                }
                put = 8 - writeBit;
                if (put > numBits)
                    put = numBits;
                fraction = value & ((1 << put) - 1);
                writeData[writeByte - 1] |= (byte)(fraction << writeBit);
                numBits -= put;
                value >>= put;
                writeBit = (writeBit + put) & 7;
            }
        }

        protected int ReadBits(int numBits)
        {
            int value, valueBits, get, fraction;

            value = 0;
            valueBits = 0;

            // Short circuit for reading single bytes at a time
            if (readBit == 0 && numBits == 8 && readByte < readLength)
            {
                readByte++;
                readTotalBytes++;
                return readData[readByte - 1];
            }

            while (valueBits < numBits)
            {
                if (readBit == 0)
                {
                    if (readByte >= readLength)
                        if (readData == buffer)
                        {
                            readLength = file.Read(buffer, buffer.Length);
                            readByte = 0;
                        }
                        else
                        {
                            get = numBits - valueBits;
                            readBit = get & 7;
                            readByte += (get >> 3) + (readBit != 0 ? 1 : 0);
                            readTotalBytes += (get >> 3) + (readBit != 0 ? 1 : 0);
                            return value;
                        }
                    readByte++;
                    readTotalBytes++;
                }
                get = 8 - readBit;
                if (get > (numBits - valueBits))
                    get = (numBits - valueBits);
                fraction = readData[readByte - 1];
                fraction >>= readBit;
                fraction &= (1 << get) - 1;
                value |= fraction << valueBits;
                valueBits += get;
                readBit = (readBit + get) & 7;
            }

            return value;
        }

        protected void UnreadBits(int numBits)
        {
            readByte -= (numBits >> 3);
            readTotalBytes -= (numBits >> 3);
            if (readBit == 0)
                readBit = 8 - (numBits & 7);
            else
            {
                readBit -= numBits & 7;
                if (readBit <= 0)
                {
                    readByte--;
                    readTotalBytes--;
                    readBit = (readBit + 8) & 7;
                }
            }
            if (readByte < 0)
            {
                readByte = 0;
                readBit = 0;
            }
        }

        protected static unsafe int Compare(byte[] src1, int bitPtr1, byte[] src2, int bitPtr2, int maxBits)
        {
            int i;

            // If the two bit pointers are aligned then we can use a faster comparison
            if ((bitPtr1 & 7) == (bitPtr2 & 7) && maxBits > 16)
            {
                fixed (byte* src1_ = &src1[bitPtr1 >> 3], src2_ = &src2[bitPtr2 >> 3])
                {
                    byte* p1 = src1_;
                    byte* p2 = src2_;

                    int bits = 0, bitsRemain = maxBits;

                    // Compare the first couple bits (if any)
                    if ((bitPtr1 & 7) != 0)
                    {
                        for (i = (bitPtr1 & 7); i < 8; i++, bits++)
                        {
                            if ((((*p1 >> i) ^ (*p2 >> i)) & 1) != 0)
                                return bits;
                            bitsRemain--;
                        }
                        p1++;
                        p2++;
                    }

                    var remain = bitsRemain >> 3;

                    // Compare the middle bytes as ints
                    while (remain >= 4 && (*(int*)p1 == *(int*)p2))
                    {
                        p1 += 4;
                        p2 += 4;
                        remain -= 4;
                        bits += 32;
                    }

                    // Compare the remaining bytes
                    while (remain > 0 && (*p1 == *p2))
                    {
                        p1++;
                        p2++;
                        remain--;
                        bits += 8;
                    }

                    // Compare the last couple of bits (if any)
                    var finalBits = 8;
                    if (remain == 0)
                        finalBits = (bitsRemain & 7);
                    for (i = 0; i < finalBits; i++, bits++)
                        if ((((*p1 >> i) ^ (*p2 >> i)) & 1) != 0)
                            return bits;

                    Debug.Assert(bits == maxBits);
                    return bits;
                }
            }
            else
            {
                for (i = 0; i < maxBits; i++)
                {
                    if ((((src1[bitPtr1 >> 3] >> (bitPtr1 & 7)) ^ (src2[bitPtr2 >> 3] >> (bitPtr2 & 7))) & 1) != 0)
                        break;
                    bitPtr1++;
                    bitPtr2++;
                }
                return i;
            }
        }
    }

    /// <summary>
    /// The following algorithm implements run length compression with an arbitrary word size.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_BitStream" />
    class VCompressor_RunLength : VCompressor_BitStream
    {
        int runLengthCode;

        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);
            runLengthCode = (1 << wordLength) - 1;
        }

        public override int Write(byte[] inData, int inLength)
        {
            int bits, nextBits, count;

            if (compress == false || inLength <= 0)
                return 0;

            InitCompress(inData, inLength);

            while (readByte <= readLength)
            {
                count = 1;
                bits = ReadBits(wordLength);
                for (nextBits = ReadBits(wordLength); nextBits == bits; nextBits = ReadBits(wordLength))
                {
                    count++;
                    if (count >= (1 << wordLength))
                        if (count >= (1 << wordLength) + 3 || bits == runLengthCode)
                            break;
                }
                if (nextBits != bits)
                    UnreadBits(wordLength);
                if (count > 3 || bits == runLengthCode)
                {
                    WriteBits(runLengthCode, wordLength);
                    WriteBits(bits, wordLength);
                    if (bits != runLengthCode)
                        count -= 3;
                    WriteBits(count - 1, wordLength);
                }
                else
                    while (count-- != 0)
                        WriteBits(bits, wordLength);
            }

            return inLength;
        }

        public override int Read(byte[] outData, int outLength)
        {
            int bits, count;

            if (compress == true || outLength <= 0)
                return 0;

            InitDecompress(outData, outLength);

            while (writeByte <= writeLength && readLength >= 0)
            {
                bits = ReadBits(wordLength);
                if (bits == runLengthCode)
                {
                    bits = ReadBits(wordLength);
                    count = ReadBits(wordLength) + 1;
                    if (bits != runLengthCode)
                        count += 3;
                    while (count-- != 0)
                        WriteBits(bits, wordLength);
                }
                else
                    WriteBits(bits, wordLength);
            }

            return writeByte;
        }
    }

    /// <summary>
    /// The following algorithm implements run length compression with an arbitrary word size for data with a lot of zero bits.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_BitStream" />
    class VCompressor_RunLength_ZeroBased : VCompressor_BitStream
    {
        public override int Write(byte[] inData, int inLength)
        {
            int bits, count;

            if (compress == false || inLength <= 0)
                return 0;

            InitCompress(inData, inLength);

            while (readByte <= readLength)
            {
                count = 0;
                for (bits = ReadBits(wordLength); bits == 0 && count < (1 << wordLength); bits = ReadBits(wordLength))
                    count++;
                if (count != 0)
                {
                    WriteBits(0, wordLength);
                    WriteBits(count - 1, wordLength);
                    UnreadBits(wordLength);
                }
                else
                    WriteBits(bits, wordLength);
            }

            return inLength;
        }

        public override int Read(byte[] outData, int outLength)
        {
            int bits, count;

            if (compress == true || outLength <= 0)
                return 0;

            InitDecompress(outData, outLength);

            while (writeByte <= writeLength && readLength >= 0)
            {
                bits = ReadBits(wordLength);
                if (bits == 0)
                {
                    count = ReadBits(wordLength) + 1;
                    while (count-- > 0)
                        WriteBits(0, wordLength);
                }
                else
                    WriteBits(bits, wordLength);
            }

            return writeByte;
        }
    }

    /// <summary>
    /// The following algorithm is based on the adaptive Huffman algorithm described in Sayood's Data Compression book. The ranks are not actually stored, but
	/// implicitly defined by the location of a node within a doubly-linked list
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    class VCompressor_Huffman : VCompressor_None
    {
        const int HMAX = 256;               // Maximum symbol
        const int NYT = HMAX;               // NYT = Not Yet Transmitted
        const int INTERNAL_NODE = HMAX + 1;         // internal node

        class HuffmanNode
        {
            public HuffmanNode left, right, parent; // tree structure
            public HuffmanNode next, prev;         // doubly-linked list
            public HuffmanNode head;                   // highest ranked node in block
            public int weight;
            public int symbol;
        }

        byte[] seq = new byte[65536];
        int bloc;
        int blocMax;
        int blocIn;
        int blocNode;
        int blocPtrs;

        int compressedSize;
        int unCompressedSize;

        HuffmanNode tree;
        HuffmanNode lhead;
        HuffmanNode ltail;
        HuffmanNode[] loc = new HuffmanNode[HMAX + 1];
        HuffmanNode[] freelist;

        HuffmanNode[] nodeList = new HuffmanNode[768];
        HuffmanNode[] nodePtrs = new HuffmanNode[768];

        public override void Init(VFile f, bool compress, int wordLength)
        {
            int i;

            this.file = f;
            this.compress = compress;
            bloc = 0;
            blocMax = 0;
            blocIn = 0;
            blocNode = 0;
            blocPtrs = 0;
            compressedSize = 0;
            unCompressedSize = 0;

            tree = null;
            lhead = null;
            ltail = null;
            for (i = 0; i < (HMAX + 1); i++)
                loc[i] = null;
            freelist = null;

            for (i = 0; i < 768; i++)
            {
                nodeList[i] = new HuffmanNode();
                nodePtrs[i] = null;
            }

            if (compress)
            {
                // Add the NYT (not yet transmitted) node into the tree/list
                tree = lhead = loc[NYT] = nodeList[blocNode++];
                tree.symbol = NYT;
                tree.weight = 0;
                lhead.next = lhead.prev = null;
                tree.parent = tree.left = tree.right = null;
                loc[NYT] = tree;
            }
            else
            {
                // Initialize the tree & list with the NYT node
                tree = lhead = ltail = loc[NYT] = nodeList[blocNode++];
                tree.symbol = NYT;
                tree.weight = 0;
                lhead.next = lhead.prev = null;
                tree.parent = tree.left = tree.right = null;
            }
        }

        void PutBit(int bit, byte[] fout, ref int offset)
        {
            bloc = offset;
            if ((bloc & 7) == 0)
                fout[bloc >> 3] = 0;
            fout[bloc >> 3] |= (byte)(bit << (bloc & 7));
            bloc++;
            offset = bloc;
        }

        int GetBit(byte[] fout, ref int offset)
        {
            bloc = offset;
            var t = (fout[bloc >> 3] >> (bloc & 7)) & 0x1;
            bloc++;
            offset = bloc;
            return t;
        }

        // Add a bit to the output file (buffered)
        void Add_bit(char bit, byte[] fout)
        {
            if ((bloc & 7) == 0)
                fout[(bloc >> 3)] = 0;
            fout[(bloc >> 3)] |= (byte)(bit << (bloc & 7));
            bloc++;
        }

        // Get one bit from the input file (buffered)
        int Get_bit()
        {
            int t, wh = bloc >> 3, whb = wh >> 16;
            if (whb != blocIn)
            {
                blocMax += file.Read(seq, seq.Length);
                blocIn++;
            }
            wh &= 0xffff;
            t = (seq[wh] >> (bloc & 7)) & 0x1;
            bloc++;
            return t;
        }

        HuffmanNode[] Get_ppnode()
        {
            HuffmanNode* tppnode;
            if (freelist == null)
            {
                return nodePtrs[blocPtrs++];
            }
            else
            {
                tppnode = freelist;
                freelist = (HuffmanNode*)*tppnode;
                return tppnode;
            }
        }

        void Free_ppnode(ref HuffmanNode ppnode)
        {
            ppnode = (HuffmanNode)freelist;
            freelist = ppnode;
        }

        // Swap the location of the given two nodes in the tree.
        void Swap(HuffmanNode node1, HuffmanNode node2)
        {
            HuffmanNode par1, par2;

            par1 = node1.parent;
            par2 = node2.parent;

            if (par1 != null)
            {
                if (par1.left == node1) par1.left = node2;
                else par1.right = node2;
            }
            else tree = node2;

            if (par2 != null)
            {
                if (par2.left == node2) par2.left = node1;
                else par2.right = node1;
            }
            else tree = node1;

            node1.parent = par2;
            node2.parent = par1;
        }

        // Swap the given two nodes in the linked list (update ranks)
        void Swaplist(HuffmanNode node1, HuffmanNode node2)
        {
            HuffmanNode par1;

            par1 = node1.next; node1.next = node2.next; node2.next = par1;
            par1 = node1.prev; node1.prev = node2.prev; node2.prev = par1;

            if (node1.next == node1) node1.next = node2;
            if (node2.next == node2) node2.next = node1;
            if (node1.next != null) node1.next.prev = node1;
            if (node2.next != null) node2.next.prev = node2;
            if (node1.prev != null) node1.prev.next = node1;
            if (node2.prev != null) node2.prev.next = node2;
        }

        void Increment(HuffmanNode node)
        {
            HuffmanNode lnode;

            if (node == null)
                return;

            if (node.next != null && node.next.weight == node.weight)
            {
                lnode = node.head;
                if (lnode != node.parent)
                    Swap(lnode, node);
                Swaplist(lnode, node);
            }
            if (node.prev != null && node.prev.weight == node.weight)
                node.head = node.prev;
            else
            {
                node.head = null;
                Free_ppnode(ref node.head);
            }
            node.weight++;
            if (node.next != null && node.next.weight == node.weight)
                node.head = node.next.head;
            else
            {
                node.head = Get_ppnode();
                node.head = node;
            }
            if (node.parent != null)
            {
                Increment(node.parent);
                if (node.prev == node.parent)
                {
                    Swaplist(node, node.parent);
                    if (node.head == node)
                        node.head = node.parent;
                }
            }
        }

        void AddRef(byte ch)
        {
            HuffmanNode tnode, tnode2;
            if (loc[ch] == null)
            { // if this is the first transmission of this node
                tnode = nodeList[blocNode++];
                tnode2 = nodeList[blocNode++];

                tnode2.symbol = INTERNAL_NODE;
                tnode2.weight = 1;
                tnode2.next = lhead.next;
                if (lhead.next != null)
                {
                    lhead.next.prev = tnode2;
                    if (lhead.next.weight == 1)
                        tnode2.head = lhead.next.head;
                    else
                    {
                        tnode2.head = Get_ppnode();
                        tnode2.head = tnode2;
                    }
                }
                else
                {
                    tnode2.head = Get_ppnode();
                    tnode2.head = tnode2;
                }
                lhead.next = tnode2;
                tnode2.prev = lhead;

                tnode.symbol = ch;
                tnode.weight = 1;
                tnode.next = lhead.next;
                if (lhead.next != null)
                {
                    lhead.next.prev = tnode;
                    if (lhead.next.weight == 1)
                        tnode.head = lhead.next.head;
                    else
                    {
                        // this should never happen
                        tnode.head = Get_ppnode();
                        tnode.head = tnode2;
                    }
                }
                else
                {
                    // this should never happen
                    tnode.head = Get_ppnode();
                    tnode.head = tnode;
                }
                lhead.next = tnode;
                tnode.prev = lhead;
                tnode.left = tnode.right = null;

                if (lhead.parent != null)
                {
                    if (lhead.parent.left == lhead) lhead.parent.left = tnode2; // lhead is guaranteed to by the NYT
                    else lhead.parent.right = tnode2;
                }
                else tree = tnode2;

                tnode2.right = tnode;
                tnode2.left = lhead;

                tnode2.parent = lhead.parent;
                lhead.parent = tnode.parent = tnode2;

                loc[ch] = tnode;

                Increment(tnode2.parent);
            }
            else
                Increment(loc[ch]);
        }

        // Get a symbol.
        int Receive(HuffmanNode node, ref int ch)
        {
            while (node != null && node.symbol == INTERNAL_NODE)
                node = Get_bit() != 0 ? node.right : node.left;
            return node == null ? 0 : (ch = node.symbol);
        }

        // Send the prefix code for this node.
        void Send(HuffmanNode node, HuffmanNode child, byte[] fout)
        {
            if (node.parent != null)
                Send(node.parent, node, fout);
            if (child != null)
                Add_bit((char)(node.right == child ? 1 : 0), fout);
        }

        // Send a symbol.
        void Transmit(int ch, byte[] fout)
        {
            if (loc[ch] == null)
            {
                // HuffmanNode hasn't been transmitted, send a NYT, then the symbol
                Transmit(NYT, fout);
                for (var i = 7; i >= 0; i--)
                    Add_bit((char)((ch >> i) & 0x1), fout);
            }
            else Send(loc[ch], null, fout);
        }

        public override int Write(byte[] inData, int inLength)
        {
            int i, ch;

            if (!compress || inLength <= 0)
                return 0;

            for (i = 0; i < inLength; i++)
            {
                ch = inData[i];
                Transmit(ch, seq);              // Transmit symbol
                AddRef((byte)ch);               // Do update
                var b = bloc >> 3;
                if (b > 32768)
                {
                    file.Write(seq, b);
                    seq[0] = seq[b];
                    bloc &= 7;
                    compressedSize += b;
                }
            }

            unCompressedSize += i;
            return i;
        }

        public override void FinishCompress()
        {
            if (!compress)
                return;

            bloc += 7;
            var str = (bloc >> 3);
            if (str != 0)
            {
                file.Write(seq, str);
                compressedSize += str;
            }
        }

        public override int Read(byte[] outData, int outLength)
        {
            int i, j, ch;

            if (compress || outLength <= 0)
                return 0;

            if (bloc == 0)
            {
                blocMax = file.Read(seq, seq.Length);
                blocIn = 0;
            }

            for (i = 0; i < outLength; i++)
            {
                ch = 0;
                // don't overflow reading from the file
                if ((bloc >> 3) > blocMax)
                    break;
                Receive(tree, ref ch);      // Get a character
                if (ch == NYT)
                {                           // We got a NYT, get the symbol associated with it
                    ch = 0;
                    for (j = 0; j < 8; j++)
                        ch = (ch << 1) + Get_bit();
                }

                outData[i] = (byte)ch;          // Write symbol
                AddRef((byte)ch);               // Increment node
            }

            compressedSize = bloc >> 3;
            unCompressedSize += i;
            return i;
        }

        public override float CompressionRatio
            => (unCompressedSize - compressedSize) * 100f / unCompressedSize;
    }
}
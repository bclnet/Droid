#define DYNAMIC_BLOCK_ALLOC_CHECK

using System.Diagnostics;

namespace System.NumericsX.Core
{
    public class DynamicBlock<T>
    {
        public T[] Memory => (T[])(((byte*)this) + sizeof(DynamicBlock<T>));
        public int Size => Math.Abs(size);
        public void SetSize(int s, bool isBaseBlock) => size = isBaseBlock ? -s : s;
        public bool IsBaseBlock => size < 0;

#if DYNAMIC_BLOCK_ALLOC_CHECK
        public int[] id = new int[3];
        public object allocator;
#endif

        public int size;                   // size in bytes of the block
        public DynamicBlock<T> prev;                   // previous memory block
        public DynamicBlock<T> next;                   // next memory block
        public BTreeNode<DynamicBlock<T>, int> node;			// node in the B-Tree with free blocks
    }

    public class DynamicBlockAlloc<T>
    {
        int baseBlockSize;
        int minBlockSize;

        DynamicBlock<T> firstBlock;               // first block in list in order of increasing address
        DynamicBlock<T> lastBlock;                // last block in list in order of increasing address
        BTree<DynamicBlock<T>, int> freeTree = new(4);          // B-Tree with free memory blocks
        bool allowAllocs;           // allow base block allocations
        bool lockMemory;                // lock memory so it cannot get swapped out

#if DYNAMIC_BLOCK_ALLOC_CHECK
        int[] blockId = new int[3];
#endif

        int numBaseBlocks;          // number of base blocks
        int baseBlockMemory;        // total memory in base blocks
        int numUsedBlocks;          // number of used blocks
        int usedBlockMemory;        // total memory in used blocks
        int numFreeBlocks;          // number of free blocks
        int freeBlockMemory;        // total memory in free blocks

        int numAllocs;
        int numResizes;
        int numFrees;

        public DynamicBlockAlloc(int baseBlockSize, int minBlockSize)
        {
            this.baseBlockSize = baseBlockSize;
            this.minBlockSize = minBlockSize;
        }

        public DynamicBlockAlloc() => Clear();
        public void Dispose() => Shutdown();
        public void Init() => freeTree.Init();

        public void Shutdown()
        {
            DynamicBlock<T> block;

            for (block = firstBlock; block != null; block = block.next)
                if (block.node == null)
                    FreeInternal(block);

            for (block = firstBlock; block != null; block = firstBlock)
            {
                firstBlock = block.next;
                Debug.Assert(block.IsBaseBlock);
                if (lockMemory)
                    sys.UnlockMemory(block, block.Size + sizeof(DynamicBlock<T>));
                Mem_Free16(block);
            }

            freeTree.Shutdown();

            Clear();
        }

        public void SetFixedBlocks(int numBlocks)
        {
            DynamicBlock<T> block;

            for (int i = numBaseBlocks; i < numBlocks; i++)
            {
                block = (DynamicBlock<T>)Mem_Alloc16(baseBlockSize);
                if (lockMemory)
                    sys.LockMemory(block, baseBlockSize);
#if DYNAMIC_BLOCK_ALLOC_CHECK
                memcpy(block.id, blockId, sizeof(block.id));
                block.allocator = this;
#endif
                block.SetSize(baseBlockSize - (int)sizeof(DynamicBlock<T>), true);
                block.next = null;
                block.prev = lastBlock;
                if (lastBlock != null) lastBlock.next = block;
                else firstBlock = block;
                lastBlock = block;
                block.node = null;

                FreeInternal(block);

                numBaseBlocks++;
                baseBlockMemory += baseBlockSize;
            }

            allowAllocs = false;
        }

        public void SetLockMemory(bool @lock)
            => lockMemory = @lock;

        public void FreeEmptyBaseBlocks()
        {
            DynamicBlock<T> block, next;

            for (block = firstBlock; block != null; block = next)
            {
                next = block.next;

                if (block.IsBaseBlock && block.node != null && (next == null || next.IsBaseBlock))
                {
                    UnlinkFreeInternal(block);
                    if (block.prev != null) block.prev.next = block.next;
                    else firstBlock = block.next;
                    if (block.next != null) block.next.prev = block.prev;
                    else lastBlock = block.prev;
                    if (lockMemory)
                        sys.UnlockMemory(block, block.Size + sizeof(DynamicBlock<T>));
                    numBaseBlocks--;
                    baseBlockMemory -= block.Size + sizeof(DynamicBlock<T>);
                    Mem_Free16(block);
                }
            }

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif
        }

        public T[] Alloc(int num)
        {
            DynamicBlock<T> block;

            numAllocs++;

            if (num <= 0)
                return null;

            block = AllocInternal(num);
            if (block == null)
                return null;
            block = ResizeInternal(block, num);
            if (block == null)
                return null;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif

            numUsedBlocks++;
            usedBlockMemory += block.Size;

            return block.Memory;
        }

        public T[] Resize(ref T[] ptr, int num)
        {
            numResizes++;

            if (ptr == null)
                return Alloc(num);

            if (num <= 0)
            {
                Free(ref ptr);
                return null;
            }

            var block = (DynamicBlock<T>)(((byte*)ptr) - sizeof(DynamicBlock<T>));

            usedBlockMemory -= block.Size;

            block = ResizeInternal(block, num);
            if (block == null)
                return null;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif

            usedBlockMemory += block.Size;

            return block.Memory;
        }
        public void Free(ref T[] ptr)
        {
            numFrees++;
            if (ptr == null)
                return;

            var block = (DynamicBlock<T>)(((byte*)ptr) - sizeof(DynamicBlock<T>));

            numUsedBlocks--;
            usedBlockMemory -= block.Size;

            FreeInternal(block);

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif
        }
        public string CheckMemory(T[] ptr)
        {
            DynamicBlock<T> block;

            if (ptr == null)
                return null;

            block = (DynamicBlock<T>)(((byte*)ptr) - sizeof(DynamicBlock<T>));

            if (block.node != null)
                return "memory has been freed";

#if DYNAMIC_BLOCK_ALLOC_CHECK
            if (block.id[0] != 0x11111111 || block.id[1] != 0x22222222 || block.id[2] != 0x33333333)
                return "memory has invalid id";
            if (block.allocator != this)
                return "memory was allocated with different allocator";
#endif
            return null;
        }

        public int NumBaseBlocks => numBaseBlocks;
        public int BaseBlockMemory => baseBlockMemory;
        public int NumUsedBlocks => numUsedBlocks;
        public int UsedBlockMemory => usedBlockMemory;
        public int NumFreeBlocks => numFreeBlocks;
        public int FreeBlockMemory => freeBlockMemory;
        public int NumEmptyBaseBlocks
        {
            get
            {
                var numEmptyBaseBlocks = 0;
                for (var block = firstBlock; block != null; block = block.next)
                    if (block.IsBaseBlock && block.node != null && (block.next == null || block.next.IsBaseBlock))
                        numEmptyBaseBlocks++;
                return numEmptyBaseBlocks;
            }
        }

        void Clear()
        {
            firstBlock = lastBlock = null;
            allowAllocs = true;
            lockMemory = false;
            numBaseBlocks = 0;
            baseBlockMemory = 0;
            numUsedBlocks = 0;
            usedBlockMemory = 0;
            numFreeBlocks = 0;
            freeBlockMemory = 0;
            numAllocs = 0;
            numResizes = 0;
            numFrees = 0;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            blockId[0] = 0x11111111;
            blockId[1] = 0x22222222;
            blockId[2] = 0x33333333;
#endif
        }
        DynamicBlock<T> AllocInternal(int num)
        {
            var alignedBytes = (num * sizeof(T) + 15) & ~15;

            var block = freeTree.FindSmallestLargerEqual(alignedBytes);
            if (block != null)
                UnlinkFreeInternal(block);
            else if (allowAllocs)
            {
                var allocSize = Math.Max(baseBlockSize, alignedBytes + (int)sizeof(DynamicBlock<T>));
                block = (DynamicBlock<T>)Mem_Alloc16(allocSize);
                if (lockMemory)
                    sys.LockMemory(block, baseBlockSize);
#if DYNAMIC_BLOCK_ALLOC_CHECK
                memcpy(block.id, blockId, sizeof(block.id));
                block.allocator = this;
#endif
                block.SetSize(allocSize - sizeof(DynamicBlock<T>), true);
                block.next = null;
                block.prev = lastBlock;
                if (lastBlock != null) lastBlock.next = block;
                else firstBlock = block;
                lastBlock = block;
                block.node = null;

                numBaseBlocks++;
                baseBlockMemory += allocSize;
            }

            return block;
        }
        DynamicBlock<T> ResizeInternal(DynamicBlock<T> block, int num)
        {
            var alignedBytes = (num * sizeof(T) + 15) & ~15;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            Debug.Assert(block.id[0] == 0x11111111 && block.id[1] == 0x22222222 && block.id[2] == 0x33333333 && block.allocator == this);
#endif

            // if the new size is larger
            if (alignedBytes > block.Size)
            {
                var nextBlock = block.next;

                // try to annexate the next block if it's free
                if (nextBlock != null && !nextBlock.IsBaseBlock && nextBlock.node != null && block.Size + (int)sizeof(DynamicBlock<T>) + nextBlock.Size >= alignedBytes)
                {
                    UnlinkFreeInternal(nextBlock);
                    block.SetSize(block.Size + sizeof(DynamicBlock<T>) + nextBlock.Size, block.IsBaseBlock);
                    block.next = nextBlock.next;
                    if (nextBlock.next != null) nextBlock.next.prev = block;
                    else lastBlock = block;
                }
                else
                {
                    // allocate a new block and copy
                    DynamicBlock<T> oldBlock = block;
                    block = AllocInternal(num);
                    if (block == null)
                        return null;
                    memcpy(block.Memory, oldBlock.Memory, oldBlock.Size);
                    FreeInternal(oldBlock);
                }
            }

            // if the unused space at the end of this block is large enough to hold a block with at least one element
            if (block.Size - alignedBytes - sizeof(DynamicBlock<T>) < Math.Max(minBlockSize, sizeof(type)))
                return block;

            var newBlock = (DynamicBlock<T>)(((byte*)block) + sizeof(DynamicBlock<T>) + alignedBytes);
#if DYNAMIC_BLOCK_ALLOC_CHECK
            memcpy(newBlock.id, blockId, sizeof(newBlock.id));
            newBlock.allocator = this;
#endif
            newBlock.SetSize(block.Size - alignedBytes - (int)sizeof(DynamicBlock<T>), false);
            newBlock.next = block.next;
            newBlock.prev = block;
            if (newBlock.next != null) newBlock.next.prev = newBlock;
            else lastBlock = newBlock;
            newBlock.node = null;
            block.next = newBlock;
            block.SetSize(alignedBytes, block.IsBaseBlock);

            FreeInternal(newBlock);

            return block;
        }
        void FreeInternal(DynamicBlock<T> block)
        {
            Debug.Assert(block.node == null);

#if DYNAMIC_BLOCK_ALLOC_CHECK
            Debug.Assert(block.id[0] == 0x11111111 && block.id[1] == 0x22222222 && block.id[2] == 0x33333333 && block.allocator == this);
#endif

            // try to merge with a next free block
            var nextBlock = block.next;
            if (nextBlock != null && !nextBlock.IsBaseBlock && nextBlock.node != null)
            {
                UnlinkFreeInternal(nextBlock);
                block.SetSize(block.Size + sizeof(DynamicBlock<T>) + nextBlock.Size, block.IsBaseBlock);
                block.next = nextBlock.next;
                if (nextBlock.next != null) nextBlock.next.prev = block;
                else lastBlock = block;
            }

            // try to merge with a previous free block
            var prevBlock = block.prev;
            if (prevBlock != null && !block.IsBaseBlock && prevBlock.node != null)
            {
                UnlinkFreeInternal(prevBlock);
                prevBlock.SetSize(prevBlock.Size + sizeof(DynamicBlock<T>) + block.Size, prevBlock.IsBaseBlock);
                prevBlock.next = block.next;
                if (block.next != null) block.next.prev = prevBlock;
                else lastBlock = prevBlock;
                LinkFreeInternal(prevBlock);
            }
            else LinkFreeInternal(block);
        }
        void LinkFreeInternal(DynamicBlock<T> block)
        {
            block.node = freeTree.Add(block, block.Size);
            numFreeBlocks++;
            freeBlockMemory += block.Size;
        }
        void UnlinkFreeInternal(DynamicBlock<T> block)
        {
            freeTree.Remove(block.node);
            block.node = null;
            numFreeBlocks--;
            freeBlockMemory -= block.Size;
        }

        void CheckMemory()
        {
            for (var block = firstBlock; block != null; block = block.next)
            {
                // make sure the block is properly linked
                if (block.prev == null) Debug.Assert(firstBlock == block);
                else Debug.Assert(block.prev.next == block);
                if (block.next == null) Debug.Assert(lastBlock == block);
                else Debug.Assert(block.next.prev == block);
            }
        }
    }
}
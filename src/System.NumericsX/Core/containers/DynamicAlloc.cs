using System.Diagnostics;

namespace System.NumericsX.Core
{
    public class DynamicAlloc<T>
    {
        int baseBlockSize;
        int minBlockSize;

        int numUsedBlocks;          // number of used blocks
        int usedBlockMemory;        // total memory in used blocks

        int numAllocs;
        int numResizes;
        int numFrees;

        public DynamicAlloc(int baseBlockSize, int minBlockSize)
        {
            this.baseBlockSize = baseBlockSize;
            this.minBlockSize = minBlockSize;
            Clear();
        }

        public void Init() { }
        public void Shutdown() => Clear();
        public void SetFixedBlocks(int numBlocks) { }
        public void SetLockMemory(bool @lock) { }
        public void FreeEmptyBaseBlocks() { }

        public T[] Alloc(int num)
        {
            numAllocs++;
            if (num <= 0)
                return default;
            numUsedBlocks++;
            throw new NotImplementedException();
            //usedBlockMemory += num * sizeof(type);
            //return Mem_Alloc16(num * sizeof(type));
        }
        public K[] Alloc<K>(int num)
        {
            numAllocs++;
            if (num <= 0)
                return default;
            numUsedBlocks++;
            throw new NotImplementedException();
            //usedBlockMemory += num * sizeof(type);
            //return Mem_Alloc16(num * sizeof(type));
        }

        public K[] Resize<K>(K[] ptr, int num)
        {
            numResizes++;
            if (ptr == null)
                return Alloc<K>(num);

            if (num <= 0)
            {
                Free(ref ptr);
                return default;
            }

            Debug.Assert(false);
            return ptr;
        }

        public void Free<K>(ref K[] ptr)
        {
            numFrees++;
            if (ptr == null)
                return;
            ptr = default;
        }

        public string CheckMemory(T ptr) => null;

        void Clear()
        {
            numUsedBlocks = 0;
            usedBlockMemory = 0;
            numAllocs = 0;
            numResizes = 0;
            numFrees = 0;
        }

        public int NumBaseBlocks => 0;
        public int BaseBlockMemory => 0;
        public int NumUsedBlocks => numUsedBlocks;
        public int UsedBlockMemory => usedBlockMemory;
        public int NumFreeBlocks => 0;
        public int FreeBlockMemory => 0;
        public int NumEmptyBaseBlocks => 0;
    }
}
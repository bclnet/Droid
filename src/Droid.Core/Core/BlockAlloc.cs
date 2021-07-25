using System;

namespace Droid
{
    public class BlockAlloc<T>
    {
        class Element
        {
            public T t;
            public Element next;
        }

        class Block
        {
            public Element[] elements;
            public Block next;
        }

        int blockSize;
        Block blocks;
        Element free;
        int total;
        int active;

        public BlockAlloc(int blockSize) => this.blockSize = blockSize;

        public void Shutdown()
        {
            blocks = null;
            free = null;
            total = active = 0;
        }

        public T Alloc()
        {
            if (free == null)
            {
                var block = blocks = new Block
                {
                    next = blocks
                };
                for (var i = 0; i < blockSize; i++)
                {
                    block.elements[i].next = free;
                    free = block.elements[i];
                }
                total += blockSize;
            }
            active++;
            var element = free;
            free = free.next;
            element.next = null;
            return element.t;
        }
        public void Free(T t) => throw new NotImplementedException();
        //{
        //    var element = (Element)t;
        //    element.next = free;
        //    free = element;
        //    active--;
        //}

        public int TotalCount => total;
        public int AllocCount => active;
        public int FreeCount => total - active;
    }
}
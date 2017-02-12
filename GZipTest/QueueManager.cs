using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class ByteBlock
    {
        private int id;
        private byte[] buffer;
        private byte[] compressedBuffer;

        public int ID { get { return id; } }
        public byte[] Buffer { get { return buffer; }}
        public byte[] CompressedBuffer { get { return compressedBuffer; } }


        public ByteBlock(int id, byte[] buffer) : this (id, buffer, new byte[0])
        {

        }

        public ByteBlock(int id, byte[] buffer, byte[] compressedBuffer)
        {
            this.id = id;
            this.buffer = buffer;
            this.compressedBuffer = compressedBuffer;
        }

    }

    public class QueueManager
    {
        private object locker = new object();
        Queue<ByteBlock> queue = new Queue<ByteBlock>();
        bool isDead = false;
        private int blockId = 0;

        public void EnqueueForWriting(ByteBlock _block)
        {
            int id = _block.ID;
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                while (id != blockId)
                {
                    Monitor.Wait(locker);
                }

                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public void EnqueueForCompressing(byte[] buffer)
        {
            lock (locker)
            {
                if (isDead)
                    throw new InvalidOperationException("Queue already stopped");

                ByteBlock _block = new ByteBlock(blockId, buffer);
                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }


        public ByteBlock Dequeue()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !isDead)
                    Monitor.Wait(locker);

                if (queue.Count == 0)
                    return null;

                return queue.Dequeue();

            }
        }

        public void Stop()
        {
            lock (locker)
            {
                isDead = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}
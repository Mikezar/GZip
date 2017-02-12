using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{

    class Decompressor : GZip
    {
        int counter = 0;
        public Decompressor(string input, string output) : base(input, output)
        {


        }

        public override void Launch()
        {
            Console.WriteLine("Decompressing...\n");

            Thread _reader = new Thread(new ThreadStart(Read));
            _reader.Start();

            for (int i = 0; i < _threads; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Decompress, i);
            }

            Thread _writer = new Thread(new ThreadStart(Write));
            _writer.Start();

            WaitHandle.WaitAll(doneEvents);

            if (!_cancelled)
            {
                Console.WriteLine("\nDecompressing has been succesfully finished");
                _success = true;
            }
        }

        private void Read()
        {
            try
            {
                using (FileStream _compressedFile = new FileStream(sourceFile, FileMode.Open))
                {
                    while (_compressedFile.Position < _compressedFile.Length)
                    {
                        byte[] lengthBuffer = new byte[8];
                        _compressedFile.Read(lengthBuffer, 0, lengthBuffer.Length);
                        int blockLength = BitConverter.ToInt32(lengthBuffer, 4);
                        byte[] compressedData = new byte[blockLength];
                        lengthBuffer.CopyTo(compressedData, 0);

                        _compressedFile.Read(compressedData, 8, blockLength - 8);
                        int _dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
                        byte [] lastBuffer = new byte[_dataSize];

                        ByteBlock _block = new ByteBlock(counter, lastBuffer, compressedData);
                        _queueReader.EnqueueForWriting(_block);
                        counter++;
                        ConsoleProgress.ProgressBar(_compressedFile.Position, _compressedFile.Length);

                    }
                    _queueReader.Stop();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }

        private void Decompress(object i)
        {
            try
            {
                while (true && !_cancelled)
                {
                    ByteBlock _block = _queueReader.Dequeue();
                    if (_block == null)
                        return;

                    using (MemoryStream ms = new MemoryStream(_block.CompressedBuffer))
                    {
                        using (GZipStream _gz = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            _gz.Read(_block.Buffer, 0, _block.Buffer.Length);
                            byte[] decompressedData = _block.Buffer.ToArray();
                            ByteBlock block = new ByteBlock(_block.ID, decompressedData);
                            _queueWriter.EnqueueForWriting(block);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);
                _cancelled = true;
            }
        }

        private void Write()
        {
            try
            {
                using (FileStream _decompressedFile = new FileStream(sourceFile.Remove(sourceFile.Length - 3), FileMode.Append))
                {
                    while (true && !_cancelled)
                    {
                        ByteBlock _block = _queueWriter.Dequeue();
                        if (_block == null)
                            return;

                        _decompressedFile.Write(_block.Buffer, 0, _block.Buffer.Length);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _cancelled = true;
            }
        }
    }
}
 
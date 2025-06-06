﻿using ft.Commands;
using ft.Listeners;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ft.Streams
{
    public class SharedFileStream(SharedFileManager sharedFileManager, int connectionId) : Stream
    {
        public void EstablishConnection(string destinationEndpointStr)
        {
            SharedFileManager.Connect(ConnectionId, destinationEndpointStr);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SharedFileManager SharedFileManager { get; } = sharedFileManager;
        public int ConnectionId { get; } = connectionId;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        byte[]? currentData = null;
        int currentDataIndex;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (currentData == null || currentData.Length == currentDataIndex)
            {
                currentData = SharedFileManager.Read(ConnectionId);
                currentDataIndex = 0;
            }

            if (currentData == null)
            {
                return 0;
            }
            else
            {
                var toCopy = count;
                toCopy = Math.Min(toCopy, currentData.Length - currentDataIndex);

                Array.Copy(currentData, currentDataIndex, buffer, offset, toCopy);
                currentDataIndex += toCopy;

                return toCopy;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var toSend = buffer;
            if (offset != 0 || count != buffer.Length)
            {
                toSend = buffer.Skip(offset).Take(count).ToArray();
            }

            var forwardCommand = new Forward(ConnectionId, toSend);
            SharedFileManager.EnqueueToSend(forwardCommand);
        }

        public override void Close()
        {
            base.Close();

            SharedFileManager.TearDown(ConnectionId);
        }
    }
}

﻿using mesos;

namespace com.bcrusu.mesosclr
{
    public interface IExecutorDriver
    {
        Status Start();

        Status Stop();

        Status Abort();

        Status Join();

        Status Run();

        Status SendStatusUpdate(TaskStatus status);

        Status SendFrameworkMessage(byte[] data);
    }
}

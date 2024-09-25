using System.Collections.Generic;
using UnityEngine;

namespace RGSSUnity
{
    // Unity's Debug.log would extract the stacktrace from c# to native,
    // and if we use this API in Ruby's rescue block, crash would happen. 
    // As a workaround, we need to use a message queue to store the log message
    // and print it in next frame.
    public class RGSSLogger
    {
        public static readonly RGSSLogger Instance = new();

        private readonly Queue<(string Message, bool IsError)> messageQueue = new();

        public static void Log(string message)
        {
            Instance.EnqueueMessage(message, false);
        }

        public static void LogError(string message)
        {
            Instance.EnqueueMessage(message, true);
        }

        public void Update()
        {
            while (this.messageQueue.Count > 0)
            {
                var message = this.messageQueue.Dequeue();
                if (message.IsError)
                {
                    Debug.LogError(message.Message);
                }
                else
                {
                    Debug.Log(message.Message);
                }
            }
        }

        public void EnqueueMessage(string message, bool isError)
        {
            this.messageQueue.Enqueue((message, isError));
        }

        private (string Message, bool isError) DequeueMessage()
        {
            return this.messageQueue.Dequeue();
        }
    }
}
using System;
using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitor;
using System.Text;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreams
{
    public class ParallelInternalMessageStream : ParallelStream<InternalMessage>
    {
        public override R Accept<R>(ITypedStreamVisitor<R> visitor, InternalMessage data)
        {
            return visitor.Visit(this, data);
        }

        public override R Accept<R>(IStreamVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public override InternalMessage FromBytes(byte[] data)
        {
            if (data.Length < 3)
            {
                return null;
            }
            string messageStrings = Encoding.Unicode.GetString(data, 1, data.Length - 1);
            string sender, content;
            if (messageStrings.IndexOf('\n') == -1)
            {
                sender = messageStrings;
                content = string.Empty;
            }
            else
            {
                int splitterIndex = messageStrings.IndexOf('\n');
                sender = messageStrings.Substring(0, splitterIndex);
                if (sender.Length == 0)
                {
                    return null;
                }
                if (splitterIndex == messageStrings.Length - 1)
                {
                    content = string.Empty;
                }
                else
                {
                    content = messageStrings.Substring(splitterIndex + 1, messageStrings.Length - splitterIndex - 1);
                }
            }
            return new InternalMessage
            {
                MessageType = (InternalMessageType)data[0],
                Sender = sender,
                Content = content
            };
        }

        public override byte[] GetBytes(InternalMessage message)
        {
            byte[] serializedStrings;
            if (message.Content == null)
            {
                serializedStrings = Encoding.Unicode.GetBytes($"{message.Sender}");
            }
            else
            {
                serializedStrings = Encoding.Unicode.GetBytes($"{message.Sender}\n{message.Content}");
            }
            byte[] serializedMessage = new byte[1 + serializedStrings.Length];
            serializedMessage[0] = (byte)message.MessageType;
            Array.Copy(serializedStrings, 0, serializedMessage, 1, serializedStrings.Length);
            return serializedMessage;
        }
    }
}

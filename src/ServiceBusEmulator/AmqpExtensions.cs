using Amqp;
using Amqp.Types;

namespace ServiceBusEmulator
{
    public static class AmqpExtensions
    {
        private static readonly Symbol XOptSequenceNumber = "x-opt-sequence-number";

        public static Message Clone(this Message message)
        {
            return message == null
                        ? null
                        : Message.Decode(message.Encode());
        }

        public static Message AddSequenceNumber(this Message message, long sequence)
        {
            if (message != null)
            {
                message.MessageAnnotations ??= new Amqp.Framing.MessageAnnotations();
                if (message.MessageAnnotations[XOptSequenceNumber] == null)
                {
                    message.MessageAnnotations[XOptSequenceNumber] = sequence;
                }
            }
            return message;
        }

    }
}

using System.Buffers;
using MessagePack;

namespace CbaCompatServer.Protocol;

public static class MessagePackEnvelopeCodec
{
    public static MessageEnvelope DecodeRequest(ReadOnlyMemory<byte> data)
    {
        var reader = new MessagePackReader(data);
        var count = reader.ReadArrayHeader();
        if (count != 3)
        {
            throw new InvalidDataException($"Unsupported MessagePack envelope item count: {count}.");
        }

        var sessionId = reader.ReadUInt32();
        var methodName = reader.ReadString() ?? string.Empty;
        var payload = reader.ReadBytes();

        return new MessageEnvelope(sessionId, methodName, payload?.ToArray() ?? Array.Empty<byte>());
    }

    public static byte[] EncodeResponse(MessageEnvelope envelope)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        writer.Write(envelope.SessionId);
        writer.Write(envelope.MethodName);
        writer.WriteBinHeader(envelope.Payload.Length);
        writer.WriteRaw(envelope.Payload);
        writer.Flush();
        return buffer.WrittenSpan.ToArray();
    }
}

namespace CbaCompatServer.Protocol;

public readonly record struct MessageEnvelope(uint SessionId, string MethodName, byte[] Payload);

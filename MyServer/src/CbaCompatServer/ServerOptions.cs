namespace CbaCompatServer;

public sealed class ServerOptions
{
    public const string SectionName = "Server";

    public int HttpPort { get; set; } = 5000;

    public int TcpPort { get; set; } = 5100;

    public string PublicHost { get; set; } = "127.0.0.1";

    public int RequestLengthPrefixBytes { get; set; } = 2;

    public int ResponseLengthPrefixBytes { get; set; } = 4;

    public string DatabasePath { get; set; } = "Data/cba-compat.db";

    public string[] DevAccountPrefixes { get; set; } = ["dev_", "test_", "gm_"];

    public int DevPlayerLevel { get; set; } = 30;
}

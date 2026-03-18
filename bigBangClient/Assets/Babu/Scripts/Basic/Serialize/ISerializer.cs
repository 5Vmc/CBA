
namespace Babu
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);

        T Deserialize<T>(byte[] data);
    }
}

namespace analyticsLibrary.library
{
    public interface IKeyIndex
    {
        bool keyExists(string key);

        bool keyExists(string key, out int index);
    }
}
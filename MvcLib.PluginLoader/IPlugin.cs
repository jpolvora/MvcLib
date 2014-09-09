namespace MvcLib.PluginLoader
{
    public interface IPlugin
    {
        string PluginName { get;}
        void Start();

    }
    
}
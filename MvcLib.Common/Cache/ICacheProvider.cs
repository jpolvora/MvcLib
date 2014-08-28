namespace MvcLib.Common.Cache
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Indica se houve uma tentativa de utilizar o cache com a chave, contendo um valor não nulo.
        /// Útil para verificar se uma entrada foi inserida no cache, mesmo que o cache esteja desabilitado.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasEntry(string key);

        object Get(string key);

        T Set<T>(string key, T value, int duration = 20, bool sliding = true)
            where T : class;

        void Remove(string key);
        void Clear();
    }
}
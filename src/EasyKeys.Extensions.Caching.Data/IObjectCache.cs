namespace EasyKeys.Extensions.Caching.Data
{
    public interface IObjectCache
    {
        int GetSize();

        int GetCacheCount();

        List<object> GetAll();

        object Read(string code);

        bool Exists(string code);

        object Read(int id);

        void Clear();

        bool Exists(int id);

        bool Exists(int? id);

        void Save(int? id, string code, object data);

        void Save(string code, object data);

        void Delete(int? id, string code);
    }
}

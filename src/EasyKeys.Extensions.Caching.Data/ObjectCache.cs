namespace EasyKeys.Extensions.Caching.Data
{
    public class ObjectCache
    {
        private IObjectCache? _cache;

        public int GetSize()
        {
            return GetCache().GetSize();
        }

        public int GetCacheCount()
        {
            return GetCache().GetCacheCount();
        }

        public List<object> GetAll()
        {
            return GetCache().GetAll();
        }

        public object Read(string code)
        {
            return GetCache().Read(code);
        }

        public bool Exists(string code)
        {
            return GetCache().Exists(code);
        }

        public object Read(int id)
        {
            return GetCache().Read(id);
        }

        public void Clear()
        {
            GetCache().Clear();
        }

        public bool Exists(int id)
        {
            return GetCache().Exists(id);
        }

        public bool Exists(int? id)
        {
            return GetCache().Exists(id);
        }

        public void Save(int? id, string code, object data)
        {
            GetCache().Save(id, code, data);
        }

        public void Save2(string code, object data)
        {
            GetCache().Save(code, data);
        }

        public void Delete(int? id, string code)
        {
            GetCache().Delete(id, code);
        }

        private IObjectCache GetCache()
        {
            if (_cache == default)
            {
                _cache = new ObjectCacheInMemory();
            }

            return _cache;
        }
    }
}

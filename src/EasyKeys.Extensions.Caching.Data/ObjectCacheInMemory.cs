using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyKeys.Extensions.Caching.Data
{
    public class ObjectCacheInMemory : IObjectCache
    {
        private readonly Hashtable _objectCache_Ids = new ();
        private readonly Hashtable _objectCache_Codes = new ();
        private readonly object _lockObject = new ();

        public int GetSize()
        {
            var allObject = GetAll();
            var totalSize = 0;
            foreach (var obj in allObject)
            {
                var properties1 = obj.GetType().GetProperties();
                int i;
                var loopTo = properties1.Length - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    var property1 = properties1[i];
                    if (property1.CanRead)
                    {
                        var val = property1.GetValue(obj, null);
                        var m = new MemoryStream();
                        var b = new BinaryFormatter();
                        if (val is object)
                        {
                            b.Serialize(m, val);
                        }

                        totalSize += (int)m.Length;
                    }
                }
            }

            return totalSize;
        }

        public int GetCacheCount()
        {
            var result = 0;
            lock (_lockObject)
            {
                // for codes and id keys
                result = _objectCache_Codes.Values.Count;
            }

            return result;
        }

        public List<object> GetAll()
        {
            var result = new List<object>();
            lock (_lockObject)
            {
                if (_objectCache_Ids.Values.Count > 0)
                {
                    var data = new object[_objectCache_Ids.Count];
                    _objectCache_Ids.Values.CopyTo(data, 0);
                    result.AddRange(data.ToList());
                }
                else if (_objectCache_Codes.Values.Count > 0)
                {
                    var dataParent = new object[_objectCache_Codes.Count];
                    _objectCache_Codes.Values.CopyTo(dataParent, 0);
                    if (dataParent.Length == 1)
                    {
                        var data = dataParent[0];
                        result.AddRange((IEnumerable<object>)data);
                    }
                    else
                    {
                        result.AddRange(dataParent.ToList());
                    }
                }
            }

            return result;
        }

        public object Read(string code)
        {
            object result = null;
            lock (_lockObject)
            {
                if (_objectCache_Codes.ContainsKey(code))
                {
                    result = _objectCache_Codes[code];
                }
            }

            return result;
        }

        public bool Exists(string code)
        {
            var result = false;
            lock (_lockObject)
            {
                result = _objectCache_Codes.ContainsKey(code);
            }

            return result;
        }

        public object Read(int id)
        {
            object result = null;
            lock (_lockObject)
            {
                if (_objectCache_Ids.ContainsKey(id))
                {
                    result = _objectCache_Ids[id];
                }
            }

            return result;
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _objectCache_Ids.Clear();
                _objectCache_Codes.Clear();
            }
        }

        public bool Exists(int id)
        {
            var result = false;
            lock (_lockObject)
            {
                result = _objectCache_Ids.ContainsKey(id);
            }

            return result;
        }

        public bool Exists(int? id)
        {
            var result = false;
            lock (_lockObject)
            {
                result = _objectCache_Ids.ContainsKey(id);
            }

            return result;
        }

        public void Save(int? id, string code, object data)
        {
            if (id == default)
            {
                return;
            }

            if (code == default)
            {
                return;
            }

            lock (_lockObject)
            {
                if (_objectCache_Ids.ContainsKey(id) == false)
                {
                    _objectCache_Ids.Add(id, data);
                }

                if (_objectCache_Codes.ContainsKey(code) == false)
                {
                    _objectCache_Codes.Add(code, data);
                }
            }
        }

        public void Save(string code, object data)
        {
            lock (_lockObject)
            {
                _objectCache_Codes.Add(code, data);
            }
        }

        public void Delete(int? id, string code)
        {
            lock (_lockObject)
            {
                _objectCache_Ids.Remove(id);
                _objectCache_Codes.Remove(code);
            }
        }
    }
}

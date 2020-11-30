using System.Collections.Generic;
using System.Linq;

namespace EasyKeys.Extensions.Data.Dapper
{
    public sealed class PropertyContainer
    {
        private readonly Dictionary<string, object> _ids;
        private readonly Dictionary<string, object> _values;

        public PropertyContainer()
        {
            _ids = new Dictionary<string, object>();
            _values = new Dictionary<string, object>();
        }

        public IEnumerable<string> IdNames => _ids.Keys;

        public IEnumerable<string> ValueNames => _values.Keys;

        public IEnumerable<string> AllNames => _ids.Keys.Union(_values.Keys);

        public IDictionary<string, object> IdPairs => _ids;

        public IDictionary<string, object> ValuePairs => _values;

        public IEnumerable<KeyValuePair<string, object>> AllPairs => _ids.Concat(_values);

        public void AddId(string name, object value)
        {
            _ids.Add(name, value);
        }

        public void AddValue(string name, object value)
        {
            _values.Add(name, value);
        }
    }
}

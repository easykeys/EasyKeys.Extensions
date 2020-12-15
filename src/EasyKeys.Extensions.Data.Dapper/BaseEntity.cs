using System;

namespace EasyKeys.Extensions.Data.Dapper
{
    [Serializable]
    public abstract class BaseEntity
    {
        public string Code { get; set; } = Guid.NewGuid().ToString();
    }
}

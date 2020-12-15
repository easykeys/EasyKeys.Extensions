using System;

namespace EasyKeys.Extensions.Data.Dapper
{
    [Serializable]
    public abstract class AuditableEntity : BaseEntity
    {
        public bool? Deleted { get; set; }

        public DateTime InsertDateTime { get; set; } = DateTime.Now;

        public DateTime LastUpdateDateTime { get; set; } = DateTime.Now;
    }
}

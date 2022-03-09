using EasyKeys.Extensions.Data.Dapper;

namespace EasyKeys.Extensions.Dapper.UnitTest.Entities
{
    [Serializable]
    public class AuditEntity : BaseEntity
    {
        new public string Code { get; set; } = Guid.NewGuid().ToString();

        public DateTime? InsertDateTime { get; set; }

        public DateTime? LastUpdateDateTime { get; set; }

        public string InsertApp { get; set; } = "unit test";

        public string LastUpdateApp { get; set; } = "unit test";
    }
}

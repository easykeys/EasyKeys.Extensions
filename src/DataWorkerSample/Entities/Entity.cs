using EasyKeys.Extensions.Data.Dapper;

namespace DataWorkerSample.Entities
{
    public class Entity : AuditableEntity
    {
        public string InsertApp { get; set; } = nameof(DataWorkerSample);

        public string LastUpdateApp { get; set; } = nameof(DataWorkerSample);
    }
}

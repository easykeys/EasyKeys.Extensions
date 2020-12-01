using System;

using Dapper.Contrib.Extensions;

namespace DataWorkerSample.Entities
{
    [Serializable]
    [Table("EmailLog")]
    public class EmailLogEntity : Entity
    {
        [Key]
        public int EmailLogID { get; set; }

        public int? EmailRequestID { get; set; }

        public string ToEmailList { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;

        public string BCCEmailList { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public bool? BodyIsHTML { get; set; }

        public string Body { get; set; } = string.Empty;

        public DateTime? CreatedDateTime { get; set; }

        public bool? Sent { get; set; }

        public DateTime? SentDateTime { get; set; }
    }
}

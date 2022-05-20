using Dapper.Contrib.Extensions;

namespace EasyKeys.Extensions.Dapper.UnitTest.Entities
{
    [Table("Manufacturer")]
    [Serializable]
    public class Vendor : AuditEntity
    {
        [Key]
        public int ManufacturerId { get; set; }

        public string? Manufacturer { get; set; }

        public string? PageName { get; set; }

        public string? PageTitleSuffix { get; set; }

        public bool Active { get; set; }

        public bool Menu { get; set; }

        public bool? HasLockSeries { get; set; }

        public int GraphicId { get; set; }

        public string? MetaKeywords { get; set; }

        public string? MetaDescription { get; set; }

        public bool LockManufacturer { get; set; }

        public bool FurnitureManufacturer { get; set; }

        public bool OrderFrom { get; set; }

        public string? PageTitle { get; set; }

        public string? PageSubTitle { get; set; }

        public string? Introduction { get; set; }

        public bool ShowEndText { get; set; }

        public string? EndText { get; set; }

        public bool ShowHelpVideo { get; set; }

        public string? HelpVideo { get; set; }

        public string? RawUrl { get; set; }

        public string? MappedUrl { get; set; }

        public bool? Deleted { get; set; }

        public bool? Rvkey { get; set; }

        public bool? Toolbox { get; set; }

        public string? HtmmapName { get; set; }

        public int? PoleadTimeInDays { get; set; }

        public string? ContactName { get; set; }

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public string? VideoLinks { get; set; }

        public string? AltKeyText { get; set; }
    }
}

namespace EasyKeys.Extensions.Data.Dapper.Sorting
{
    public class SortDescriptor
    {
        public static object SortingDirection { get; internal set; } = new object();

        public SortingDirection Direction { get; set; }

        public string Field { get; set; } = string.Empty;
    }
}

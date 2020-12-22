namespace Demo.EnrichedSearch.Shared.Models
{
    public class IndexStats
    {
        public string IndexName { get; set; }
        public long DocumentCount { get; set; }
        public long StorageSize { get; set; }
        public IndexStats()
        {

        }
    }
}

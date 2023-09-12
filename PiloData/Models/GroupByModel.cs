namespace PiloData.Models
{
    public class GroupByModel : ResponseModel
    {
        public int Key { get; set; }
        public string SuffixGroup { get; set; }
        public string PrefixGroup { get; set; }
        public int MidfixGroup { get; set; }
        public string MidfixGroupString
        {
            get
            {
                return MidfixGroup.ToString();
            }
        }

    }
}

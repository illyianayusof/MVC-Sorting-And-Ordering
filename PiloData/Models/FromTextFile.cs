using System.Text.RegularExpressions;

namespace PiloData.Models
{
    public class FromTextFile : ResponseModel
    {
        public int Index { get; set; }
        public string ERSATZ { get; set; }
        public string ENTNAHME { get; set; }
        public string TYP { get; set; }
        public string ERF_USER { get; set; }
        public string ERF_DATUM { get; set; }
        // public string R_ENTNAHME { get; set; }

        public string Prefix_ENTNAHME
        {
            get
            {
                return ENTNAHME.Substring(0, 2);
            }
        }

        public string Suffix_ENTNAHME
        {
            get
            {
                return ENTNAHME.Substring(ENTNAHME.Length - 1);
            }
        }

        public int Middle_ENTNAHME
        {
            get
            {
                return Convert.ToInt32(Regex.Split(ENTNAHME, @"\D+")[1]);

            }
        }
    }
}

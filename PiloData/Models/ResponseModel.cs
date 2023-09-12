namespace PiloData.Models
{
    public class ResponseModel
    {
        public string Message { get; set; }
        public string FilePath { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsResponse { get; set; }
        public int NoOfRows { get; set; }
        public int TotalNoOfRows { get; set; }
        public string NetworkNumber { get; set; }
        public DateTime InsertedTime { get; set; }
        public int NoOfGroups { get; set; }
    }
       
}

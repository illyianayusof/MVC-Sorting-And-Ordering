using System.ComponentModel.DataAnnotations;

namespace PiloData.Models
{
    public class SingleFileModel : ResponseModel
    {
        
        [Required(ErrorMessage = "Please select file")]
        public IFormFile File { get; set; }
        public List<FromTextFile> Files { get; set; }
        public string StartRange { get; set; }
        public string EndRange { get; set; }

    }
}

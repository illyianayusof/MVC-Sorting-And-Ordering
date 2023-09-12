using PiloData.Models;
using System.ComponentModel.DataAnnotations;

namespace PiloData.Controllers
{
    public class MultipleFiles : ResponseModel
    {
        [Required(ErrorMessage = "Please select files")]
        public List<IFormFile> Files { get; set; }
    }
}

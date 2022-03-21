using System.ComponentModel.DataAnnotations;

namespace TableIT.Web.Data;

public class ViewerModel
{
    [Required(ErrorMessage = "Table ID is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Table ID must be exactly 6 characters")]
    public string? TableId { get; set; }
}

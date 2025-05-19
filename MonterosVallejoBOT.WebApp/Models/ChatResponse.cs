using System.ComponentModel.DataAnnotations; 

namespace MonterosVallejoBOT.WebApp.Models;

public class ChatResponse
{
    [Key] 
    public int Id { get; set; }

    [Required(ErrorMessage = "La respuesta no puede estar vacía.")]
    public string? Respuesta { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "El proveedor no puede estar vacío.")]
    public string? Proveedor { get; set; } 

    [Required(ErrorMessage = "El campo 'GuardadoPor' no puede estar vacío.")]
    public string? GuardadoPor { get; set; }
}
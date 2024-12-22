using System.ComponentModel.DataAnnotations;

namespace GestaoClientes.Models
{
    public class Cliente
    {
        [Key]
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string? Logotipo { get; set; }
        public List<Logradouro> Logradouros { get; set; }
    }
}

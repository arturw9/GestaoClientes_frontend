using System.ComponentModel.DataAnnotations;

namespace GestaoClientes.Models
{
    public class Logradouro
    {
        [Key]
        public Guid IdLogradouro { get; set; }
        public Guid IdCliente { get; set; }
        public Cliente Cliente { get; set; }
        public string Rua { get; set; }
        public string Quadra { get; set; }
        public string Lote { get; set; }
        public string? Numero { get; set; }
        public string Bairro { get; set; }
    }
}

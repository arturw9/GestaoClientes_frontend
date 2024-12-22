using GestaoClientes.Models;

namespace GestaoClientes.ViewModels
{
    public class ClienteViewModel
    {
        public Guid Id { get; set; }
        public string Nome {  get; set; }
        public string Email {  get; set; }
        public string Logotipo {  get; set; }
        public List<Logradouro> ?Logradouros { get; set; }
        public IFormFile? LogotipoFile { get; set; }
    }
}
namespace GestaoClientes.ViewModels
{
    public class LogradouroViewModel
    {
        public Guid IdLogradouro { get; set; }
        public Guid IdCliente { get; set; }
        public string Rua { get; set; }
        public string Quadra { get; set; }
        public string Lote { get; set; }
        public int? Numero { get; set; }
        public string Bairro { get; set; }
    }

}

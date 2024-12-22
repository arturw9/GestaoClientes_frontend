using GestaoClientes.ViewModels;

public class DetalhesClienteViewModel
{
    public string ClienteNome { get; set; }
    public Guid? ClienteId { get; set; }
    public List<LogradouroViewModel> Logradouros { get; set; }
}

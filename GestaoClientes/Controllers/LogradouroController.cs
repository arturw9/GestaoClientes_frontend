using GestaoClientes.Models;
using GestaoClientes.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

public class LogradourosController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public LogradourosController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> AdicionarLogradouro()
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetStringAsync($"{_configuration["Jwt:Url"].ToString()}Clientes/Visualizar");
        var clientes = JsonConvert.DeserializeObject<List<Cliente>>(response);

        ViewBag.Clientes = clientes;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> InserirLogradouro(LogradouroViewModel logradouro, string idCliente)
    {
        var token = HttpContext.Session.GetString("AuthToken");

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var url = $"{_configuration["Jwt:Url"].ToString()}Logradouros/Inserir?idCliente={idCliente}";

        var jsonContent = JsonConvert.SerializeObject(logradouro);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", "Clientes");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Erro ao adicionar logradouro.");
            return View("AdicionarLogradouro", logradouro);
        }
    }

    public async Task<IActionResult> Detalhes(Guid id, string nome)
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        string apiUrl = $"{_configuration["Jwt:Url"].ToString()}Logradouros/Visualizar?idCliente={id}";

        var response = await _httpClient.GetAsync(apiUrl);
        if (!response.IsSuccessStatusCode)
        {
            return View("Error", new ErrorViewModel { RequestId = "Não foi possível carregar os logradouros." });
        }

        var logradouros = await response.Content.ReadFromJsonAsync<List<LogradouroViewModel>>();

        var viewModel = new DetalhesClienteViewModel
        {
            ClienteNome = nome,
            Logradouros = logradouros
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Atualizar(Guid id, Guid idCliente, string rua, string quadra,
        string lote, int? numero, string bairro)
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var viewModel = new LogradouroViewModel
        {
            IdLogradouro = id,
            Rua = rua,
            Quadra = quadra,
            Lote = lote,
            Numero = numero,
            Bairro = bairro,
            IdCliente = idCliente
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Atualizar(LogradouroViewModel viewModel)
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        if (ModelState.IsValid)
        {
            var jsonContent = JsonConvert.SerializeObject(viewModel);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_configuration["Jwt:Url"].ToString()}Logradouros/Atualizar?idCliente={viewModel.IdCliente}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Detalhes", "Logradouros", new { id = viewModel.IdCliente });
            }

            ModelState.AddModelError(string.Empty, "Erro ao atualizar logradouro.");
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Remover(Guid idLogradouro, Guid idCliente)
    {
        var token = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"{_configuration["Jwt:Url"].ToString()}Logradouros/Remover?idLogradouro={idLogradouro}&idCliente={idCliente}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", "Clientes");
        }

        ModelState.AddModelError(string.Empty, "Erro ao deletar logradouro.");
        return View("Error", new ErrorViewModel { RequestId = "Falha ao deletar o logradouro." });
    }
}
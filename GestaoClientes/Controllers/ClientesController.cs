using GestaoClientes.Models;
using GestaoClientes.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace GestaoClientes.Controllers
{
    public class ClientesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ClientesController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ActionResult> Index()
        {
            // Verifica se o token já foi salvo em Session
            var token = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(token))
            {
                var loginSuccess = await LoginAutomatico();

                token = HttpContext.Session.GetString("AuthToken"); // Obtém o token após o login
            }

            List<Cliente> clientes = new List<Cliente>();

            using (HttpClient client = new HttpClient())
            {
                // Adiciona o token ao cabeçalho da requisição
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync($"{_configuration["Jwt:Url"].ToString()}Clientes/Visualizar");
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    clientes = JsonConvert.DeserializeObject<List<Cliente>>(responseData);
                }
            }

            return View(clientes);
        }
        private async Task<bool> LoginAutomatico()
        {
            var loginModel = new LoginViewModel
            {
                Username = _configuration["Jwt:Username"].ToString(),
                Password = _configuration["Jwt:Password"].ToString()
            };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = JsonConvert.SerializeObject(loginModel);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"{_configuration["Jwt:Url"].ToString()}Administrador/Login", content);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseData);
                    string token = result?.token; // Acessando diretamente a chave "token"

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Armazena o token em Session para ser usado nas próximas requisições
                        HttpContext.Session.SetString("AuthToken", token);
                        return true;
                    }
                }
            }

            return false;
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Inserir(ClienteViewModel cliente)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken"); // Obtém o token de Session

                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
                {
                    // Caminho para salvar a imagem
                    var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", cliente.LogotipoFile.FileName);

                    // Salvar o arquivo no diretório
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await cliente.LogotipoFile.CopyToAsync(stream);
                    }

                    // Atribuir o caminho da imagem ao cliente
                    cliente.Logotipo = "/images/" + cliente.LogotipoFile.FileName;
                }

                var clienteModel = new Cliente
                {
                    Nome = cliente.Nome,
                    Email = cliente.Email,
                    Logotipo = cliente.Logotipo,
                    Logradouros = cliente.Logradouros,
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Adiciona o token no cabeçalho

                    var jsonContent = JsonConvert.SerializeObject(clienteModel);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"{_configuration["Jwt:Url"].ToString()}Clientes/Inserir", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");

                        return View("Adicionar", cliente);
                    }
                    else
                    {

                        ModelState.AddModelError("", "Ocorreu um erro ao salvar o cliente. Tente novamente.");

                        return View("Adicionar", cliente);
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocorreu um erro inesperado. Tente novamente.");
                return View("Adicionar", cliente);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(string email)
        {
            var clientes = new List<ClienteViewModel>();

            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Adiciona o token no cabeçalho

                HttpResponseMessage response = await client.GetAsync($"{_configuration["Jwt:Url"].ToString()}Visualizar?email={email}");
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    clientes = JsonConvert.DeserializeObject<List<ClienteViewModel>>(responseData);
                }
            }

            var cliente = clientes.FirstOrDefault();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Atualizar(Guid id, ClienteViewModel cliente)
        {
            if (ModelState.IsValid)
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login");
                }

                if (cliente.LogotipoFile != null && cliente.LogotipoFile.Length > 0)
                {
                    // Caminho para salvar a imagem
                    var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", cliente.LogotipoFile.FileName);

                    // Salvar o arquivo no diretório
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await cliente.LogotipoFile.CopyToAsync(stream);
                    }

                    // Atribuir o caminho da imagem ao cliente
                    cliente.Logotipo = "/images/" + cliente.LogotipoFile.FileName;
                }

                var clienteModel = new Cliente
                {
                    Nome = cliente.Nome,
                    Email = cliente.Email,
                    Logotipo = cliente.Logotipo,
                    Logradouros = cliente.Logradouros,
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Adiciona o token no cabeçalho

                    var jsonContent = JsonConvert.SerializeObject(clienteModel);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync($"{_configuration["Jwt:Url"].ToString()}Clientes/Atualizar?id={id}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("Email", "E-mail possui a outra cliente!");
                        return View("Editar", cliente);
                    }
                }
            }

            return View("Editar", cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Remover(Guid id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            using (HttpClient client = _httpClient)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Adiciona o token no cabeçalho

                var response = await client.DeleteAsync($"{_configuration["Jwt:Url"].ToString()}Clientes/Remover?id={id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Clientes");
                }

                ModelState.AddModelError(string.Empty, "Erro ao deletar logradouro.");
                return View("Error", new ErrorViewModel { RequestId = "Falha ao deletar o logradouro." });
            }
        }
    }
}
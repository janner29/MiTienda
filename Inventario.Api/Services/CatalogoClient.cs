namespace Inventario.Api.Services;

public class CatalogoClient
{
    private readonly HttpClient _http;
    public CatalogoClient(HttpClient http) => _http = http;

    // true si el producto existe en Catalogo.Api, false si responde 404.
    // Si Catalogo está apagado lanza HttpRequestException.
    public async Task<bool> ProductoExisteAsync(int idProducto)
    {
        var resp = await _http.GetAsync($"api/Productos/{idProducto}");
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        resp.EnsureSuccessStatusCode();
        return true;
    }
}
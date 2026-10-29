using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MerchantOS.Admin.Models;
namespace MerchantOS.Admin.Services;
public interface IPosClient {
    Task<List<ProductDto>> Products(CancellationToken ct);
    Task<ProductDto?> Product(string id, CancellationToken ct);
    Task<List<OrderDto>> Orders(CancellationToken ct);
    Task<OrderDto?> Order(Guid id, CancellationToken ct);
}
public sealed class PosUnavailableException : Exception {
    public PosUnavailableException(Exception inner) : base("The POS service is unavailable. Please try again.", inner) { }
}
public sealed class PosClient(HttpClient client) : IPosClient {
    private async Task<T?> Read<T>(string path, bool allowMissing, CancellationToken ct) {
        try {
            using var response = await client.GetAsync(path, ct);
            if (allowMissing && response.StatusCode == HttpStatusCode.NotFound) return default;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
                ?? throw new JsonException("Unexpected null POS response");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is OperationCanceledException && !ct.IsCancellationRequested) {
            throw new PosUnavailableException(ex);
        }
    }
    public async Task<List<ProductDto>> Products(CancellationToken ct) => await Read<List<ProductDto>>("api/products", false, ct) ?? [];
    public Task<ProductDto?> Product(string id, CancellationToken ct) => Read<ProductDto>("api/products/" + Uri.EscapeDataString(id), true, ct);
    public async Task<List<OrderDto>> Orders(CancellationToken ct) => await Read<List<OrderDto>>("api/orders", false, ct) ?? [];
    public Task<OrderDto?> Order(Guid id, CancellationToken ct) => Read<OrderDto>($"api/orders/{id:D}", true, ct);
}

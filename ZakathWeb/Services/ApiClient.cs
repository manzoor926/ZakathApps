using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ZakathApp.Web.Models;

namespace ZakathApp.Web.Services;

public interface IApiClient
{
    Task<ApiResponse<UserModel>> LoginAsync(LoginRequest request);
    Task<ApiResponse<object>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<UserModel>> GetProfileAsync(string token);
    Task<ApiResponse<DashboardStats>> GetDashboardStatsAsync(string token);
    Task<ApiResponse<List<RecentTransaction>>> GetRecentTransactionsAsync(string token);
    Task<ApiResponse<List<IncomeModel>>> GetIncomesAsync(string token);
    Task<ApiResponse<object>> CreateIncomeAsync(IncomeRequest request, string token);
    Task<ApiResponse<object>> DeleteIncomeAsync(int id, string token);
    Task<ApiResponse<List<ExpenseModel>>> GetExpensesAsync(string token);
    Task<ApiResponse<object>> CreateExpenseAsync(ExpenseRequest request, string token);
    Task<ApiResponse<object>> DeleteExpenseAsync(int id, string token);
    Task<ApiResponse<List<AssetModel>>> GetAssetsAsync(string token);
    Task<ApiResponse<object>> CreateAssetAsync(AssetRequest request, string token);
    Task<ApiResponse<object>> DeleteAssetAsync(int id, string token);
    Task<ApiResponse<ZakathResult>> CalculateZakathAsync(CalculateZakathRequest request, string token);
    Task<ApiResponse<List<ZakathResult>>> GetZakathHistoryAsync(string token);
    Task<ApiResponse<List<PaymentModel>>> GetPaymentsAsync(string token);
    Task<ApiResponse<object>> CreatePaymentAsync(PaymentRequest request, string token);
    Task<ApiResponse<object>> DeletePaymentAsync(int id, string token);
    Task<ApiResponse<List<CategoryModel>>> GetCategoriesByTypeAsync(string type);
    Task<ApiResponse<List<AssetCategoryModel>>> GetAssetCategoriesAsync();
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpMethod method, string endpoint, object? data = null, string? token = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, endpoint);

            // Log the full URL for debugging
            var fullUrl = new Uri(_httpClient.BaseAddress!, endpoint).ToString();
            Console.WriteLine($"[API Call] {method} {fullUrl}");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                Console.WriteLine($"[API Body] {json}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[API Response] Status: {response.StatusCode}");
            Console.WriteLine($"[API Response] Body: {content}");

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiResponse<T>>(content) ?? new ApiResponse<T> { Success = false };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"API Error: {response.StatusCode} - {content}"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API Exception] {ex.Message}");
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Request failed: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserModel>> LoginAsync(LoginRequest request)
        => await SendRequestAsync<UserModel>(HttpMethod.Post, "/Auth/login", request);

    public async Task<ApiResponse<object>> RegisterAsync(RegisterRequest request)
        => await SendRequestAsync<object>(HttpMethod.Post, "/Auth/register", request);

    public async Task<ApiResponse<UserModel>> GetProfileAsync(string token)
        => await SendRequestAsync<UserModel>(HttpMethod.Get, "/Auth/profile", null, token);

    public async Task<ApiResponse<DashboardStats>> GetDashboardStatsAsync(string token)
        => await SendRequestAsync<DashboardStats>(HttpMethod.Get, "/Dashboard/stats", null, token);

    public async Task<ApiResponse<List<RecentTransaction>>> GetRecentTransactionsAsync(string token)
        => await SendRequestAsync<List<RecentTransaction>>(HttpMethod.Get, "/Dashboard/recent-transactions", null, token);

    public async Task<ApiResponse<List<IncomeModel>>> GetIncomesAsync(string token)
        => await SendRequestAsync<List<IncomeModel>>(HttpMethod.Get, "/Income", null, token);

    public async Task<ApiResponse<object>> CreateIncomeAsync(IncomeRequest request, string token)
        => await SendRequestAsync<object>(HttpMethod.Post, "/Income", request, token);

    public async Task<ApiResponse<object>> DeleteIncomeAsync(int id, string token)
        => await SendRequestAsync<object>(HttpMethod.Delete, $"/Income/{id}", null, token);

    public async Task<ApiResponse<List<ExpenseModel>>> GetExpensesAsync(string token)
        => await SendRequestAsync<List<ExpenseModel>>(HttpMethod.Get, "/Expense", null, token);

    public async Task<ApiResponse<object>> CreateExpenseAsync(ExpenseRequest request, string token)
        => await SendRequestAsync<object>(HttpMethod.Post, "/Expense", request, token);

    public async Task<ApiResponse<object>> DeleteExpenseAsync(int id, string token)
        => await SendRequestAsync<object>(HttpMethod.Delete, $"/Expense/{id}", null, token);

    public async Task<ApiResponse<List<AssetModel>>> GetAssetsAsync(string token)
        => await SendRequestAsync<List<AssetModel>>(HttpMethod.Get, "/Asset", null, token);

    public async Task<ApiResponse<object>> CreateAssetAsync(AssetRequest request, string token)
        => await SendRequestAsync<object>(HttpMethod.Post, "/Asset", request, token);

    public async Task<ApiResponse<object>> DeleteAssetAsync(int id, string token)
        => await SendRequestAsync<object>(HttpMethod.Delete, $"/Asset/{id}", null, token);

    public async Task<ApiResponse<ZakathResult>> CalculateZakathAsync(CalculateZakathRequest request, string token)
        => await SendRequestAsync<ZakathResult>(HttpMethod.Post, "/Zakath/calculate", request, token);

    public async Task<ApiResponse<List<ZakathResult>>> GetZakathHistoryAsync(string token)
        => await SendRequestAsync<List<ZakathResult>>(HttpMethod.Get, "/Zakath/history", null, token);

    public async Task<ApiResponse<List<PaymentModel>>> GetPaymentsAsync(string token)
        => await SendRequestAsync<List<PaymentModel>>(HttpMethod.Get, "/Payment", null, token);

    public async Task<ApiResponse<object>> CreatePaymentAsync(PaymentRequest request, string token)
        => await SendRequestAsync<object>(HttpMethod.Post, "/Payment", request, token);

    public async Task<ApiResponse<object>> DeletePaymentAsync(int id, string token)
        => await SendRequestAsync<object>(HttpMethod.Delete, $"/Payment/{id}", null, token);

    public async Task<ApiResponse<List<CategoryModel>>> GetCategoriesByTypeAsync(string type)
        => await SendRequestAsync<List<CategoryModel>>(HttpMethod.Get, $"/Category/{type}");

    public async Task<ApiResponse<List<AssetCategoryModel>>> GetAssetCategoriesAsync()
        => await SendRequestAsync<List<AssetCategoryModel>>(HttpMethod.Get, "/AssetCategory");
}
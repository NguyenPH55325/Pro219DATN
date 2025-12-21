using Pro219.Web.Constants;
using Pro219.Web.DTOs;

namespace Pro219.Web.Services
{
    public class StatisticalService
    {
        private readonly HttpClient _httpClient;

        public StatisticalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<ReportResponseDTO>> GetStatistics(DateTime? startDate, DateTime? endDate, string splitData = "month")
        {
            var queryString = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&splitData={splitData}";
            var url = "/Report/Statistics" + queryString;

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ReportResponseDTO>();
                    return ServiceResult<ReportResponseDTO>.Success(result);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var errorCode = result;

                    var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;

                    return ServiceResult<ReportResponseDTO>.Failure(result, errorMess, response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<ReportResponseDTO>.Failure("Exception", ex.Message, "500");
            }
        }
    }
}

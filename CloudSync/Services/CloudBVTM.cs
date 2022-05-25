using CQDT.CloudClient;
using HSDT.Common;
using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudSync.Services
{
    public class CloudBVTM: ICloud
    {
        private static ILogger logger = NLogger.GetLogger("CloudBVTM");

        /// <summary>
        /// Gửi dữ liệu object theo phương thức POST.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="api"></param>
        /// <param name="body"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(string api, object body, string token)
             where T : class, new()
        {
            var vResult = await PostObject(api, body, token);
            var vJson = JsonConvert.SerializeObject(vResult);
            return JsonConvert.DeserializeObject<T>(vJson);            
        }

        /// <summary>
        /// Gửi dữ liệu object theo phương thức POST.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="body"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<object> PostObject(string api, object body, string token)
        {
            try
            {
                var vResult = await body.PostAsJson(api, token);
                return vResult;
            }
            catch (UnauthorizedAccessException ex)
            {
                var json = JsonConvert.SerializeObject(body);
                logger.Error(ex, $"Không có quyền truy cập hoặc không đúng giấy phép: {api}|{json}");
                throw;
            }
            catch (HttpRequestException ex)
            {
                var json = JsonConvert.SerializeObject(body);
                logger.Error(ex, $"Không gửi được dữ liệu, kiểm tra các chi tiết trường thông tin và api: {api}|{json}");
                throw;
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(body);
                logger.Error(ex, $"Không gửi được dữ liệu, kiểm tra các chi tiết trường thông tin và api: {api}|{json}");
                throw;
            }
        }

        /// <summary>
        /// Hàm nạp danh sách dữ liệu từ Cloud theo phương thức GET.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="domain"></param>
        /// <param name="api"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<T> Get<T>(string api, string query, string token)
            where T : class, new()
        {
            try
            {
                var request = $"{api}?{query}";
                var result2 = await request.GetAsJson<T>(token);
                return result2;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"Không có quyền truy cập hoặc không đúng giấy phép: {api}?{query}");
                throw;
            }
            catch (HttpRequestException ex)
            {
                logger.Error(ex, $"Get dữ liệu thất bại: {api}?{query}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Get dữ liệu thất bại: {api}?{query}");
                throw;
            }

        }
    }
}

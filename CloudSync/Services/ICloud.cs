using System.Threading.Tasks;

namespace CloudSync.Services
{
    public interface ICloud
    {
        Task<T> Get<T>(string api, string query, string token)
            where T : class, new();

        Task<T> Post<T>(string api, object body, string token)
         where T : class, new();

        Task<object> PostObject(string api, object body, string token);
    }
}

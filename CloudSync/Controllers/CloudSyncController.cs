using CloudSync.Services;
using HSDT.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudSync.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudSyncController : ControllerBase
    {
        private readonly ILogger<CloudSyncController> _logger;
        private readonly ICloud _svCloud;

        public CloudSyncController(ILogger<CloudSyncController> logger, ICloud svCloud)
        {
            _logger = logger;
            _svCloud = svCloud;
        }

        /// <summary>
        /// Lấy token truyền lên
        /// </summary>
        protected string Token
        {
            get
            {
                if (this.Request.Headers.TryGetValue("token", out var tokenFromHeader)
                    || this.Request.Headers.TryGetValue("Authorization", out tokenFromHeader))
                {
                    return tokenFromHeader;
                }
                else if (this.Request.Query.TryGetValue("token", out var tokenFromQuery))
                {
                    return tokenFromQuery;
                }
                else
                {
                    throw new Exception("Không có thông tin giấy phép!");
                }
            }
        }

        #region Core API Functions
        /// <summary>
        /// Hàm gửi dữ liệu
        /// </summary>
        /// <param name="api"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{api}")]
        public async Task<IActionResult> PostBody(string api, [FromBody] object body)
        {
            var vResult = await _svCloud.PostObject(api, body, Token);
            this.Log($"Post {api} ok!");
            return Ok(vResult);
        }


        /// <summary>
        /// Hàm lấy dữ liệu
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{api}")]
        public async Task<IActionResult> GetQuery(string api)
        {
            var vQuery = this.Request.QueryString.Value;
            var vResult = await _svCloud.Get<object>(api, vQuery, Token);
            this.Log($"Get {api} ok!");
            return Ok(vResult);
        }
        #endregion
    }
}

using CQDT.CloudClient;
using HSDT.Common.Exceptions;
using HSDT.Common;
using Newtonsoft.Json;
using RestSharp;
using System;
using HSDT.AutoSync.Process;
using System.Threading.Tasks;
using HSDT.Common.Helper;

namespace CloudSync.Process
{
    public class GetPhieuDonTiepPubSub: QueueJobProcess<object>
    {
        private string latestMessageToken = string.Empty;

        /// <summary>
        /// - Khởi tạo sub, lắng nghe lấy phiếu đón tiếp.
        /// - Chờ nhận tín hiệu tử cloud.
        /// </summary>
        protected override void PostInitialize()
        {
            var authCloudToken = Config.FindValue("CloudToken");
            var subChannel = Config.GetValue("Channel", "lichkham/pending");
            var infoToken = authCloudToken.Substring(0, 6);
            if (!PubSubHelper.Initialize(authCloudToken).Wait(10000))
            {
                logger.Error($"Không thể khởi tạo Pubsub => Không thể chạy! Vui lòng kiểm tra, token={infoToken}...");
                return;
            }
            logger.Info($"PubSubHelper.Initialize ok!: token={infoToken}...");
            var subResult = PubSubHelper.Subcribe(subChannel, (info, token) =>
            {
                logger.Info($"0.0. Raised event & update new token, sub={info},subChannel={subChannel}");
                this.latestMessageToken = token;
                // Alert scanner!
                this.Alert();
                return Task.FromResult(true);
            }, (subErr) =>
            {
                if (subErr is Exception)
                {
                    logger.Error("Please check subError!", subErr);
                }
                else
                {
                    logger.Info($"Listening... subErr={subErr},subChannel={subChannel}");
                }
                return true;
            });
            logger.Info($"subResult={subResult},subChannel={subChannel}!");
        }

        /// <summary>
        /// - Kéo các kết quả phiếu đón tiếp từ Cloud 
        /// - Đẩy vào queue chờ xử lý.
        /// </summary>
        protected override async void Scan()
        {
            if (this.latestMessageToken == string.Empty)
            {
                logger.Info($"Token is empty, wait for next scan()!");
                return;
            }
            var apiCloudGetUrl = Config.FindValue("CloudGetUrl");
            if (apiCloudGetUrl.IsNullOrEmpty())
            {
                logger.Error($"ApiGetUrl: is missing!");
                return;
            }

            //copy token value to local
            var token = this.latestMessageToken;
            try
            {
                logger.Info("0.1. Pulling ketqua phieu don tiep...");
                var authToken = Config.FindValue("CloudToken");
                var vListResult = await $"{apiCloudGetUrl}?msgToken={token}".GetAsJson<object>(authToken);
                if (vListResult != null)
                {
                    logger.Info($"1.1. Pulled new result: data={JsonConvert.SerializeObject(vListResult)}");
                    this.Enqueue(vListResult);
                    this.Notify(); // Notify worker to process data!
                }
                else
                {
                    logger.Info($"1.1. No result! token={token}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "1.3. Fail pulling result, " + ex.Message);
            }
            finally
            {
                if (this.latestMessageToken == token)
                {
                    // clear token if not new one
                    logger.Info("Clean token and wait for new one!");
                    this.latestMessageToken = string.Empty;
                }
            }
        }

        /// <summary>
        /// - Lấy dữ liệu tuần tự từ Queue.
        /// - Gửi dữ liệu về HIS.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="HSCoreException"></exception>
        protected override void Process(object data)
        {
            var vApiUrl = Config.GetValue<string>("ApiHisUrl")
                ?? throw new HSCoreException("Chưa có cấu hình ApiHisUrl", HSCoreError.ERROR_INVALID_PARAMETER);
            var vApiPostPhieuDonTiep = Config.GetValue<string>("ApiPostPhieuDonTiep")
                ?? throw new HSCoreException("ApiPostPhieuDonTiep: is mising.", HSCoreError.ERROR_INVALID_PARAMETER);
            var vTokenKey = Config.GetValue("ApiHisToken", "");
            var client = new RestClient(vApiUrl);
            // Gửi kết quả về HIS.
            var vSendHisRequest = new RestRequest(vApiPostPhieuDonTiep, Method.Post)
            .AddHeader("Authorization", vTokenKey)
            .AddJsonBody(new
            {
                data,
            });
            // Send request
            var vUpdateTaskResponse = client.ExecuteAsync<string>(vSendHisRequest).Result;
            if (vUpdateTaskResponse.IsSuccessful)
            {
                logger.Info($"2. Gửi cập nhật kết quả phiếu khám thành công, result={vUpdateTaskResponse.Content}");
            }
            else
            {
                logger.Error($"2. Gửi cập nhật trạng thái thanh toán phiếu khám về HIS thất bại, error={vUpdateTaskResponse.Content}, data={JsonConvert.SerializeObject(data)}");
            }
        }
    }
}

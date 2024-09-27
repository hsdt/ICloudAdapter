using CQDT.CloudClient;
using HSDT.Common.Exceptions;
using Newtonsoft.Json;
using RestSharp;
using System;
using HSDT.AutoSync.Process;
using System.Threading.Tasks;
using HSDT.Common.Helper;

namespace CloudSync.Process
{
    public class GetKetQuaKCBPubSub : QueueJobProcess<object>
    {
        private string latestMessageToken = string.Empty;

        /// <summary>
        /// - Khởi tạo sub, lắng nghe lấy phiếu đón tiếp.
        /// - Chờ nhận tín hiệu tử cloud.
        /// </summary>
        protected override void PostInitialize()
        {
            // TODO: Cập nhật cấu hình CloudToken vào file cấu hình Config/GetPhieuDonTiepPubSub.json
            var authCloudToken = Config.FindValue("CloudToken");
            var subChannel = Config.GetValue("Channel", "phieukhamkq/pending");
            var infoToken = authCloudToken?.Substring(0, 6) ?? "NULL";
            if (authCloudToken is null || !PubSubHelper.Initialize(authCloudToken).Wait(10000))
            {
                logger.Error($"Cannot init Pubsub => Please verify CloudToken, token={infoToken}...");
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
                logger.Error($"CloudGetUrl: is missing!");
                return;
            }

            //copy token value to local
            var token = this.latestMessageToken;
            try
            {
                logger.Info("0.1. Pulling ketqua kcb...");
                var authToken = Config.FindValue("CloudToken");
                var vLane = Config.GetValue<string>("Lane");
                var apiMethodUrl = $"{apiCloudGetUrl}?msgToken={token}";
                if (vLane.IsNotNullOrEmpty())
                {
                    apiMethodUrl += $"&lane={vLane}";
                }
                var vListResult = await apiMethodUrl.GetAsJson<VMData[]>(authToken);
                if (vListResult != null)
                {
                    logger.Info($"1.1. Pulled new result: data={JsonConvert.SerializeObject(vListResult)}");
                    foreach (var d in vListResult)
                    {
                        d.XMLData = CQDT.CloudClient.StringHelper.Decompress(d.XMLData);
                    }
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
            if (data is null)
            {
                return;
            }
            // TODO: Cập nhật cấu hình gửi Lịch khám về HIS cho đúng.
            var vApiUrl = Config.GetValue<string>("ApiHisKqKCBUrl")
                ?? throw new HSCoreException("Chưa có cấu hình ApiHisKqKCBUrl", HSCoreError.ERROR_INVALID_PARAMETER);
            var vApiPostKQKCB = Config.GetValue<string>("ApiPostKetQuaKCB")
                ?? throw new HSCoreException("ApiPostKqKCB: is mising.", HSCoreError.ERROR_INVALID_PARAMETER);
            var vTokenKey = Config.GetValue("ApiHisToken", "");
            var client = new RestClient(vApiUrl);
            // Gửi kết quả về HIS.
            var vRequest = new RestRequest(vApiPostKQKCB, Method.Post)
                .AddJsonBody(data);
            if (vTokenKey.IsNotNullOrEmpty())
            {
                vRequest.AddHeader("ApiKey", vTokenKey);
            }
            // Send request
            var vResponse = client.ExecuteAsync<string>(vRequest).Result;
            if (vResponse.IsSuccessStatusCode)
            {
                logger.Info($"2. Gửi kết quả thành công,status={vResponse.StatusCode},result={vResponse.Content}");
            }
            else
            {
                logger.Error($"2. Gửi kết quả thất bại,status={vResponse.StatusCode},error={vResponse.Content}, data=...");
            }
        }
    }

    public class VMData
    {
        public Guid Id { get; set; }
        public string XMLData { get; set; }
        public string MaLanKham { get; set; }
        public string CCCD { get; set; }
    }
}

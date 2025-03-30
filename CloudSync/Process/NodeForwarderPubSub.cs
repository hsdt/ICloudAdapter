using CQDT.CloudClient;
using HSDT.Common.Exceptions;
using HSDT.Common;
using Newtonsoft.Json;
using RestSharp;
using System;
using HSDT.AutoSync.Process;
using System.Threading.Tasks;
using HSDT.Common.Helper;
using CloudSync.Process.Dto;

namespace CloudSync.Process
{
    public class NodeForwarderPubSub : QueueJobProcess<TopicSubResult>
    {
        private string latestMessageToken = string.Empty;

        /// <summary>
        /// - Khởi tạo sub, lắng nghe các sự kiện Pub/sub.
        /// - Chờ nhận tín hiệu tử cloud.
        /// </summary>
        protected override void PostInitialize()
        {
            var authToken = Config.FindValue("CloudToken");
            //var subChannel = Config.FindValue("Channel", "phieukhamkq/pending");
            var vNodeURL = Config.FindValue("NodeURL", "");
            var infoToken = authToken?.Substring(0, 6) ?? "NULL";
            var subRequests = Config.FindValue("Requests", "");
            if (subRequests.IsNullOrEmpty())
            {
                logger.Error("Pub/sub Requests chưa được cấu hình!");
                return;
            }
            if (authToken is null || !PubSubHelper.Initialize(authToken, vNodeURL).Wait(10000))
            {
                logger.Error($"Cannot init Pubsub => Please verify CloudToken, token={infoToken}...");
                return;
            }
            logger.Info($"PubSubHelper.Initialize ok!: token={infoToken}...");
            var vSubRequests = JsonConvert.DeserializeObject<TopicSubRequest[]>(subRequests);
            foreach (var item in vSubRequests)
            {
                var topic = item.Topic;
                logger.Info($"Starting sub topic: {item.Topic}, {item.ExecuteApi}, {item.ForwardUrl}");
                PubSubHelper.Subcribe(topic, async (subTopic, token) =>
                {
                    this.latestMessageToken = token;
                    // $"{item.ExecuteApi}&msgToken={token}";
                    var apiMethodUrl = item.ExecuteApi.Interpolate(new { token });
                    logger.Info($"0.0. Raised event, subTopic={subTopic}, apiMethodUrl={apiMethodUrl}...");
                    var vGetData = await apiMethodUrl.GetAsJson<object[]>(authToken, vNodeURL);
                    if (vGetData.Length > 0)
                    {
                        var vResult = new TopicSubResult();
                        vResult.Request = item;
                        vResult.Token = token;
                        vResult.Data = vGetData;
                        this.Enqueue(vResult);
                        // Alert scanner!
                        this.Alert();
                    }
                    return true;
                }, (subErr) =>
                {
                    if (subErr is Exception)
                    {
                        logger.Error("Please check subError!", subErr);
                    }
                    else
                    {
                        logger.Info($"Listening... subErr={subErr},subTopic={topic}");
                    }
                    return true;
                });
            }
            logger.Info($"Done sub topic length: {vSubRequests.Length}!");
        }

        /// <summary>
        /// - Kéo các kết quả phiếu đón tiếp từ Cloud 
        /// - Đẩy vào queue chờ xử lý.
        /// </summary>
        protected override void Scan()
        {
            // TODO: something
            logger.Info("Process still alive!");
            this.Notify();
        }

        /// <summary>
        /// - Lấy dữ liệu tuần tự từ Queue.
        /// - Gửi dữ liệu về HIS.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="HSCoreException"></exception>
        protected override void Process(TopicSubResult data)
        {
            if (data is null)
            {
                return;
            }
            var vForwardAuthKey = data.Request.ForwardAuthKey ?? Config.FindValue("ForwardAuthKey");
            var client = new RestClient(data.Request.ForwardUrl);
            // Gửi kết quả về HIS.
            var vRequest = new RestRequest(data.Request.ForwardUrl, Method.Post)
                .AddJsonBody(data.Data.ToString());
            if (vForwardAuthKey.IsNotNullOrEmpty())
            {
                vRequest.AddHeader("Authorization", vForwardAuthKey);
            }
            // Send request
            var vResponse = client.ExecuteAsync<string>(vRequest).Result;
            if (vResponse.IsSuccessStatusCode)
            {
                logger.Info($"2. Gửi kết quả thành công: url={data.Request.ForwardUrl},status={vResponse.StatusCode},result={vResponse.Content}");
            }
            else
            {
                logger.Error($"2. Gửi kết quả thất bại: url={data.Request.ForwardUrl},status={vResponse.StatusCode},error={vResponse.Content}");
            }
        }
    }
}

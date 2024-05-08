using CQDT.CloudClient;
using HSDT.AutoSync.Process;
using HSDT.Common;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System.Timers;
using Timers = System.Timers;

namespace AutoSync.Tasks
{
    public class PhieuDonTiepPubSub : CronJobProcess
    {
        private static Queue<LichKhamModelTransfer> _queue = new Queue<LichKhamModelTransfer>();
        private string _latestMessageToken = string.Empty;
        private static Timers.Timer _timer = new Timers.Timer();
        private static Timers.Timer _timerProcessData = new Timers.Timer();

        bool IsPulling = false;
        public override async void Initialize()
        {
            logger.Info("Khởi tạo CronJob, get lịch khám ");
            var cronTime = Config.GetValue<string>("JobInterval", CRON_EVERY_3MINUTES);
            this.AddJob(cronTime, ProcessGetLichKham);

            // Init pubsub
            var authToken = Config.GetValue<string>("AuthToken");
            await PubSubHelper.Initialize(authToken);
            logger.Info("Init PubSub");

            _timer.Interval = Config.GetValue<int>("Timer", 1000);
            _timer.Elapsed += TimerPulling;
            _timer.Start();

            _timerProcessData.Interval = Config.GetValue<int>("TimerProcess", 5000); ;
            _timerProcessData.Elapsed += TimerProcess;
            _timerProcessData.Start();
        }

        private async void TimerProcess(object sender, ElapsedEventArgs e)
        {
            if (_queue.TryDequeue(out LichKhamModelTransfer data))
            {
                logger.Info("2.0. Dequeue...");
                var vApiUrl = Config.GetValue<string>("ApiAutoSync", "http://localhost:5005/");
                var client = new RestClient(vApiUrl);
                var vRequestUrl = Config.GetValue("ImportLichKham", "LichKham");
                var vRequest = new RestRequest(vRequestUrl, method: Method.Post);
                logger.Info(JsonConvert.SerializeObject(data));
                vRequest.AddJsonBody(data);
                var vResponseTaskResult = await client.ExecuteAsync(vRequest);
                if (vResponseTaskResult.IsSuccessful)
                {
                    logger.Info($"2.1. insert lich kham done, Response={vResponseTaskResult.Content}");
                }
                else
                {
                    logger.Error($"2.2. import error, data {data.Id}, url: {vApiUrl}, request: {vRequestUrl}");
                }
            }
        }

        private async void TimerPulling(object sender, ElapsedEventArgs e)
        {
            if (_latestMessageToken != string.Empty && !this.IsPulling)
            {
                var token = _latestMessageToken;
                this.IsPulling = true; //hold to process
                try
                {

                    // 
                    var authToken = Config.GetValue<string>("AuthToken");

                    logger.Info("0.1. Pulling ketqua");

                    var ketqua = await $"chidinh-lichkham_list?msgToken={token}".GetAsJson<LichKhamModelTransfer[]>(authToken);
                    if (ketqua != null)
                    {
                        logger.Info("1.1. Pulled lichkham");
                        foreach (var item in ketqua)
                        {
                            _queue.Enqueue(item);
                            logger.Info($"1.2. Enqueuing ...");
                        }


                        logger.Debug("1.1. Data from cloud: " + JsonConvert.SerializeObject(ketqua));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "1.3. Fail TimerPulling_Tick, " + ex.Message);
                }
                finally
                {
                    this.IsPulling = false; //stop hold 
                    if (_latestMessageToken == token)
                    {
                        _latestMessageToken = string.Empty;// clear token if not new one   
                    }
                }
            }
        }

        public void ProcessGetLichKham()
        {
            try
            {
                // var delay = 1000;
                if (PubSubHelper.Stats() == null || PubSubHelper.Stats()?.Length <= 0)
                {
                    logger.Info("Subscribe now...");
                    PubSubHelper.Subcribe("lichkham/pending", async (sub, token) =>
                    {
                        logger.Info("0.0. Raised event LichKham");
                        _latestMessageToken = token;
                        return true;
                    }, (subErr) =>
                    {
                        logger.Debug($"Listening... {PubSubHelper.Stats()?.Length} subs");
                        return true;
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"3.1. Error in GetKetQuaDonThhuoc");
            }
        }
    }
}
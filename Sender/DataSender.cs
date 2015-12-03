using Newtonsoft.Json;
using RabotatAgent.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabotatAgent.Sender
{
    class DataSender
    {
        private MySettings Settings;
        private ConcurrentQueue<ActiveWindow> Queue;

        public DataSender(MySettings s, ref ConcurrentQueue<ActiveWindow> q) {
            Settings = s;
            Queue = q;
        }

        public string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private string CreatePacket(List<ActiveWindow> list) {
            var packet = new DataPacket();
            packet.UserName = Environment.UserName.ToLower();
            packet.DomainName = Environment.UserDomainName.ToLower();
            packet.HostName = Environment.MachineName.ToLower();
            packet.Windows = list;
            packet.Timestamp = DateTime.Now.ToUniversalTime();
            var str = packet.UserName +
                packet.DomainName +
                packet.HostName +
                packet.Timestamp.ToString(@"yyyy-MM-dd HH:mm:ss") +
                Settings.Secret;
            packet.Hash = CalculateMD5Hash(str);

            return JsonConvert.SerializeObject(packet);
        }

        private bool SendData(List<ActiveWindow> list)
        {
            var json = CreatePacket(list);

            var client = new HttpClient();
            HttpResponseMessage responce;
            try
            {
                responce = client.PostAsync(Settings.SubmitUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            }
            catch (Exception ex) {
                Debug.WriteLine("HttpClient exception: " + ex.Message);
                return false;
            }

            return (responce.StatusCode == HttpStatusCode.OK);
        }

        private void CreatePackets()
        {
            var list = new List<ActiveWindow>();
            var lastSend = DateTime.Now.ToUniversalTime();
            var maxDelay = new TimeSpan(0, 0, Settings.MaxSendDelay);
            for (;;)
            {
                Debug.WriteLine("List size: " + list.Count);
                // не пора ли нам отправить данные?
                if (list.Count > 0 
                    && (list.Count >= Settings.MaxSendSize || DateTime.Now.ToUniversalTime() - lastSend >= maxDelay))
                {
                    Debug.WriteLine("Time to send datas");
                    if (SendData(list))
                    {
                        list.Clear();
                        lastSend = DateTime.Now.ToUniversalTime();
                    }
                    else {
                        Thread.Sleep(Settings.StepDelay *60);
                    }
                }

                // если очередь пуста, то можно и поспать
                if (Queue.IsEmpty)
                {
                    Debug.WriteLine("Queue is emply - sleep");
                    Thread.Sleep(Settings.StepDelay);
                }

                // если очередь не пуста и у нас не собран пакет на отправку
                while (!Queue.IsEmpty && list.Count < Settings.MaxSendSize)
                {
                    ActiveWindow foo = null;
                    if (Queue.TryDequeue(out foo))
                    {
                        Debug.WriteLine("Dequeued element");
                        list.Add(foo);
                    };
                }
            }
        }

        public Task StartTask()
        {
            return Task.Factory.StartNew(CreatePackets);
        }
    }
}

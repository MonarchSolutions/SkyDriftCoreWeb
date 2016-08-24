using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SkyDriftCoreWeb
{
    //[Serializable] is not available in .NET Core. Use Json instead.
    public class Config
    {
        /// <summary>
        /// 服务器欢迎语，建议为服务器名称
        /// </summary>
        public string ServerWelcome = "欢迎来到幻走服务器【幻想乡】";
        /// <summary>
        /// 服务器消息
        /// </summary>
        public string ServerNews = "By Ulysses";
        /// <summary>
        /// 上传成绩的最小间隔（秒）
        /// </summary>
        public double MinimumBreakTime = 60.0;
        /// <summary>
        /// 最大房间生存时间（小时）
        /// </summary>
        public double MaximumRoomTime = 4.0;
        /// <summary>
        /// 最大用户挂机时间（小时）
        /// </summary>
        public double MaximumUserOnlineTime = 3.0;
        /// <summary>
        /// 匹配重试次数
        /// </summary>
        public int MatchTime = 3;
        /// <summary>
        /// 提供匹配服务
        /// </summary>
        public bool MatchAvailable = true;
        /// <summary>
        /// 当前最新版本
        /// </summary>
        public string NewestVersion = "1603";
        
        /// <summary>
        /// 监听地址
        /// </summary>
        public string[] ListenUrls = new[] { "http://localhost:8080/" };

        /// <summary>
        /// 测试用用户
        /// </summary>
        public int[] TestUsers = new[] {1, 2};

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var sw = new StreamWriter(fs))
            {
                JsonSerializer js = new JsonSerializer();
                js.Serialize(sw, this);
                sw.Flush();
            }
        }

        public static Config Load(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        var config =
                            JsonSerializer.Create().Deserialize<Config>(new JsonTextReader(new StreamReader(fs)));
                        return config;
                    }

                }
                catch (Exception)
                {
                    return new Config();
                }
            }
            return new Config();
        }
    }
}

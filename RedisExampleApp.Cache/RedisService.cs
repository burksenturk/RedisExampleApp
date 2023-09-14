using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisExampleApp.Cache
{
    public class RedisService
    {
        //singleton nesne oluşturucaz
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RedisService(string url)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(url);  //url'i appsetting içerisinden okuyoruz.
        }

        public IDatabase GetDb(int dbIndex)  // hangi db ye bağlanacaksa onu belirtiyoruz
        {
            return _connectionMultiplexer.GetDatabase(dbIndex);
        }
    }
}

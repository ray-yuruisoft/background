using System;
using background.Models;
using DapperWrapper;
namespace background.Data
{
    public class simpleData:DapperBase<simpleModel>
    {
        public simpleData()
        {
        }
        public override string connectionString => "server=localhost;database=mj_log;uid=root;pwd=admin123456;charset=utf8";
        public override string tableName { get ; set ; }
    }
}

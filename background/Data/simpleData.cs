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
        public override string connectionString => "office_localhost_mjlog";
        public override string tableName { get ; set ; }
    }
}

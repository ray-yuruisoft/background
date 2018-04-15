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
        public override string connectionString => "home_mjlog";
        public override string tableName { get ; set ; }
    }
}

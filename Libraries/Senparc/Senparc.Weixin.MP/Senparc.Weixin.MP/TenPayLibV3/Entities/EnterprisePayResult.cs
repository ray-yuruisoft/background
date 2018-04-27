using System;

namespace Senparc.Weixin.MP.TenPayLibV3
{
    public class EnterprisePayResult
    {


        public bool return_code { get; set; }
        public string return_msg { get; set; }
        public string mch_appid { get; set; }

        public string mchid { get; set; }

        public string device_info { get; set; }

        public string nonce_str { get; set; }

        public bool result_code { get; set; }

        public string err_code { get; set; }

        public string err_code_des { get; set; }

        public string partner_trade_no { get; set; }
        public string payment_no { get; set; }
        public string payment_time { get; set; }

    }
}

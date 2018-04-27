using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace background.Service
{
    [ServiceContract]
    public interface ICalculatorService
    {
        [OperationContract]
        double Add(double x, double y);
        [OperationContract]
        double Subtract(double x, double y);
        [OperationContract]
        double Multiply(double x, double y);
        [OperationContract]
        double Divide(double x, double y);

        [OperationContract]
        string Get(string str);
    }
}

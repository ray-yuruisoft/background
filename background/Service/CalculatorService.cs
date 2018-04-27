using System;
using System.Collections.Generic;
using System.Text;

namespace background.Service
{
    public class CalculatorService : ICalculatorService
    {
        public double Add(double x, double y) => x + y;
        public double Divide(double x, double y) => x / y;
        public double Multiply(double x, double y) => x * y;
        public double Subtract(double x, double y) => x - y;
        public string Get(string str) => $"{str} Hello World!";
    }
}

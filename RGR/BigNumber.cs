using System;
using System.Numerics;

namespace RGR
{
    public class BigNumber : IComparable<BigNumber>
    {
        public bool IsNegative { get; private set; }
        public BigInteger IntegerPart { get; private set; }
        public string FractionalPart { get; private set; }

        // Конструктори
        public BigNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Empty number");

            value = value.Trim();
            if (value[0] == '-')
            {
                IsNegative = true;
                value = value.Substring(1);
            }

            string[] parts = value.Split('.');
            IntegerPart = BigInteger.Parse(parts[0]);
            FractionalPart = parts.Length > 1 ? parts[1] : "";

            Normalize();
        }

        public BigNumber(BigInteger integerPart, string fractionalPart, bool isNegative)
        {
            IntegerPart = integerPart;
            FractionalPart = fractionalPart ?? "";
            IsNegative = isNegative;
            Normalize();
        }

        public BigNumber(BigInteger value)
        {
            IsNegative = value < 0;
            IntegerPart = BigInteger.Abs(value);
            FractionalPart = "";
        }

        public BigNumber(double value)
        {
            string str = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BigNumber temp = new BigNumber(str);
            IntegerPart = temp.IntegerPart;
            FractionalPart = temp.FractionalPart;
            IsNegative = temp.IsNegative;
        }

        private void Normalize()
        {
            if (IntegerPart == 0 && FractionalPart.Length > 0)
            {
                bool allZeros = true;
                foreach (char c in FractionalPart)
                    if (c != '0') { allZeros = false; break; }
                if (allZeros) FractionalPart = "";
            }

            if (IntegerPart == 0 && string.IsNullOrEmpty(FractionalPart))
                IsNegative = false;
        }

        public override string ToString()
        {
            string fractional = string.IsNullOrEmpty(FractionalPart) ? "" : "." + FractionalPart;
            string sign = IsNegative ? "-" : "";
            return sign + IntegerPart.ToString() + fractional;
        }

        // Додавання
        public static BigNumber Add(BigNumber a, BigNumber b)
        {
            if (a.IsNegative != b.IsNegative)
                return Subtract(a, new BigNumber(b.IntegerPart, b.FractionalPart, !b.IsNegative));

            string fracA = a.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');
            string fracB = b.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');

            BigInteger intA = a.IntegerPart;
            BigInteger intB = b.IntegerPart;
            BigInteger fracSum = 0;
            int fracLength = fracA.Length;

            if (fracLength > 0)
            {
                fracSum = BigInteger.Parse(fracA) + BigInteger.Parse(fracB);
                if (fracSum.ToString().Length > fracLength)
                {
                    string fracSumStr = fracSum.ToString();
                    fracSum = BigInteger.Parse(fracSumStr.Substring(1));
                    intA++;
                }
            }

            BigInteger intSum = intA + intB;
            string fracResult = fracSum.ToString().PadLeft(fracLength, '0');
            if (fracResult.Length > fracLength)
                fracResult = fracResult.Substring(1);

            return new BigNumber(intSum, fracResult, a.IsNegative);
        }

        // Віднімання
        public static BigNumber Subtract(BigNumber a, BigNumber b)
        {
            if (a.IsNegative != b.IsNegative)
                return Add(a, new BigNumber(b.IntegerPart, b.FractionalPart, !b.IsNegative));

            bool resultNegative = false;
            if (CompareAbsolute(a, b) < 0)
            {
                resultNegative = !a.IsNegative;
                BigNumber temp = a;
                a = b;
                b = temp;
            }

            string fracA = a.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');
            string fracB = b.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');

            BigInteger intA = a.IntegerPart;
            BigInteger intB = b.IntegerPart;
            BigInteger fracDiff = 0;
            int fracLength = fracA.Length;

            if (fracLength > 0)
            {
                fracDiff = BigInteger.Parse(fracA) - BigInteger.Parse(fracB);
                if (fracDiff < 0)
                {
                    fracDiff += BigInteger.Pow(10, fracLength);
                    intA--;
                }
            }

            BigInteger intDiff = intA - intB;
            string fracResult = fracDiff.ToString().PadLeft(fracLength, '0');

            return new BigNumber(intDiff, fracResult, resultNegative);
        }

        // Множення
        public static BigNumber Multiply(BigNumber a, BigNumber b)
        {
            string strA = a.IntegerPart.ToString() + a.FractionalPart;
            string strB = b.IntegerPart.ToString() + b.FractionalPart;
            int decimalPlaces = a.FractionalPart.Length + b.FractionalPart.Length;

            BigInteger product = BigInteger.Parse(strA) * BigInteger.Parse(strB);
            string productStr = product.ToString();

            if (decimalPlaces > 0)
            {
                if (productStr.Length <= decimalPlaces)
                    productStr = productStr.PadLeft(decimalPlaces + 1, '0');

                string intPart = productStr.Substring(0, productStr.Length - decimalPlaces);
                string fracPart = productStr.Substring(productStr.Length - decimalPlaces).TrimEnd('0');

                if (string.IsNullOrEmpty(intPart)) intPart = "0";
                return new BigNumber(intPart, fracPart, a.IsNegative != b.IsNegative);
            }

            return new BigNumber(productStr, "", a.IsNegative != b.IsNegative);
        }

        // Ділення (спрощена версія)
        public static BigNumber Divide(BigNumber a, BigNumber b, int precision = 50)
        {
            if (b.IntegerPart == 0 && string.IsNullOrEmpty(b.FractionalPart))
                throw new DivideByZeroException();

          
            double valA = double.Parse(a.ToString());
            double valB = double.Parse(b.ToString());
            double result = valA / valB;

            return new BigNumber(result.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private static int CompareAbsolute(BigNumber a, BigNumber b)
        {
            int compareInt = a.IntegerPart.CompareTo(b.IntegerPart);
            if (compareInt != 0) return compareInt;

            string fracA = a.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');
            string fracB = b.FractionalPart.PadRight(Math.Max(a.FractionalPart.Length, b.FractionalPart.Length), '0');
            return fracA.CompareTo(fracB);
        }

        public int CompareTo(BigNumber other)
        {
            if (IsNegative != other.IsNegative)
                return IsNegative ? -1 : 1;
            return CompareAbsolute(this, other) * (IsNegative ? -1 : 1);
        }

        // Оператори
        public static BigNumber operator +(BigNumber a, BigNumber b) => Add(a, b);
        public static BigNumber operator -(BigNumber a, BigNumber b) => Subtract(a, b);
        public static BigNumber operator *(BigNumber a, BigNumber b) => Multiply(a, b);
        public static BigNumber operator /(BigNumber a, BigNumber b) => Divide(a, b);
    }
}
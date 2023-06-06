using System;
using System.Numerics;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ShamirSecretSharing.Data
{
    internal static class ShamirSecretProtocol
    {
        // Формирование частей секрета
        public static List<Tuple<int, BigInteger>> ShareSecret(BigInteger secret, int threshold, int numShares, BigInteger prime)
        {
            List<Tuple<int, BigInteger>> shares = new List<Tuple<int, BigInteger>>();

            BigInteger[] coefficients = new BigInteger[numShares];
            coefficients[0] = secret;

            for (int i = 1; i < numShares; i++)
                coefficients[i] = GenerateRandomNumber(prime);

            for (int i = 0; i < numShares; i++)
            {
                BigInteger x = new BigInteger(i + 1);
                BigInteger y = 0;

                for (int j = 0; j < threshold; j++)
                    y += coefficients[j] * BigInteger.Pow(x, j) % prime;

                shares.Add(new Tuple<int, BigInteger>(i + 1, y));
            }

            return shares;
        }

        // Генерация случайных чисел
        private static BigInteger GenerateRandomNumber(BigInteger max)
        {
            byte[] bytes = max.ToByteArray();
            BigInteger number;

            using (RandomNumberGenerator rand = RandomNumberGenerator.Create())
            {
                do
                {
                    rand.GetBytes(bytes);
                    bytes[bytes.Length - 1] &= (byte)0x7F;
                    number = new BigInteger(bytes);
                }
                while (number >= max && number == 0);
            }

            return number;
        }

        // Восстановление секрета
        public static BigInteger RecoverSecret(List<Tuple<int, BigInteger>> shares, BigInteger prime, int threshold)
        {
            BigInteger secret = 0;

            for (int i = 0; i < threshold; i++)
            {
                BigInteger xi = shares[i].Item1;
                BigInteger yi = shares[i].Item2;

                BigInteger numerator = 1;
                BigInteger denominator = 1;

                for (int j = 0; j < threshold; j++)
                {
                    if (i != j)
                    {
                        BigInteger xj = shares[j].Item1;

                        numerator *= -xj;
                        denominator *= (xi - xj);
                    }
                }

                BigInteger inverseDenominator = InverseModule(denominator, prime);
                secret += yi * numerator * inverseDenominator;
            }

            secret %= prime;

            return secret < 0 ? secret + prime : secret;
        }

        // Расширенный алгоритм Евклида
        private static BigInteger InverseModule(BigInteger a, BigInteger m)
        {
            if (a < 0) a += m;

            if (m == 1)
                return 0;

            BigInteger m0 = m;
            BigInteger t, q;
            BigInteger x0 = 0, x1 = 1;

            while (a > 1)
            {
                q = a / m;
                t = m;
                m = a % m;
                a = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }
    }
}

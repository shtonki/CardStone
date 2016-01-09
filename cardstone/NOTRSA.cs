using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

/// <summary>
/// Definately not RSA
/// </summary>
abstract class RSA
{
    private static RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();


    public static void generateKeyPairs(int bytes, string filepath)
    {
        BigInteger p = rprime(bytes);
        var fs = UIF(p - 1);
        var qs = primeRoots(p, fs);

        StreamWriter writer = new StreamWriter(filepath);


        writer.WriteLine(p);

        foreach (var v in qs)
        {
            writer.WriteLine(v);
        }
    }

    public static BigInteger getPublic(BigInteger p, BigInteger g, BigInteger s)
    {
        return BigInteger.ModPow(g, s, p);
    }

    public static BigInteger getSecret(BigInteger p, BigInteger s, BigInteger o)
    {
        return BigInteger.ModPow(o, s, p);
    }


    private static BigInteger rbigint(int bytes)
    {
        byte[] bs = new byte[bytes + 1];
        r.GetNonZeroBytes(bs);
        bs[bytes] = 00; //forces it to be unsigned, brahman knows how
        return new BigInteger(bs);
    }

    private static BigInteger rprime(int bytes)
    {
        BigInteger r;

        do
        {
            r = rbigint(bytes);
        } while (!(isProbablyPrime(r, 10)));

        return r;
    }

    private static List<BigInteger> primeRoots(BigInteger prime, List<BigInteger> fs)
    {
        List<BigInteger> r = new List<BigInteger>();

        BigInteger s = prime - 1;

        BigInteger lpg = 2;
        while (lpg < prime)
        {
            if (tst(lpg, fs, prime))
            {
                r.Add(lpg);
                break;
            }
            lpg++;
        }

        //return r;

        for (int i = 2; i < prime - 1; i++)
        {
            if (BigInteger.GreatestCommonDivisor(i, s) == 1)
            {
                r.Add(BigInteger.ModPow(lpg, i, prime));
            }
        }

        return r;
    }

    private static bool tst(BigInteger baze, List<BigInteger> es, BigInteger p)
    {
        foreach (var v in es)
        {
            BigInteger q = BigInteger.ModPow(baze, v, p);
            if (q == 1) { return false; }
        }
        return true;
    }

    public static List<BigInteger> UIF(BigInteger b)
    {
        BigInteger o = b;
        List<BigInteger> l = new List<BigInteger>();
        BigInteger c = 2;
        while (b > 1)
        {
            if (BigInteger.ModPow(b, 1, c) == 0)
            {
                l.Add(c);
                do
                {
                    b = b/c;
                } while (BigInteger.ModPow(b, 1, c) == 0);
            }
            else
            {
                c++;
            }
        }

        return l.Select(integer => o/integer).ToList();
    }

    private static bool isProbablyPrime(BigInteger src, int c)
    {
        if (src == 2 || src == 3)
            return true;
        if (src < 2 || src % 2 == 0)
            return false;

        BigInteger d = src - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        BigInteger a;

        for (int i = 0; i < c; i++)
        {
            do
            {
                a = rbigint(src.ToByteArray().Length);
            }
            while (a < 2 || a >= src - 2);

            BigInteger x = BigInteger.ModPow(a, d, src);
            if (x == 1 || x == src - 1)
                continue;

            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, src);
                if (x == 1) { return false; }
                if (x == src - 1) { break; }
            }

            if (x != src - 1) { return false; }
        }

        return true;
    }
}
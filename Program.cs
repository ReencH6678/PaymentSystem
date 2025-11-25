using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Payment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int secretKey = 777;

            Hasher hasher = Hasher.Create(new IdHasher());
            Hasher hasher1 = Hasher.Create(new IdHasher(), new AmountHasher());
            Hasher hasher2 = Hasher.Create(new IdHasher(), new AmountHasher(), new SecretHasрer(secretKey));

            PaymentSystem paymentSystem = new PaymentSystem("pay.system1.ru/order?amount=12000RUB&hash={}", hasher);
            PaymentSystem paymentSystem1 = new PaymentSystem("order.system2.ru/pay?hash={}", hasher1);
            PaymentSystem paymentSystem2 = new PaymentSystem("system3.com/pay?amount=12000&curency=RUB&hash={}", hasher2);

            Order order = new Order(143, 123);

            Console.WriteLine(paymentSystem.GetPayingLink(order));
            Console.WriteLine(paymentSystem1.GetPayingLink(order));
            Console.WriteLine(paymentSystem2.GetPayingLink(order));
        }
    }
}

public class Order
{
    public readonly int Id;
    public readonly int Amount;

    public Order(int id, int amount) => (Id, Amount) = (id, amount);
}

public interface IPaymentSystem
{
    string GetPayingLink(Order order);
}

public interface IHasher
{
    string GetOrderHash(Order order);
}

public class PaymentSystem : IPaymentSystem
{
    private string _payingLink;
    private IHasher _hasher;

    public PaymentSystem(string payingLink, IHasher hasher)
    {
        if (payingLink == null)
            throw new ArgumentNullException();

        if (hasher == null) 
            throw new ArgumentNullException();

        _payingLink = payingLink;
        _hasher = hasher;
    }

    public string GetPayingLink(Order order)
    {
        if(order == null)
            throw new ArgumentNullException();

        return _payingLink.Insert(_payingLink.Length - 1, _hasher.GetOrderHash(order));
    }
}


public class Hasher : IHasher
{
    private IEnumerable<IHasher> _hasher;

    public Hasher (IEnumerable<IHasher> hasher)
    {
        _hasher = hasher;
    }

    public string GetOrderHash(Order order)
    {
        if(order == null)
            throw new ArgumentNullException();

        string hash = "";
        
        foreach (var hasher in _hasher)
            hash += hasher.GetOrderHash(order);

        return hash;
    }

    public static Hasher Create(params IHasher[] hasher)
    {
        return new Hasher(hasher);
    }
}
public class IdHasher : IHasher
{
    public string GetOrderHash(Order order)
    {
        return Utilities.GetMD5(order.Id);
    }
}

public class AmountHasher : IHasher
{
    public string GetOrderHash(Order order)
    {
        return Utilities.GetMD5(order.Amount);
    }
}

public class SecretHasрer : IHasher
{
    private int _secretKay;

    public SecretHasрer(int secretKay)
    {
        _secretKay = secretKay;
    }

    public string GetOrderHash(Order order)
    {
        return Utilities.GetMD5(_secretKay);
    }
}

public static class Utilities
{
    public static string GetMD5(int input)
    {
        var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input.ToString()));

        string replaceCahr = "-";

        return BitConverter.ToString(hash).Replace(replaceCahr, "").ToLower();
    }
}

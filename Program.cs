using System;
using System.Security.Cryptography;
using System.Text;

namespace Payment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PaymentSystem paymentSystem = new PaymentSystem("pay.system1.ru/order?amount=12000RUB&hash={}", new MD5Hasher());
            PaymentSystem1 paymentSystem1 = new PaymentSystem1("order.system2.ru/pay?hash={}", new MD5Hasher());
            PaymentSystem2 paymentSystem2 = new PaymentSystem2("system3.com/pay?amount=12000&curency=RUB&hash={}", 777, new SHA1Hasher());

            Order order = new Order(32, 1200);

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

    public Order(int id, int amount)
    {
        Id = id;
        Amount = amount;
    }
}

public interface IPaymentSystem
{
    string GetPayingLink(Order order);
}

public interface IHashSystem
{
    string GetOrderHash(int input);
}

public class PaymentSystem : IPaymentSystem
{
    private string _payingLink;
    private IHashSystem _hasher;

    public PaymentSystem(string payingLink, IHashSystem hasher)
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

        string payLinkHash = _payingLink.Insert(_payingLink.Length - 1, _hasher.GetOrderHash(order.Id));

        return payLinkHash;
    }
}

public class PaymentSystem1 : IPaymentSystem
{
    private string _payingLink;
    private IHashSystem _hasher;

    public PaymentSystem1(string payingLink, IHashSystem hasher)
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
        if (order == null)
            throw new ArgumentNullException();

        string hash = _hasher.GetOrderHash(order.Id) + _hasher.GetOrderHash(order.Amount);
        string payLinkHash = _payingLink.Insert(_payingLink.Length - 1, hash);

        return payLinkHash;
    }
}

public class PaymentSystem2 : IPaymentSystem
{
    private int _secretKey;
    private string _payingLink;

    private IHashSystem _hasher;

    public PaymentSystem2(string payingLink, int secretKey, IHashSystem hasher)
    {
        if (payingLink == null)
            throw new ArgumentNullException();

        if (hasher == null)
            throw new ArgumentNullException();

        _payingLink = payingLink;
        _hasher = hasher;
        _secretKey = secretKey;
    }

    public string GetPayingLink(Order order)
    {
        if (order == null)
            throw new ArgumentNullException();

        string hash = _hasher.GetOrderHash(order.Id) + _hasher.GetOrderHash(order.Amount) + _hasher.GetOrderHash(_secretKey);
        string payLinkHash = _payingLink.Insert(_payingLink.Length - 1, hash);

        return payLinkHash;
    }
}

public class MD5Hasher : IHashSystem
{
    public string GetOrderHash(int input)
    {
        MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input.ToString()));

        return Convert.ToBase64String(hash);
    }
}

public class SHA1Hasher : IHashSystem
{
    public string GetOrderHash(int input)
    {
        SHA1 sha1 = SHA1.Create();
        byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input.ToString()));

        return Convert.ToBase64String(hash);
    }
}


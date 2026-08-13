using Microsoft.Data.SqlClient;
using PaymentsApplication.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;
namespace PaymentsApplication.Models
{
    public class transactionRepo
    {
        public List<Transaction> transactionsList = new List<Transaction>();
        public List<Transaction> transactionsFromDateList= new List<Transaction>();
        //local
        //private string connectionString = @"Server=.;Database=testDB;Integrated Security=True;TrustServerCertificate=True;";

        //server
        //private string connectionString = @"Server=.\MSSQLSERVER02;Database=testDB;Integrated Security=True;TrustServerCertificate=True;";
        ApplicationDbContext context = new ApplicationDbContext();
        public bool insertTransaction(Transaction transaction)
        {
            
            context.Transactions.Add(transaction);
            context.SaveChanges();


            return true;


        }
        public List<Transaction> GetTransactionsHistory(string ph)
        {

            return context.Transactions
        .Where(x => x.SenderPhoneNumber == ph ||
                    x.ReceiverPhoneNumber == ph)
        .ToList();

        }
        public List<Transaction> GetTransactionsHistoryFromDate(string PhoneNumber, DateTime TransactionStartDate,
    DateTime TransactionEndDate)
        {

            return context.Transactions
        .Where(x =>
            x.TransactionDate >= TransactionStartDate &&
            x.TransactionDate <= TransactionEndDate &&
            (x.SenderPhoneNumber == PhoneNumber ||
             x.ReceiverPhoneNumber == PhoneNumber))
        .ToList();

        }
        public decimal GetSumOfAddMoney(string PhoneNumber)
        {

            DateTime startOfMonth =
        new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime startOfNextMonth =
                startOfMonth.AddMonths(1);

            decimal sum = context.Transactions
                .Where(x =>
                    x.TransactionType == "add money" &&
                    x.ReceiverPhoneNumber == PhoneNumber &&
                    x.TransactionDate >= startOfMonth &&
                    x.TransactionDate < startOfNextMonth)
                .Sum(x => x.Amount);

            return sum;
        }
        public decimal GetSumOfSendMoney(string PhoneNumber)
        {

            DateTime startOfMonth =
        new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime startOfNextMonth =
                startOfMonth.AddMonths(1);

            decimal sum = context.Transactions
                .Where(x =>
                    x.TransactionType == "send money" &&
                    x.SenderPhoneNumber == PhoneNumber &&
                    x.TransactionDate >= startOfMonth &&
                    x.TransactionDate < startOfNextMonth)
                .Sum(x => x.Amount);

            return sum;
        }
        public decimal GetSumOfReceiveMoney(string PhoneNumber)
        {

            DateTime startOfMonth =
       new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime startOfNextMonth =
                startOfMonth.AddMonths(1);

            decimal sum = context.Transactions
                .Where(x =>
                    x.TransactionType == "send money" &&
                    x.ReceiverPhoneNumber == PhoneNumber &&
                    x.TransactionDate >= startOfMonth &&
                    x.TransactionDate < startOfNextMonth)
                .Sum(x => x.Amount);

            return sum;
        }
    }
}

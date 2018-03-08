using System;
using System.Collections.Generic;
using GH.Core.Models;
using GH.Core.ViewModels;

namespace GH.Core.Adapters
{
    public class TransactionAdapter
    {
        public static TransactionViewModel Convert(Transaction transaction)
        {
            Type type = transaction.FollowTransaction.Type.GetType();
            string name = Enum.GetName(type, transaction.FollowTransaction.Type);
            return new TransactionViewModel
            {
                Date = transaction.FollowTransaction.Date,
                Description = name ?? string.Empty
            };
        }

        public static IEnumerable<TransactionViewModel> Convert(IEnumerable<Transaction> transactions)
        {
            var list = new List<TransactionViewModel>();
            foreach (var transaction in transactions)
            {
                Type type = transaction.FollowTransaction.Type.GetType();
                string name = Enum.GetName(type, transaction.FollowTransaction.Type);
                list.Add(new TransactionViewModel
                {
                    Date = transaction.FollowTransaction.Date,
                    Description = name ?? string.Empty
                });
            }
            return list;
        }
    }
}
using System;
using GH.Core.Extensions;
using GH.Core.IRepositories;
using GH.Core.IServices;
using GH.Core.Models;
using GH.Core.Repositories;
using GH.Core.ViewModels;
using MongoDB.Driver;

namespace GH.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private ITransactionRepository TransactionRepository { get; set; }
        public TransactionService()
        {
            TransactionRepository = new TransactionRepository();
        }

        public Transaction InsertTransaction(FollowTransactionParamter paramter)
        {
            var followTransaction = new FollowTransaction
            {
                FromUser = paramter.FromUser,
                ToUser = paramter.ToUser,
                Date = paramter.Date,
                Type = paramter.Type
            };

            var transaction = new Transaction
            {
                TransactionType = TransactionType.FollowTransaction,
                FollowTransaction = followTransaction,
                ModifiedOn = DateTime.Now.Date,
                CreatedOn = DateTime.Now.Date,
                CreatedBy = paramter.User.Id,
                ModifiedBy = paramter.User.Id
            };

            TransactionRepository.Create(transaction);
            return transaction;
        }

        public TransactionResult GetFollowTransactions(TransactionParamter paramter)
        {
            var builders = Builders<Transaction>.Filter;
            var filter = builders.Eq("TransactionType", paramter.TransactionType) &
                         builders.Eq("FollowTransaction.FromUser", paramter.FollowTransactionParamter.FromUser) &
                         builders.Eq("FollowTransaction.ToUser", paramter.FollowTransactionParamter.ToUser);
            var skip = paramter.Start * paramter.Length;
            var result = new TransactionResult
            {
                Total = TransactionRepository.GetTotalTransactions(filter).Count(),
                Transactions = TransactionRepository.GetTransactions(filter, skip, paramter.Length).ToEnumerable()
            };
            return result;
        }
    }
}
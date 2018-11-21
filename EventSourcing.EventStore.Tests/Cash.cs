using System;
using EventSourcing;

namespace Tnf.EventSourcing.EventStore.Tests
{
    public class Cash : EventSourcedAggregateRoot
    {
        public double Limit { get; private set; }
        public double Balance { get; private set; }

        public Cash(Guid id) : base(id)
        {
            Handle<CashCreated>(Apply);
            Handle<WithdrawRefused>(Apply);
            Handle<BalanceChanged>(Apply);
        }

        public Cash(Guid id, double limit) : this(id)
        {
            Update(new CashCreated(limit));
        }

        public bool Withdraw(double value)
        {
            if (value <= Balance)
            {
                Update(new WithdrawMade(value, Balance, Balance - value));
                Update(new BalanceChanged(Balance, Balance - value));

                return true;
            }
            else
            {
                Update(new WithdrawRefused(value, "Insuficent balance"));

                return false;
            }
        }

        public void Deposit(double value)
        {
            Update(new DepositMade(value, Balance, Balance + value));
            Update(new BalanceChanged(Balance, Balance - value));
        }

        private void Apply(BalanceChanged balanceChanged)
        {
            Balance = balanceChanged.BalanceAfter;
        }

        private void Apply(CashCreated cashCreated)
        {
            Limit = cashCreated.Limit;
        }

        private void Apply(WithdrawRefused withdrawalRefused) { }
    }

    public class BalanceChanged : VersionedEvent
    {
        public double BalanceBefore { get; set; }
        public double BalanceAfter { get; set; }

        public BalanceChanged(double balanceBefore, double balanceAfter)
        {
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
        }
    }

    public class CashCreated : VersionedEvent
    {
        public double Limit { get; set; }

        public CashCreated(double limit)
        {
            Limit = limit;
        }
    }

    public class WithdrawMade : VersionedEvent
    {
        public double Amount { get; set; }
        public double BalanceBefore { get; set; }
        public double BalanceAfter { get; set; }

        public WithdrawMade(double amount, double balanceBefore, double balanceAfter)
        {
            Amount = amount;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
        }
    }

    public class WithdrawRefused : VersionedEvent
    {
        public double Amount { get; set; }
        public string RefusalReason { get; set; }

        public WithdrawRefused(double amount, string refusalReason)
        {
            Amount = amount;
            RefusalReason = refusalReason;
        }
    }

    public class DepositMade : VersionedEvent
    {
        public double Amount { get; set; }
        public double BalanceBefore { get; set; }
        public double BalanceAfter { get; set; }

        public DepositMade(double amount, double balanceBefore, double balanceAfter)
        {
            Amount = amount;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
        }
    }
}

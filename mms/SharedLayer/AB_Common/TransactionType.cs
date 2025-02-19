using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.AB_Common
{
    public enum TransactionType
    {
        ItemIssue = 1,
        IssueReturn = 2,
        Purchase = 3,
        PurchaseReturn = 4,
        Adjustment = 5,
        ItemDamage = 6,
        InventoryUpdate = 7,
        IssueToJob = 8,
        IssueReturnFromJob = 9,
        FuelIssueInplant = 10,
        FuelIssueReturn = 11,
        DirectIssue = 12,
        TransferOut = 14,
        TransferIn = 15, 
        Transfer = 16,
        Ticket = 17,
        CostBuildup = 18,
        Intangible = 20
    }
}
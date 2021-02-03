using MismeAPI.Data.Entities.NonDatabase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IAppleAppStoreService
    {
        bool IsReceiptValid(AppleReceipt receipt);

        AppleReceipt GetReceipt(string receiptData);
    }
}

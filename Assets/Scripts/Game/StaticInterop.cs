using UnityEngine;
using System.Collections;
using System;

/*
A class for communication with the Windows Universal SDK for In-App purchases.
*/
public class StaticInterop
{
    // Delegate for purchasing coins.
    public delegate void PurchaseCoinsF(string product);

    // Delegate for confirming local purchase.
    public delegate void PurchaseConfirmF(string product, Guid transaction);

    public static PurchaseCoinsF PurchaseCoins;
    public static PurchaseConfirmF ConfirmLocalPurchase;

    // Requests the purchase of the product from the Windows Store.
    public static void FirePurchaseCoins(string product)
    {
        if (PurchaseCoins != null)
        {
            PurchaseCoins(product);
        }
        else if(Debug.isDebugBuild)
        {
            // If the Windows Store is not initialized, fake a purchase.
            Store.ConfirmPurchase(Guid.NewGuid(), "coins100");
        }
    }

    public static void ConfirmPurchase(string product, Guid transaction)
    {
        if(ConfirmLocalPurchase != null)
        {
            ConfirmLocalPurchase(product, transaction);
        }
    }
}
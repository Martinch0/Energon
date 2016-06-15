using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/*
A class to handle all store functionality.
*/
public class Store : RESTapi {

    // Cost of the shield and revive
    [SerializeField] private int ShieldCost = 10;
    [SerializeField] private int ReviveCost = 20;

    // Text lables for the shield and revive costs
    [SerializeField] private Text ShieldCostText;
    [SerializeField] private Text ReviveCostText;

    public static Store mInstance = null;

    private static Guid transaction;
    private static bool isInProgress = false;

    public static Store GetInstance()
    {
        return mInstance;
    }

	void Start () {
        if(mInstance == null)
        {
            mInstance = this;
        }

        // Load the price labels
        ShieldCostText.text = ShieldCost + "";
        ReviveCostText.text = ReviveCost + "";
	}
	
    // Buy a shield
    public void BuyShield()
    {
        if (PlayerSettings.SpendCoins(ShieldCost))
        {
            PlayerSettings.AddShields(1);
        }
    }

    // Buy a revive
    public void BuyRestart()
    {
        if (PlayerSettings.SpendCoins(ReviveCost))
        {
            PlayerSettings.AddRestarts(1);
        }
    }

    // Confirm purchase after store transaction has been complete.
    public static void ConfirmPurchase(Guid transactionId, string product)
    {
        Dictionary<string, string> formData = new Dictionary<string, string>();
        formData.Add("uid", SystemInfo.deviceUniqueIdentifier);
        formData.Add("transactionid", transactionId.ToString());
        formData.Add("productid", product);
        formData.Add("secret", secret);

        transaction = transactionId;
        isInProgress = true;

        GetInstance().POST(server + "/newTransaction", formData, OnPurchaseConfirmed, OnPurchaseError);
    }

    private static void OnPurchaseError()
    {
        isInProgress = false;
        GameLogic.GetGameLogic().HideShopMenu();
        GameLogic.GetGameLogic().ShowPurchaseError();
    }

    private static void OnPurchaseConfirmed(string product)
    {
        switch (product)
        {
            case "coins100":
                PlayerSettings.AddCoins(100);
                StaticInterop.ConfirmPurchase(product, transaction);
                GameLogic.GetGameLogic().HideShopMenu();
                GameLogic.GetGameLogic().ShowPurchaseSuccessful();
                break;
            case "coins250":
                PlayerSettings.AddCoins(250);
                StaticInterop.ConfirmPurchase(product, transaction);
                GameLogic.GetGameLogic().HideShopMenu();
                GameLogic.GetGameLogic().ShowPurchaseSuccessful();
                break;
            case "coins1000":
                PlayerSettings.AddCoins(1000);
                StaticInterop.ConfirmPurchase(product, transaction);
                GameLogic.GetGameLogic().HideShopMenu();
                GameLogic.GetGameLogic().ShowPurchaseSuccessful();
                break;
            case "existing":
                StaticInterop.ConfirmPurchase(product, transaction);
                GameLogic.GetGameLogic().HideShopMenu();
                GameLogic.GetGameLogic().ShowPurchaseExisting();
                break;
            default:
                GameLogic.GetGameLogic().HideShopMenu();
                GameLogic.GetGameLogic().ShowPurchaseError();
                break;
        }
        transaction = new Guid();
        isInProgress = false;
    }

    // In-App store calls.

    public void Purchase100Coins()
    {
        if (!isInProgress)
        {
            StaticInterop.FirePurchaseCoins("coins100");
        }
    }

    public void Purchase250Coins()
    {
        if (!isInProgress)
        {
            StaticInterop.FirePurchaseCoins("coins250");
        }
    }

    public void Purchase1000Coins()
    {
        if (!isInProgress)
        {
            StaticInterop.FirePurchaseCoins("coins1000");
        }
    }
}

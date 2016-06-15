After the build with Unity is completed. Add the following methods to the App.xaml.cs and link the StaticInterop.PurchaseCoins and StaticInterop.ConfirmPurchase with the corresponding methods.

// StaticInterop.PurchaseCoins += StaticInterop_PurchaseCoins;
// StaticInterop.ConfirmLocalPurchase += StaticInterop_ConfirmLocalPurchase;

        public void StaticInterop_PurchaseCoins(string product)
        {
            appCallbacks.InvokeOnUIThread(() => {
                appCallbacks.UnityPause(1);
                InitializePurchasesModule(product);
                appCallbacks.UnityPause(0);
            }, true);
        }

        public void StaticInterop_ConfirmLocalPurchase(string product, Guid transaction)
        {
            appCallbacks.InvokeOnUIThread(async () => {
#if DEBUG
                await CurrentAppSimulator.ReportConsumableFulfillmentAsync(product, transaction);
#else
                await CurrentApp.ReportConsumableFulfillmentAsync(product, transaction);
#endif
            }, true);
        }

        public async void InitializePurchasesModule(string product)
        {
#if DEBUG
            Package package = Package.Current;
            Windows.Storage.StorageFolder sfolder = package.InstalledLocation;
            Windows.Storage.StorageFolder sfolder2 = await sfolder.GetFolderAsync("data");
            Windows.Storage.StorageFile sfile = await sfolder2.GetFileAsync("WindowsStoreProxy.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(sfile);
            PurchaseResults purchaseResults = await CurrentAppSimulator.RequestProductPurchaseAsync(product);
#else
            PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync(product);
#endif
            Debug.WriteLine("TransactionID: " + purchaseResults.TransactionId);
            switch (purchaseResults.Status)
            {
                case ProductPurchaseStatus.Succeeded:
                    Debug.WriteLine("Success");
                    appCallbacks.InvokeOnAppThread(() => {
                        Store.ConfirmPurchase(purchaseResults.TransactionId, product);
                    }, true);
                    break;
                case ProductPurchaseStatus.AlreadyPurchased:
                    Debug.WriteLine("AlreadyPurchased");
                    appCallbacks.InvokeOnAppThread(() => {
                        Store.ConfirmPurchase(purchaseResults.TransactionId, product);
                    }, true);
                    break;
                case ProductPurchaseStatus.NotFulfilled:
                    Debug.WriteLine("NotFullfilled");
                    appCallbacks.InvokeOnAppThread(() => {
                        Store.ConfirmPurchase(purchaseResults.TransactionId, product);
                    }, true);
                    break;
                case ProductPurchaseStatus.NotPurchased:
                    Debug.WriteLine("Not Purchased");
                    break;
                default:
                    Debug.WriteLine("Error");
                    break;
            }
        }
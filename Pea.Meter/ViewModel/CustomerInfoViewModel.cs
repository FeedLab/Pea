using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CustomerInfoViewModel : ObservableObject
{
    private readonly ILogger<CustomerInfoViewModel> logger;
    private readonly PeaAdapter peaAdapter;

    [ObservableProperty] private bool isCustomerProfileVisible = false;
    [ObservableProperty] private string customerName = "";
    [ObservableProperty] private string customerId = "";
    [ObservableProperty] private string meterNumber = "";

    public CustomerInfoViewModel(ILogger<CustomerInfoViewModel> logger, PeaAdapter peaAdapter)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsCustomerProfileVisible = true;

                if (string.IsNullOrEmpty(peaAdapter.CustomerName) ||
                    string.IsNullOrEmpty(peaAdapter.CustomerId) ||
                    string.IsNullOrEmpty(peaAdapter.MeterNumber))
                {
                    logger.LogError("Customer profile not loaded yet");
                    return;
                }

                CustomerName = peaAdapter.CustomerName;
                CustomerId = peaAdapter.CustomerId;
                MeterNumber = peaAdapter.MeterNumber;
            });
        });

        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this,
            (r, m) => { MainThread.InvokeOnMainThreadAsync(async () => { IsCustomerProfileVisible = false; }); });
    }
}
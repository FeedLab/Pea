using System.Diagnostics.CodeAnalysis;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.Components.Configuration;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
public partial class TariffComponent : ContentView
{
    public ConfigurationTariffModel TariffConfiguration { get; }

    public TariffComponent()
    {
        var storageService = AppService.GetRequiredService<StorageService>();
        TariffConfiguration =storageService.ConfigurationTariffModel;

        InitializeComponent();
    }
}
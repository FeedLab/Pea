using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Meter.View.Interface;

namespace Pea.Meter.View.Components;

public partial class HomeRegisterDemoAccountWaitPopup : ContentView, ICloseable
{
    public event EventHandler? CloseRequested;

    public HomeRegisterDemoAccountWaitPopup()
    {
        InitializeComponent();
    }

    // private void Close_OnClicked(object? sender, EventArgs e)
    // {
    //     CloseRequested?.Invoke(this, EventArgs.Empty);
    // }
    
    public void CloseAction()
    {
    }
}
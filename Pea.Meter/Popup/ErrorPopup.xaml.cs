using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pea.Meter.Popup;

public partial class ErrorPopup : ContentView
{
    public ErrorPopup(string title, string message)
    {
        InitializeComponent();
        
        ErrorTitle.Text = title;
        ErrorMessage.Text = message;
    }
}
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Models;
using Pea.Meter.View.Interface;
using Pea.Meter.ViewModel;
using Syncfusion.Maui.Buttons;

namespace Pea.Meter.View.Components;

public partial class CustomerInfoPopupView : ContentView, ICloseable
{
    private readonly CustomerInfoViewModel viewModel;
    public event EventHandler? CloseRequested;
    private bool hasRemoveButtonBeenPressed = false;
    private bool removeAccount;

    public CustomerInfoPopupView(CustomerInfoViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        
        BindingContext = viewModel;
    }
    

    private void Button_OnClicked(object? sender, EventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CloseAndRemove_OnClicked(object? sender, EventArgs e)
    {
        hasRemoveButtonBeenPressed = true;


        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Close_OnClicked(object? sender, EventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public void CloseAction()
    {
        if (hasRemoveButtonBeenPressed)
        {
            WeakReferenceMessenger.Default.Send(new UserLoggedOutMessage());
            WeakReferenceMessenger.Default.Send(new UserAccountRemovedMessage());
        }
    }

    private void ToggleButton_OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        removeAccount = e.IsChecked ?? false;

        var (fadeIn, fadeOut) = removeAccount
            ? (CloseAndRemove, Close)
            : (Close, CloseAndRemove);

        fadeIn.IsVisible = true;

        var animation = new Animation();
        animation.Add(0, 1, new Animation(t => fadeOut.Opacity = t, 1, 0));
        animation.Add(0, 1, new Animation(t => fadeIn.Opacity = t, 0, 1));
        animation.Commit(this, "ButtonSwap", length: 200, finished: (_, _) => fadeOut.IsVisible = false);
    }
}
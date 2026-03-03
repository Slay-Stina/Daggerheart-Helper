using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Daggerheart_Helper;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Daggerheart-Helper" };
    }
}
using RecetteApp.ViewModels;

namespace RecetteApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;
    private bool _animationDejaJouee;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();

        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_animationDejaJouee)
        {
            _animationDejaJouee = true;

            RootGrid.TranslationY = 10;
            await Task.WhenAll(
                RootGrid.FadeToAsync(1, 220, Easing.CubicOut),
                RootGrid.TranslateToAsync(0, 0, 220, Easing.CubicOut)
            );
        }

        await _vm.ChargerDonnees();
    }

    // Pas de logique métier — animation visuelle seulement (effet bounce sur le bouton cœur)
    private async void OnHeartClicked(object? sender, EventArgs e)
    {
        if (sender is not VisualElement el)
            return;

        try
        {
            await el.ScaleToAsync(1.15, 70, Easing.CubicOut);
            await el.ScaleToAsync(1.0, 90, Easing.CubicIn);
        }
        catch
        {
            // animations: best effort
        }
    }
}
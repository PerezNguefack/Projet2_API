using RecetteApp.ViewModels;

namespace RecetteApp.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (RootStack.Opacity < 1)
        {
            RootStack.TranslationY = 10;
            await Task.WhenAll(
                RootStack.FadeToAsync(1, 220, Easing.CubicOut),
                RootStack.TranslateToAsync(0, 0, 220, Easing.CubicOut)
            );
        }
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

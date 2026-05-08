using CommunityToolkit.Mvvm.ComponentModel;
using RecetteApp.Helpers;
using RecetteApp.Models;

namespace RecetteApp.ViewModels;

/// <summary>Ligne recette sur la page principale : favori via cœur (pas de swipe).</summary>
public partial class MealListRowVm : ObservableObject
{
    public Meal Recette { get; }

    [ObservableProperty]
    public partial bool EstFavori { get; set; }

    public MealListRowVm(Meal recette, bool estFavori)
    {
        Recette = recette;
        EstFavori = estFavori;
    }

    public bool PeutFavoriser => !string.IsNullOrWhiteSpace(Recette.IdMeal);

    /// <summary>Aperçu court des instructions (carte).</summary>
    public string ApercuDescription => RecetteTexteHelper.ApercuInstructions(Recette.StrInstructions);

    public string TempsEstime
    {
        get
        {
            var nbIngredients = Recette.GetIngredientLines().Count;
            var nbMots = (Recette.StrInstructions ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            int minutes = nbIngredients switch
            {
                <= 5  => 15,
                <= 9  => 25,
                <= 13 => 40,
                _     => 60
            };

            if (nbMots > 300) minutes += 10;

            return $"⏱ ~{minutes} min";
        }
    }

    public string SymboleCoeur => EstFavori ? "♥" : "♡";

    partial void OnEstFavoriChanged(bool value)
    {
        OnPropertyChanged(nameof(SymboleCoeur));
        OnPropertyChanged(nameof(CouleurCoeur));
    }

    public Color CouleurCoeur => EstFavori
        ? Color.FromArgb("#C62828")
        : Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#B0B0B0")
            : Color.FromArgb("#757575");
}

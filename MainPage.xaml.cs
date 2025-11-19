using DogPoemApp.ViewModels;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DogPoemApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSwiped(object sender, SwipedEventArgs e)
        {
            // Safety checks
            if (CardContainer == null) return;

            var viewModel = BindingContext as MainViewModel;
            if (viewModel == null || viewModel.IsLoading) return;

            // 1. Animate Card Out (Fly off screen based on swipe direction)
            double translationX = e.Direction == SwipeDirection.Right ? 500 : -500;
            uint duration = 200;

            await Task.WhenAll(
                CardContainer.FadeTo(0, duration),
                CardContainer.TranslateTo(translationX, 0, duration)
            );

            // 2. Reset Position (Invisible)
            CardContainer.TranslationX = 0;

            // 3. Load Data
            await viewModel.LoadDataAsync();

            // 4. Animate Card In (Fade back in)
            await CardContainer.FadeTo(1, duration);
        }
    }
}


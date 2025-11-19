using DogPoemApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DogPoemApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // Properties for UI Binding
        private string _dogImageUrl;
        public string DogImageUrl
        {
            get => _dogImageUrl;
            set { _dogImageUrl = value; OnPropertyChanged(); }
        }

        private string _poemTitle;
        public string PoemTitle
        {
            get => _poemTitle;
            set { _poemTitle = value; OnPropertyChanged(); }
        }

        private string _poemAuthor;
        public string PoemAuthor
        {
            get => _poemAuthor;
            set { _poemAuthor = value; OnPropertyChanged(); }
        }

        private string _poemContent;
        public string PoemContent
        {
            get => _poemContent;
            set { _poemContent = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private bool _isContentVisible;
        public bool IsContentVisible
        {
            get => _isContentVisible;
            set { _isContentVisible = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadNewPairCommand { get; }

        public MainViewModel()
        {
            _apiService = new ApiService();
            LoadNewPairCommand = new Command(async () => await LoadDataAsync());

            // Load initial data
            Task.Run(LoadDataAsync);
        }

        public async Task LoadDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                IsContentVisible = false; // Hide content while loading

                // Run tasks in parallel for speed
                var dogTask = _apiService.GetRandomDogAsync();
                var poemTask = _apiService.GetRandomPoemAsync();

                await Task.WhenAll(dogTask, poemTask);

                var dog = await dogTask;
                var poem = await poemTask;

                // Update UI on Main Thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Set Dog
                    DogImageUrl = dog?.Status == "success" ? dog.ImageUrl : "https://images.dog.ceo/breeds/retriever-golden/n02099601_3004.jpg"; // Fallback image

                    // Set Poem
                    if (poem != null)
                    {
                        PoemTitle = poem.Title;
                        PoemAuthor = $"— {poem.Author}";
                        PoemContent = string.Join("\n", poem.Content); // Ensure formatting
                    }
                    else
                    {
                        // Fallback if API fails
                        PoemTitle = "The Dog Stays";
                        PoemAuthor = "Unknown";
                        PoemContent = "The API is taking a nap,\nBut here is a dog,\nTo sit on your lap.";
                    }

                    IsContentVisible = true;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        // MVVM Property Changed Boilerplate
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

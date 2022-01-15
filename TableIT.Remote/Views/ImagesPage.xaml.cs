﻿using Microsoft.Maui.Controls;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class ImagesPage : ContentPage
    {
        private ImagesPageViewModel ViewModel { get; }

        public ImagesPage(ImagesPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadImages();
        }
    }
}
using System;
using Xamarin.Forms;

namespace DoubleTapTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageModel();
        }
    }
}

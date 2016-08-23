using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using PropertyChanged;
using FreshMvvm;
using Xamarin.Forms;

namespace DoubleTapTest
{
    /// <summary>
    /// Example of locking event to run once, if started twice.
    /// Will work by sharing locks. So every command must have the same internal lock object to be "syncronized".
    /// </summary>
    [ImplementPropertyChanged]
    public class MainPageModel : FreshBasePageModel
    {
        public ICommand NativeCommand { get; private set; }
        public ICommand CustomSharedCommand { get; private set; }
        public ICommand CustomWithConvertedParamterSharedLockCommand { get; private set; }
        public ICommand CustomWithoutParameterSharedLockCommand { get; private set; }
        public ICommand CustomObjectTypeParameterCommand { get; private set; }
        public ICommand CustomStringTypeParameterCommand { get; private set; }

        public bool CustomButtonCanExecute { get; private set; } = true;

        public MainPageModel()
        {
            NativeCommand = new Command<string>(NavigateLogic, (str) => !_isLoading);
            CustomSharedCommand = CoreMethods.CreateCommand(NavigateWrapperIntLogic, int.Parse); //async Task NavigateWrapperLogic(int index)
            CustomSharedCommand.CanExecuteChanged += (sender, e) => CustomButtonCanExecute = CustomSharedCommand.CanExecute(null);

            var sharedLock = new SharedLock();
            CustomWithConvertedParamterSharedLockCommand = CoreMethods.CreateCommand(NavigateWrapperIntLogic, int.Parse, sharedLock);
            CustomWithoutParameterSharedLockCommand = CoreMethods.CreateCommand(NavigateWrapperVoidLogic, sharedLock);

            CustomObjectTypeParameterCommand = CoreMethods.CreateCommand(NavigateWrapperObjectLogic); //async Task NavigateWrapperLogic(object indexObj)
            CustomStringTypeParameterCommand = CoreMethods.CreateCommand<string>(NavigateWrapperStringLogic); //async Task NavigateWrapperLogic(string indexStr)
        }

        Task NavigateWrapperObjectLogic(object indexObj)
        {
            return NavigateWrapperStringLogic(indexObj?.ToString());
        }

        Task NavigateWrapperStringLogic(string indexStr)
        {
            return NavigateWrapperIntLogic(int.Parse(indexStr));
        }

        async Task NavigateWrapperIntLogic(int index)
        {
            if (index == 0)
            {
                await Task.Delay(1000);
                await CoreMethods.PushPageModel<SubPageModel>();
            }
        }

        async Task NavigateWrapperVoidLogic()
        {
            await Task.Delay(1000);
            await CoreMethods.PushPageModel<SubPageModel>();
        }

        #region NavigateLogic
        private bool _isLoading; //Local can execute value.

        async void NavigateLogic(string indexStr)
        {
            int index = int.Parse(indexStr);
            if (index == 0)
            {
                _isLoading = true;
                ((Command)NativeCommand).ChangeCanExecute();
                await Task.Delay(1000);
                await Application.Current.MainPage.Navigation.PushAsync(new SubPage());
                _isLoading = false;
                ((Command)NativeCommand).ChangeCanExecute();
            }
            if (index == 1)
            {
                _isLoading = true;
                ((Command)NativeCommand).ChangeCanExecute();
                await Task.Delay(1000);
                await CoreMethods.PushPageModel<SubPageModel>();
                _isLoading = false;
                ((Command)NativeCommand).ChangeCanExecute();
            }
        }
        #endregion
    }
}

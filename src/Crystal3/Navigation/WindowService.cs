﻿using Crystal3.Model;
using Crystal3.UI.StatusManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Crystal3.Navigation
{
    /// <summary>
    /// User-level class for manipulating the Window itself.
    /// </summary>
    public class WindowService
    {
        internal WindowService(Window view, NavigationManager navManager, StatusManager statManager)
        {
            WindowView = view;
            NavigationManager = navManager;
            StatusManager = statManager;

            WindowViewId = ApplicationView.GetApplicationViewIdForWindow(view.CoreWindow);

            //todo figure out a better way to handle this.
            if (NavigationManager.RootNavigationService is FrameNavigationService)
                ((FrameNavigationService)NavigationManager.RootNavigationService).NavigationFrame.Navigated += HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated;

            WindowView.SizeChanged += WindowView_SizeChanged;
            WindowView.Closed += WindowView_Closed;
        }

        private void WindowView_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            //This is for detecting tablet-mode status.
            DeviceInformation.RefreshSubplatform(null);
        }

        private void WindowView_Closed(object sender, CoreWindowEventArgs e)
        {
            WindowView.SizeChanged -= WindowView_SizeChanged;
            WindowView.Closed -= WindowView_Closed;

            if (NavigationManager.RootNavigationService is FrameNavigationService)
                ((FrameNavigationService)NavigationManager.RootNavigationService).NavigationFrame.Navigated -= HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated;
        }

        internal Window WindowView { get; private set; }
        internal NavigationManager NavigationManager { get; private set; }
        internal StatusManager StatusManager { get; private set; }
        public int WindowViewId { get; private set; }

        private void HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            RefreshAppViewBackButtonVisibility(NavigationManager.RootNavigationService);
        }

        private void RefreshAppViewBackButtonVisibility(NavigationServiceBase sender)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            if (((CrystalApplication)CrystalApplication.Current).Options.HandleBackButtonForTopLevelNavigation)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                    sender.CanGoBackward ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        public void RefreshAppViewBackButtonVisibility()
        {
            if (((CrystalApplication)CrystalApplication.Current).Options.HandleBackButtonForTopLevelNavigation)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                    NavigationManager.GetAllServices().Any(x => x.CanGoBackward) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        //not very mvvm-y
        public void SetAppViewBackButtonVisibility(bool visible)
        {
            if (((CrystalApplication)CrystalApplication.Current).Options.HandleBackButtonForTopLevelNavigation)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = 
                    visible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        internal ViewModelBase GetRootViewModel()
        {
            return NavigationManager.RootNavigationService.GetNavigatedViewModel();
        }

        /// <summary>
        /// Queries if the window paired with this window service is currently in tablet mode.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsViewInTabletModeAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            WindowView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                taskCompletionSource.SetResult(DeviceInformation.IsCurrentViewInTabletMode());
            })).AsTask().Forget();

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Queries if the window paired with this window service is currently in mixed reality.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsViewInMixedRealityAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            WindowView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                taskCompletionSource.SetResult(DeviceInformation.IsCurrentViewInTabletMode());
            })).AsTask().Forget();

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Queries if the window paired with this window service is currently in Continuum for Phone mode.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsViewInPhoneContinuumModeAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            WindowView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                taskCompletionSource.SetResult(DeviceInformation.IsCurrentViewInPhoneContinuumMode());
            })).AsTask().Forget();

            return taskCompletionSource.Task;
        }
    }
}

﻿using Crystal3.Model;
using Crystal3.UI.StatusManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
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

            NavigationManager.RootNavigationService.NavigationFrame.Navigated += HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated;

            WindowView.Closed += WindowView_Closed;
        }

        private void WindowView_Closed(object sender, CoreWindowEventArgs e)
        {
            try
            {
                NavigationManager.RootNavigationService.NavigationFrame.Navigated -= HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated;
            }
            catch (Exception) { }
        }

        internal Window WindowView { get; private set; }
        internal NavigationManager NavigationManager { get; private set; }
        internal StatusManager StatusManager { get; private set; }

        private void HandleTopLevelNavigationForBackButton_NavigationFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            RefreshAppViewBackButtonVisibility(sender as Frame);
        }

        private void RefreshAppViewBackButtonVisibility(Frame sender)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            if (((CrystalApplication)CrystalApplication.Current).Options.HandleBackButtonForTopLevelNavigation)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ((Frame)sender).CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        public void RefreshAppViewBackButtonVisibility()
        {
            RefreshAppViewBackButtonVisibility(NavigationManager.RootNavigationService.NavigationFrame);
        }

        //not very mvvm-y
        public void SetAppViewBackButtonVisibility(bool visible)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = visible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        internal ViewModelBase GetRootViewModel()
        {
            var navManager = NavigationManager;
            var frame = navManager.RootNavigationService.NavigationFrame;
            var page = frame.Content as Page;
            var viewModel = page.DataContext as ViewModelBase;

            return viewModel;
        }
    }
}

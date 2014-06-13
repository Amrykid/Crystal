﻿using Crystal2.IOC;
using Crystal2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Crystal2
{
    /// <summary>
    /// Implements ICrystalApplication for WinRT and WinPRT (Windows Phone) platforms.
    /// </summary>
    public abstract class CrystalWinRTApplication : Application, ICrystalApplication
    {
        private TransitionCollection transitions;

        public CrystalWinRTApplication()
            : base()
        {
            var initializeComponent = this.GetType().GetTypeInfo().GetDeclaredMethod("InitializeComponent");
            if (initializeComponent != null)
                initializeComponent.Invoke(this, new object[] {  });

            this.Suspending += this.OnSuspending;

            OnInitialize();

            DetectPlatform();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity

            await OnSuspendingAsync();

            deferral.Complete();
        }

        /// <summary>
        /// Detects if we're running WinRT or WinPRT.
        /// </summary>
        private void DetectPlatform()
        {
            try
            {
                //Calls an unsupported API.
                Windows.Networking.BackgroundTransfer.BackgroundDownloader.RequestUnconstrainedDownloadsAsync(null);
                CurrentPlatform = Platform.Windows;
            }
            catch (NotImplementedException)
            {
                //The API isn't supported on Windows Phone. Thus, the current platform is Windows Phone.
                CurrentPlatform = Platform.WindowsPhone;
            }
            catch (Exception)
            {
                //Otherwise, this is Windows.
                CurrentPlatform = Platform.Windows;
            }
        }

        public void OnInitialize()
        {
            //set up navigation
            IoCManager.Register<INavigationDirectoryProvider>(new W8NavigationDirectoryProvider(this.GetType().GetTypeInfo().Assembly));
            var navigationProvider = new W8NavigationProvider();
            IoCManager.Register<INavigationProvider>(navigationProvider);
        }

        protected override void OnLaunched(Windows.ApplicationModel.Activation.LaunchActivatedEventArgs e)
        {

            RootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (RootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                RootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                RootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = RootFrame;

                //Now that we have a frame, initialize the INavigationProvider.
                IoCManager.Resolve<INavigationProvider>()
                    .Setup(RootFrame);
            }

            if (RootFrame.Content == null)
            {
                //TODO Figure this out
                if (IsPhone())
                {
                    //#if WINDOWS_PHONE_APP
                    // Removes the turnstile navigation for startup.
                    if (RootFrame.ContentTransitions != null)
                    {
                        this.transitions = new TransitionCollection();
                        foreach (var c in RootFrame.ContentTransitions)
                        {
                            this.transitions.Add(c);
                        }
                    }

                    RootFrame.ContentTransitions = null;
                    RootFrame.Navigated += this.RootFrame_FirstNavigated;
                    //#endif
                }

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                //if (!RootFrame.Navigate(typeof(MainPage), e.Arguments))
                //{
                //    throw new Exception("Failed to create initial page");
                //}
                OnNormalLaunchNavigationReady(e);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
        }

        /// <summary>
        /// Returns if the application is running on Windows Phone.
        /// </summary>
        /// <returns></returns>
        public static bool IsPhone()
        {
            //var val = Type.GetType("Windows.Phone.UI.Input.HardwareButtons", false);
            //var val = Type.GetType("Windows.Networking.BackgroundTransfer.ContentPrefetcher", false);
            //return val == null;

            return ((CrystalWinRTApplication)Current).CurrentPlatform == Platform.WindowsPhone;
        }

        /// <summary>
        /// Returns the WinRT current platform.
        /// </summary>
        public Platform CurrentPlatform { get; private set; }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection(); // { new Windows.UI.Xaml.Media.Animation.NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// The root frame created by Crystal.
        /// </summary>
        protected Frame RootFrame { get; private set; }

        /// <summary>
        /// An abstract method called when the application is ready to navigate.
        /// </summary>
        protected abstract void OnNormalLaunchNavigationReady(Windows.ApplicationModel.Activation.LaunchActivatedEventArgs args);

        protected abstract Task OnSuspendingAsync();
    }
}

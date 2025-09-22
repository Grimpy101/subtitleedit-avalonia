﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;
using System;
using System.Text;

namespace Nikse.SubtitleEdit
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Global exception handling
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    var exception = e.ExceptionObject as Exception;
                    Se.LogError(exception ?? new Exception(), "Unhandled AppDomain Exception");
                };

                // Setup application lifetime
                var lifetime = new ClassicDesktopStyleApplicationLifetime
                {
                    Args = args,
                    ShutdownMode = ShutdownMode.OnLastWindowClose
                };

                // Register icon providers
                IconProvider.Current
                    .Register<FontAwesomeIconProvider>()
                    .Register<MaterialDesignIconProvider>();

                // Load settings
                Se.LoadSettings();

                // Build and configure the app
                var appBuilder = AppBuilder.Configure<Application>()
                    .UsePlatformDetect()
                    .AfterSetup(b => ConfigureApplication(b, lifetime))
                    .SetupWithLifetime(lifetime);

                // Configure dependency injection
                SetupDependencyInjection();

                // Register encoding provider
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Set current theme
                UiUtil.SetCurrentTheme();

                // Setup main window
                SetupMainWindow(lifetime);

#if DEBUG
                if (lifetime.MainWindow != null)
                {
                    lifetime.MainWindow.AttachDevTools();
                }
#endif

                // Start the application
                lifetime.Start(args);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Critical error during application startup");
                Environment.Exit(1);
            }
        }

        private static void ConfigureApplication(AppBuilder b, ClassicDesktopStyleApplicationLifetime lifetime)
        {
            // Setup Fluent theme
            UiUtil.FluentTheme = new FluentTheme();
            UiUtil.FluentTheme.Palettes.Add(ThemeVariant.Dark, new ColorPaletteResources()
            {
                RegionColor = UiUtil.GetDarkThemeBackgroundColor(),
            });

            if (b.Instance != null)
            {
                b.Instance.Styles.Add(UiUtil.FluentTheme);
            }

            // Add DataGrid styles
            if (b.Instance != null)
            {
                b.Instance.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", UriKind.Absolute))
                {
                    Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
                });
            }

            // Add ColorPicker styles
            if (b.Instance != null)
            {
                b.Instance.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml", UriKind.Absolute))
                {
                    Source = new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml", UriKind.Absolute)
                });
            }

            // Set custom font
            if (Application.Current != null && !string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
            {
                UiUtil.SetFontName(Se.Settings.Appearance.FontName);
            }

            // Set application name
            if (b.Instance != null)
            {
                b.Instance.Name = "Subtitle Edit";
            }

            // Setup native menu
            if (b.Instance != null)
            {
                SetupNativeMenu(b.Instance, lifetime);
            }
        }

        private static void SetupNativeMenu(Application app, ClassicDesktopStyleApplicationLifetime lifetime)
        {
            var nativeMenu = new NativeMenu();
            var aboutMenu = new NativeMenuItem(Se.Language.Help.AboutSubtitleEdit);

            aboutMenu.Click += async (sender, e) =>
            {
                var aboutWindow = new AboutWindow();
                if (lifetime.MainWindow != null)
                {
                    await aboutWindow.ShowDialog(lifetime.MainWindow);
                }
            };

            nativeMenu.Items.Add(aboutMenu);
            NativeMenu.SetMenu(app, nativeMenu);

            // mac finder "Send to"
            if (OperatingSystem.IsMacOS())
            {
                if (app.TryGetFeature(typeof(IActivatableLifetime)) is not IActivatableLifetime activatable)
                {
                    return;
                }

                activatable.Activated += (sender, e) =>
                {
                    if (e is not FileActivatedEventArgs args || args.Kind != ActivationKind.File)
                    {
                        return;
                    }

                    Se.LogError($"App activated with {args.Files.Count} files");
                    
                    foreach (var storageItem in args.Files)
                    {
                        var filePath = storageItem.Path.LocalPath;
                        if (System.IO.File.Exists(filePath))
                        {
                            Se.LogError($"File opened via macOS activation: {filePath}");
                            if (lifetime.MainWindow?.Content is MainView mainView)
                            {
                                Dispatcher.UIThread.Post(async () =>
                                {
                                    await mainView.OpenFile(filePath);
                                });
                            }

                            break;
                        }
                    }
                };
            }
        }

        private static void SetupDependencyInjection()
        {
            var collection = new ServiceCollection();
            collection.AddSubtitleEditServices();
            Locator.Services = collection.BuildServiceProvider();
        }

        private static void SetupMainWindow(ClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = new Window
            {
                Title = "Subtitle Edit",
                Name = "MainWindow",
                Icon = UiUtil.GetSeIcon(),
                MinWidth = 800,
                MinHeight = 500,
            };

            var mainView = new MainView();
            lifetime.MainWindow.Content = mainView;

            lifetime.Startup += (_, e) =>
            {
                if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
                {
                    Se.LogError("lifetime.Startup parameter: " + e.Args[0]);
                    Dispatcher.UIThread.Post(async void () => { await mainView.OpenFile(e.Args[0]); });
                }
            };
        }
    }
}
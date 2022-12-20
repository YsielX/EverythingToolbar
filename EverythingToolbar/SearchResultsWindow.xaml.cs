﻿using EverythingToolbar.Helpers;
using EverythingToolbar.Properties;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EverythingToolbar
{
    public partial class SearchResultsWindow : Window
    {
        //public static Edge taskbarEdge;
        public static double taskbarHeight = 0;
        public static double taskbarWidth = 0;
        public bool IsOpen = false;
        Size dragStartSize = new Size();
        Point dragStartPosition = new Point();
        
        public new double Height
        {
            get
            {
                return (double)GetValue(HeightProperty);
            }
            set
            {
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double newHeight = Math.Max(Math.Min(screenHeight - taskbarHeight, value), 300);
                SetValue(HeightProperty, newHeight);
            }
        }
        
        public new double Width
        {
            get
            {
                return (double)GetValue(WidthProperty);
            }
            set
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double newWidth = Math.Max(Math.Min(screenWidth - taskbarWidth, value), 300);
                SetValue(WidthProperty, newWidth);
            }
        }

        public static readonly SearchResultsWindow Instance = new SearchResultsWindow();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Preventing the window from getting focus keeps focus on the deskband search box
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                const int WM_MOUSEACTIVATE = 0x0021;
                const int MA_NOACTIVATE = 0x0003;

                if (msg == WM_MOUSEACTIVATE)
                {
                    handled = true;
                    return new IntPtr(MA_NOACTIVATE);
                }
                else return IntPtr.Zero;
            });
        }

        private SearchResultsWindow()
        {
            InitializeComponent();
            DataContext = EverythingSearch.Instance;

            if (Settings.Default.isUpgradeRequired)
            {
                Settings.Default.Upgrade();

                if (Settings.Default.theme == "MEDIUM")
                    Settings.Default.theme = "DARK";

                Settings.Default.isUpgradeRequired = false;
                Settings.Default.Save();
            }

            ApplicationResources.Instance.ResourceChanged += (object sender, ResourcesChangedEventArgs e) =>
            {
                try
                {
                    Resources.MergedDictionaries.Add(e.NewResource);
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    ToolbarLogger.GetLogger("EverythingToolbar").Error(ex, "Failed to apply resource.");
                }
            };
            ApplicationResources.Instance.LoadDefaults();

            LostKeyboardFocus += OnLostKeyboardFocus;
            EventDispatcher.Instance.HideWindow += Hide;
            EventDispatcher.Instance.ShowWindow += Show;
        }

        public new void Hide()
        {
            IsOpen = false;
            HistoryManager.Instance.AddToHistory(EverythingSearch.Instance.SearchTerm);
            base.Hide();
        }

        public new void Show()
        {
            IsOpen = true;
            base.Show();
            Keyboard.Focus(SearchBox);
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == null)  // New focus outside application
            {
                Hide();
            }
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            dragStartSize.Height = Height;
            dragStartSize.Width = Width;
            dragStartPosition = PointToScreen(Mouse.GetPosition(this));
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Point mousePos = PointToScreen(Mouse.GetPosition(this));
            int widthModifier = (sender as Thumb).HorizontalAlignment == HorizontalAlignment.Left ? -1 : 1;
            int heightModifier = (sender as Thumb).VerticalAlignment == VerticalAlignment.Top ? -1 : 1;
            Width = dragStartSize.Width + widthModifier * (mousePos.X - dragStartPosition.X);
            Height = dragStartSize.Height + heightModifier * (mousePos.Y - dragStartPosition.Y);
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Settings.Default.popupSize = new Size(Width, Height);
            Settings.Default.Save();
        }

        private void OnOpened(object sender, EventArgs e)
        {
            //Keyboard.Focus(SearchBox);

            //switch (taskbarEdge)
            //{
            //    case Edge.Top:
            //        Placement = PlacementMode.Bottom;
            //        PopupMarginBorder.Margin = new Thickness(12, 0, 12, 12);
            //        break;
            //    case Edge.Left:
            //        Placement = PlacementMode.Right;
            //        PopupMarginBorder.Margin = new Thickness(0, 12, 12, 12);
            //        break;
            //    case Edge.Right:
            //        Placement = PlacementMode.Left;
            //        PopupMarginBorder.Margin = new Thickness(12, 12, 0, 12);
            //        break;
            //    case Edge.Bottom:
            //        Placement = PlacementMode.Top;
            //        PopupMarginBorder.Margin = new Thickness(12, 12, 12, 0);
            //        break;
            //}

            //Height = Properties.Settings.Default.popupSize.Height;
            //Width = Properties.Settings.Default.popupSize.Width;

            //QuinticEase ease = new QuinticEase
            //{
            //    EasingMode = EasingMode.EaseOut
            //};

            //int modifier = taskbarEdge == Edge.Right || taskbarEdge == Edge.Bottom ? 1 : -1;
            //Duration duration = TimeSpan.FromSeconds(Properties.Settings.Default.isAnimationsDisabled ? 0 : 0.4);
            //DoubleAnimation outer = new DoubleAnimation(modifier * 150, 0, duration)
            //{
            //    EasingFunction = ease
            //};
            //DependencyProperty outerProp = taskbarEdge == Edge.Bottom || taskbarEdge == Edge.Top ? TranslateTransform.YProperty : TranslateTransform.XProperty;
            //translateTransform?.BeginAnimation(outerProp, outer);

            //DoubleAnimation opacity = new DoubleAnimation(0, 1, duration)
            //{
            //    EasingFunction = ease
            //};
            //PopupMarginBorder?.BeginAnimation(OpacityProperty, opacity);

            //duration = TimeSpan.FromSeconds(Properties.Settings.Default.isAnimationsDisabled ? 0 : 0.8);
            //ThicknessAnimation inner = new ThicknessAnimation(new Thickness(0), duration)
            //{
            //    EasingFunction = ease
            //};
            //if (taskbarEdge == Edge.Top)
            //    inner.From = new Thickness(0, -50, 0, 50);
            //else if (taskbarEdge == Edge.Right)
            //    inner.From = new Thickness(50, 0, -50, 0);
            //else if (taskbarEdge == Edge.Bottom)
            //    inner.From = new Thickness(0, 50, 0, -50);
            //else if (taskbarEdge == Edge.Left)
            //    inner.From = new Thickness(-50, 0, 50, 0);
            //ContentGrid?.BeginAnimation(MarginProperty, inner);
        }

        private void OpenSearchInEverything(object sender, RoutedEventArgs e)
        {
            EverythingSearch.Instance.OpenLastSearchInEverything();
        }
    }
}

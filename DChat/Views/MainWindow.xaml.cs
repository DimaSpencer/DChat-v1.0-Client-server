using System;
using System.Windows;

namespace DarkChat
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TopPanel.MouseDown += MoveWindow;
            CloseButton.Click += CloseWindow;
        }
        private void MoveWindow(object sender, EventArgs e) => this.DragMove();
        private void CloseWindow(object sender, EventArgs e) => this.Close();
        private void MinimizeAWindow(object sender, EventArgs e) => this.WindowState = WindowState.Minimized;
    }
}

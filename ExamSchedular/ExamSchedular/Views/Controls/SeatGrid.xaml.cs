using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamSchedular.UI.Views.Controls
{
    public partial class SeatGrid : UserControl
    {
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(SeatGrid),
                new PropertyMetadata(0, OnLayoutChanged));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(SeatGrid),
                new PropertyMetadata(0, OnLayoutChanged));

        public static readonly DependencyProperty SeatGroupSizeProperty =
            DependencyProperty.Register(nameof(SeatGroupSize), typeof(int), typeof(SeatGrid),
                new PropertyMetadata(1, OnLayoutChanged));

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public int SeatGroupSize
        {
            get => (int)GetValue(SeatGroupSizeProperty);
            set => SetValue(SeatGroupSizeProperty, value);
        }

        public SeatGrid()
        {
            InitializeComponent();
            Loaded += (_, __) => Build();
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SeatGrid sg && sg.IsLoaded)
                sg.Build();
        }

        private void Build()
        {
            PART_Grid.Children.Clear();

            var r = Math.Max(0, Rows);
            var c = Math.Max(0, Columns);
            if (r == 0 || c == 0) return;

            int total = r * c;
            int group = Math.Max(1, SeatGroupSize);

            // Görsel koltuklar
            for (int i = 0; i < total; i++)
            {
                bool alt = ((i / group) % 2) == 1;

                var seat = new Border
                {
                    Width = 24,
                    Height = 24,
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(alt ? Color.FromRgb(180, 200, 220)
                                                         : Color.FromRgb(140, 170, 200)),
                    BorderBrush = Brushes.DimGray,
                    BorderThickness = new Thickness(0.5)
                };

                PART_Grid.Children.Add(seat);
            }

            PART_Grid.Rows = r;
            PART_Grid.Columns = c;
        }
    }
}

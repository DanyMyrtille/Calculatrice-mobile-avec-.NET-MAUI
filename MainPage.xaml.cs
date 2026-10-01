namespace Calculatrice;

public partial class MainPage : ContentPage
{
    private readonly CalculatorEngine _calc = new();
    private bool? _isLandscape;

    public MainPage()
    {
        InitializeComponent();

        // Aligne l'opération à droite même quand elle est courte
        ExpressionScroll.SizeChanged += (_, _) =>
            ExpressionLabel.MinimumWidthRequest = ExpressionScroll.Width;

        UpdateDisplay();
    }

    /// <summary>
    /// Portrait : afficheur en haut, clavier en bas.
    /// Paysage : afficheur à gauche, clavier à droite (évite un clavier écrasé).
    /// </summary>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;

        bool landscape = width > height;
        if (_isLandscape == landscape) return;

        _isLandscape = landscape;
        ApplyOrientation(landscape);
    }

    private void ApplyOrientation(bool landscape)
    {
        RootGrid.RowDefinitions.Clear();
        RootGrid.ColumnDefinitions.Clear();

        if (landscape)
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.4, GridUnitType.Star)));

            Grid.SetRow(DisplayBorder, 0);
            Grid.SetColumn(DisplayBorder, 0);
            Grid.SetRow(Keypad, 0);
            Grid.SetColumn(Keypad, 1);
        }
        else
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(2.2, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(DisplayBorder, 0);
            Grid.SetColumn(DisplayBorder, 0);
            Grid.SetRow(Keypad, 1);
            Grid.SetColumn(Keypad, 0);
        }

        foreach (var key in Keypad.Children.OfType<Button>())
            key.FontSize = landscape ? 20 : 28;

        UpdateDisplay();
    }

    private void OnKeyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;

        switch (b.ClassId)
        {
            case "C": _calc.Clear(); break;
            case "BACK": _calc.Backspace(); break;
            case "PCT": _calc.Percent(); break;
            case "SIGN": _calc.ToggleSign(); break;
            case "DOT": _calc.Decimal(); break;
            case "EQ": _calc.Evaluate(); break;
            case "+" or "-" or "*" or "/": _calc.Operator(b.ClassId[0]); break;
            default:
                if (b.ClassId.Length == 1 && char.IsDigit(b.ClassId[0]))
                    _calc.Digit(b.ClassId[0]);
                break;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        ResultLabel.Text = _calc.Result;
        ExpressionLabel.Text = _calc.Expression;

        ResultLabel.TextColor = _calc.IsError ? Colors.Tomato : Colors.White;
        ExpressionLabel.TextColor = _calc.IsError ? Colors.Tomato : Color.FromArgb("#8E8E93");

        // Réduction de la police selon la longueur : jamais de débordement
        int len = _calc.Result.Length;
        double size = len <= 8 ? 56 : len <= 11 ? 44 : len <= 14 ? 34 : 28;
        if (_isLandscape == true) size *= 0.7;
        ResultLabel.FontSize = size;

        // L'opération longue défile jusqu'à sa fin
        _ = ExpressionScroll.ScrollToAsync(ExpressionLabel, ScrollToPosition.End, false);
    }
}

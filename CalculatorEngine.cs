using System.Globalization;

namespace Calculatrice;

/// <summary>
/// Logique de calcul pure (aucune dépendance à l'interface).
/// Utilise decimal pour éviter les erreurs de type 0.1 + 0.2.
/// </summary>
public class CalculatorEngine
{
    private const int MaxDigits = 15;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private string _input = "0";
    private decimal? _left;
    private char? _op;
    private bool _newEntry = true;
    private string _expression = "";

    public bool IsError { get; private set; }
    private string _errorMessage = "";

    /// <summary>Opération en cours, affichée au-dessus du résultat.</summary>
    public string Expression => IsError ? _errorMessage : _expression;

    /// <summary>Nombre affiché (virgule décimale à la française).</summary>
    public string Result => IsError ? "Erreur" : _input.Replace('.', ',');

    public void Digit(char d)
    {
        if (IsError) Clear();
        if (_op == null && _newEntry) _expression = "";

        if (_newEntry || _input == "0") _input = d.ToString();
        else if (_input == "-0") _input = "-" + d;
        else if (_input.Count(char.IsDigit) < MaxDigits) _input += d;

        _newEntry = false;
    }

    public void Decimal()
    {
        if (IsError) Clear();
        if (_op == null && _newEntry) _expression = "";

        if (_newEntry)
        {
            _input = "0.";
            _newEntry = false;
        }
        else if (!_input.Contains('.'))
        {
            _input += ".";
        }
    }

    public void Operator(char op)
    {
        if (IsError) return;

        if (_op != null && !_newEntry)
        {
            // Enchaînement : 2 + 3 + ... => on calcule 2 + 3 d'abord
            if (!TryCompute(_left!.Value, _op.Value, Parse(), out var r)) return;
            _left = r;
            _input = ToText(r);
        }
        else if (_op == null)
        {
            _left = Parse();
        }
        // Si _op != null et _newEntry : simple remplacement de l'opérateur

        _op = op;
        _newEntry = true;
        _expression = $"{Fmt(_left!.Value)} {Symbol(op)}";
    }

    public void Evaluate()
    {
        if (IsError || _op == null) return;

        var left = _left!.Value;
        var op = _op.Value;
        var right = Parse();

        if (!TryCompute(left, op, right, out var r)) return;

        _expression = $"{Fmt(left)} {Symbol(op)} {Fmt(right)} =";
        _input = ToText(r);
        _left = null;
        _op = null;
        _newEntry = true;
    }

    public void Clear()
    {
        _input = "0";
        _left = null;
        _op = null;
        _newEntry = true;
        _expression = "";
        IsError = false;
        _errorMessage = "";
    }

    public void Backspace()
    {
        if (IsError) { Clear(); return; }
        if (_newEntry) return;

        _input = (_input.Length <= 1 || (_input.Length == 2 && _input[0] == '-'))
            ? "0"
            : _input[..^1];
    }

    public void ToggleSign()
    {
        if (IsError) return;

        if (_newEntry && _op != null)
        {
            // Opérateur saisi, on commence directement un nombre négatif
            _input = "-0";
            _newEntry = false;
            return;
        }

        if (_input == "0") return;
        _input = _input.StartsWith('-') ? _input[1..] : "-" + _input;
    }

    public void Percent()
    {
        if (IsError) return;

        var cur = Parse();
        // 200 + 10 % => 10 % de 200 ; sinon simple division par 100
        var v = (_left.HasValue && _op is '+' or '-')
            ? _left!.Value * cur / 100
            : cur / 100;

        _input = ToText(v);
        _newEntry = true;
    }

    // ---------- Internes ----------

    private bool TryCompute(decimal a, char op, decimal b, out decimal result)
    {
        result = 0;
        try
        {
            switch (op)
            {
                case '+': result = a + b; break;
                case '-': result = a - b; break;
                case '*': result = a * b; break;
                case '/':
                    if (b == 0) { SetError("Division par zéro impossible"); return false; }
                    result = a / b;
                    break;
            }
            return true;
        }
        catch (OverflowException)
        {
            SetError("Nombre trop grand");
            return false;
        }
    }

    private void SetError(string message)
    {
        IsError = true;
        _errorMessage = message;
        _left = null;
        _op = null;
        _newEntry = true;
    }

    private decimal Parse() => decimal.Parse(_input, NumberStyles.Number, Inv);

    private static string ToText(decimal v) =>
        Math.Round(v, 10).ToString("0.##########", Inv);

    private static string Fmt(decimal v) => ToText(v).Replace('.', ',');

    private static string Symbol(char op) => op switch
    {
        '*' => "×",
        '/' => "÷",
        '-' => "−",
        _ => "+"
    };
}

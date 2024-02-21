using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Calculator.Operator;

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _output = "0";
        public string Output
        {
            get { return _output; }
            set { _output = value; OnPropertyChanged(); }
        }
        private List<string> _inputs;

        private const int MAX_INPUT_LENGHT = 12;

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _inputs = new List<string>();
        }

        private void NumBtn_Click(object sender, RoutedEventArgs e)
        {
            var number = $"{((Button)sender).Name[^1]}";
            if (int.TryParse(number, out var result))
            {
                var numberString = number.ToString();

                if (_inputs.Count == MAX_INPUT_LENGHT) return;

                if (_inputs.Count > 0 && int.TryParse(_inputs[^1], out var lastNumber))
                {
                    _inputs[^1] += numberString;
                }
                else _inputs.Add(numberString);

                if (_output.Equals("0")) Output = "";
                Output += numberString;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_inputs.Count == 0) return;
            var lastInput = _inputs[^1];
            if (int.TryParse(lastInput, out var result))
            {
                var numberToString = lastInput.ToString();
                if (numberToString.Length > 1)
                {
                    numberToString = numberToString.Substring(0, numberToString.Length - 1);
                    _inputs[^1] = numberToString;
                }
                else RemoveLastInput();
            }
            else RemoveLastInput();

            if (_inputs.Count == 0)
            {
                Output = "0";
                return;
            }
            Output = GetOutputFromInputs();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearInputAndOutput();
        }
        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_inputs.Count == MAX_INPUT_LENGHT) return;

            if (_inputs.Count > 0 && _inputs[^1].Equals("-")) { return; }
            if (_inputs[^1].Equals("0")) _inputs[^1] = "_";
            else _inputs.Add("-");
            Output += "-";
        }

        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckOperator("+");
        }
        private void MulBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckOperator("*");
        }

        private void CheckOperator(string op)
        {
            if (_inputs.Count == MAX_INPUT_LENGHT) return;

            if (_output.Equals("0") || _inputs[^1].Equals(op)) { return; }
            _inputs.Add(op);
            Output += op;
        }

        private void EqualBtn_Click(object sender, RoutedEventArgs e)
        {
            var temporalResult = 0;

            while (IsThereOperatorsLeft())
            {
                var nextOperator = GetNextOperator();

                int leftNumber, rightNumber;

                if (nextOperator.Index == 0)
                {
                    if (nextOperator.Index + 1 < _inputs.Count) //There is a number at right of +
                    {
                        rightNumber = int.Parse(_inputs[nextOperator.Index + 1]);

                        temporalResult = temporalResult = GetResult(0, rightNumber, nextOperator.OType); ;
                        _inputs.RemoveAt(nextOperator.Index);
                        _inputs[nextOperator.Index] = temporalResult.ToString();
                    }
                    else
                    {
                        ClearInputAndOutput();
                        return;
                    }
                    continue;
                }

                leftNumber = int.Parse(_inputs[nextOperator.Index - 1]);

                if (nextOperator.Index + 1 < _inputs.Count) //There is a number at right of +
                {
                    rightNumber = int.Parse(_inputs[nextOperator.Index + 1]);

                    temporalResult = GetResult(leftNumber, rightNumber, nextOperator.OType);
                    _inputs.RemoveAt(nextOperator.Index - 1);
                    _inputs.RemoveAt(nextOperator.Index - 1);
                    _inputs[nextOperator.Index - 1] = temporalResult.ToString();
                }
                else //There is a number at left of +
                {
                    temporalResult = leftNumber;
                    _inputs.RemoveAt(nextOperator.Index - 1);
                    _inputs[nextOperator.Index - 1] = temporalResult.ToString();
                }
            }

            Output = _inputs[0];
        }

        private bool IsThereOperatorsLeft()
        {
            return _inputs.Contains("*") || _inputs.Contains("+") || _inputs.Contains("-");
        }

        private int GetResult(int numberLeft, int numberRight, OperatorType type)
        {
            switch (type)
            {
                case OperatorType.Plus:
                    return numberLeft + numberRight;
                case OperatorType.Minus:
                    return numberLeft - numberRight;
                case OperatorType.Mul:
                    return numberLeft * numberRight;
                default: return 0;
            }
        }

        private Operator GetNextOperator()
        {
            if (_inputs.Contains("*"))
            {
                return new Operator
                {
                    Index = _inputs.IndexOf("*"),
                    OType = Operator.OperatorType.Mul
                };
            }
            if (_inputs.Contains("+"))
            {
                return new Operator
                {
                    Index = _inputs.IndexOf("+"),
                    OType = Operator.OperatorType.Plus
                };
            }
            if (_inputs.Contains("-"))
            {
                return new Operator
                {
                    Index = _inputs.IndexOf("-"),
                    OType = Operator.OperatorType.Minus
                };
            }

            return null;
        }

        private void ClearInputAndOutput()
        {
            _inputs.Clear();
            Output = "0";
        }
        private void RemoveLastInput()
        {
            _inputs.RemoveAt(_inputs.Count - 1);
        }

        private string GetOutputFromInputs()
        {
            var stringBuilder = new StringBuilder();
            foreach (var input in _inputs)
            {
                stringBuilder.Append(input);
            }
            return stringBuilder.ToString();
        }
    }

    public class Operator
    {
        public int Index { get; set; }
        public OperatorType OType { get; set; }

        public enum OperatorType
        {
            Plus, Minus, Mul, Div
        }
    }
}
using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AlCalc
{
    public partial class FormMain : Form
    {
        //最后输入的元素的类型
        enum LastInput
        {
            number,
            unaryOperators,
            binaryOperators,
        };

        //输入缓冲区
        private string currentText
        {
            get
            {
                return labelResult.Text;
            }
            set
            {
                //出错后只可清除
                if (isError)
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                string text = value;

                if (text == "-0")
                    text = "0";

                //调整字号
                float emSize = 18;
                if (text.Length > 12)
                    emSize = 14;
                if (text.Length > 16)
                    emSize = 11;
                if (text.Length > 19)
                    emSize = 9;
                labelResult.Font = new System.Drawing.Font(labelResult.Font.FontFamily, emSize);

                //
                labelExpression.Text = string.Join(" ", previousInputs);
                labelResult.Text = text;
            }
        }
        //当前数字
        private double currentNumber = 0;
        //前一数字
        private double previousNumber = 0;
        //二元操作符
        private string binaryOperator = null;
        //二元操作数
        private double? binaryNumber = null;
        //是否输入过小数点
        private bool isInputedDot
        {
            get
            {
                return currentText.Contains(".");
            }
        }
        //上次输入的类型
        private LastInput typeLastInput = LastInput.number;
        //之前的表达式
        private List<string> previousInputs = new List<string>();
        //是否发生过错误
        private bool isError = false;

        public FormMain()
        {
            InitializeComponent();
        }

        //二元计算 previousNumber = previousNumber <binaryOperator> currentNumber
        private double BinaryCalc(double a, double b, string opt)
        {
            double result = a;
            switch (opt)
            {
                case "+":
                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    if (b == 0)
                    {
                        currentText = "除数不能为零";
                        isError = true;
                    }
                    break;
            }
            return result;
        }
        private double BinaryCalc(double number)
        {
            return BinaryCalc(number, (double)binaryNumber, binaryOperator);
        }

        //输入数字
        private void buttonNumbers_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            //按了非数字键再按数字键会输入一个新的数字
            if (typeLastInput != LastInput.number)
                buttonClearentry_Click(buttonClearentry, null);

            //清除首位的0
            if (currentText == "0")
                currentText = "";

            //输入时最多可以有16个有效数字
            int lengthLimit = 16;
            if (isInputedDot) ++lengthLimit;
            if (currentText.StartsWith("0.")) ++lengthLimit;
            if (currentText.Length < lengthLimit)
                currentText += (sender as Button).Text;
            else
                System.Media.SystemSounds.Beep.Play();

            //按了数字键
            typeLastInput = LastInput.number;
        }
        private void buttonDot_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            //上次按的不是数字先输入0
            if (typeLastInput != LastInput.number)
                buttonNumbers_Click(buttonNumber0, null);

            //最多可以有一个小数点
            if (!isInputedDot)
            {
                //输入小数点
                currentText += ".";

                //按了数字键
                typeLastInput = LastInput.number;
            }
            else
                System.Media.SystemSounds.Beep.Play();
        }

        //一元运算符(相反数 平方根 倒数 百分数)
        private void buttonUnaryOperators_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            //输入数字取反特殊处理
            if (typeLastInput == LastInput.number && sender == buttonInverse)
            {
                if (currentText.StartsWith("-"))
                    currentText = currentText.Remove(0, 1);
                else
                    currentText = "-" + currentText;
            }
            else
            {
                if (typeLastInput == LastInput.number)
                    currentNumber = double.Parse(currentText);

                //操作number
                if (sender == buttonInverse)
                    currentNumber = -currentNumber;
                else if (sender == buttonRadical)
                {
                    if (currentNumber < 0)
                    {
                        currentText = "无效输入";
                        isError = true;
                    }
                    currentNumber = System.Math.Sqrt(currentNumber);
                }
                else if (sender == buttonReciprocal)
                {
                    if (currentNumber == 0)
                    {
                        currentText = "除数不能为零";
                        isError = true;
                    }
                    currentNumber = 1 / currentNumber;
                }
                else if (sender == buttonPercent)
                    currentNumber = previousNumber * currentNumber * 0.01;

                //更新表达式
                if (typeLastInput == LastInput.unaryOperators && previousInputs.Count > 0)
                    previousInputs.RemoveAt(previousInputs.Count - 1);
                previousInputs.Add(currentNumber.ToString());

                currentText = currentNumber.ToString();

                //按了一元运算符
                typeLastInput = LastInput.unaryOperators;
            }
        }

        //二元运算符(加 减 乘 除)
        private void buttonBinaryOperators_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (typeLastInput == LastInput.number)
                currentNumber = double.Parse(currentText);
            if (previousInputs.Count == 0)
                previousInputs.Add(currentNumber.ToString());
            if (typeLastInput == LastInput.number || typeLastInput == LastInput.unaryOperators)
                if (previousInputs.Count > 1)
                {
                    binaryNumber = currentNumber;
                    currentNumber = BinaryCalc(previousNumber);
                }
            if (typeLastInput == LastInput.binaryOperators)
                previousInputs.RemoveAt(previousInputs.Count - 1);

            previousNumber = currentNumber;

            //设置二元运算符
            binaryOperator = (sender as Button).Text;
            previousInputs.Add(binaryOperator);

            //等待操作数
            binaryNumber = null;

            currentText = currentNumber.ToString();

            //按了二元运算符
            typeLastInput = LastInput.binaryOperators;
        }

        //等号
        private void buttonEqual_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (typeLastInput == LastInput.number)
                currentNumber = double.Parse(currentText);
            if (binaryNumber == null)
            {
                binaryNumber = currentNumber;
                if (binaryOperator != null)
                    currentNumber = BinaryCalc(previousNumber);
            }
            else
            {
                if (binaryOperator != null)
                    currentNumber = BinaryCalc(currentNumber);
            }
            
            previousInputs.Clear();

            currentText = currentNumber.ToString();

            //按了等号
            typeLastInput = LastInput.unaryOperators;
        }

        //退格
        private void buttonBack_Click(object sender, EventArgs e)
        {
            //出错后只可清除
            if (isError)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (typeLastInput == LastInput.number)
            {
                currentText = currentText.Remove(currentText.Length - 1, 1);
                if (currentText.Length == 0) currentText = "0";
            }
            else
                System.Media.SystemSounds.Beep.Play();
        }

        //清除当前
        private void buttonClearentry_Click(object sender, EventArgs e)
        {
            if (isError)
                buttonClear_Click(buttonClear, null);

            if (typeLastInput == LastInput.unaryOperators && previousInputs.Count > 0)
                previousInputs.RemoveAt(previousInputs.Count - 1);
            currentText = "0";
            currentNumber = 0;
            typeLastInput = LastInput.number;
        }

        //清除所有
        private void buttonClear_Click(object sender, EventArgs e)
        {
            isError = false;
            previousInputs.Clear();
            currentText = "0";
            currentNumber = 0;
            previousNumber = 0;
            binaryOperator = null;
            binaryNumber = null;
            typeLastInput = LastInput.number;
        }

        //快捷键
        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Shift | Keys.D5:
                    buttonUnaryOperators_Click(buttonPercent, null);
                    break;
                case Keys.F9:
                    buttonUnaryOperators_Click(buttonInverse, null);
                    break;
                case Keys.Add:
                case Keys.Shift | Keys.Oemplus:
                    buttonBinaryOperators_Click(buttonPlus, null);
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    buttonBinaryOperators_Click(buttonMinus, null);
                    break;
                case Keys.Multiply:
                case Keys.Shift | Keys.D8:
                    buttonBinaryOperators_Click(buttonMultiply, null);
                    break;
                case Keys.Divide:
                case Keys.OemQuestion:
                    buttonBinaryOperators_Click(buttonDivide, null);
                    break;
                case Keys.R:
                case Keys.Shift | Keys.R:
                    buttonUnaryOperators_Click(buttonReciprocal, null);
                    break;
                case Keys.Shift | Keys.D2:
                    buttonUnaryOperators_Click(buttonRadical, null);
                    break;
                case Keys.D0:
                case Keys.NumPad0:
                    buttonNumbers_Click(buttonNumber0, null);
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    buttonNumbers_Click(buttonNumber1, null);
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    buttonNumbers_Click(buttonNumber2, null);
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    buttonNumbers_Click(buttonNumber3, null);
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    buttonNumbers_Click(buttonNumber4, null);
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    buttonNumbers_Click(buttonNumber5, null);
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    buttonNumbers_Click(buttonNumber6, null);
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    buttonNumbers_Click(buttonNumber7, null);
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    buttonNumbers_Click(buttonNumber8, null);
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    buttonNumbers_Click(buttonNumber9, null);
                    break;
                case Keys.Enter:
                case Keys.Oemplus:
                    buttonEqual_Click(buttonEqual, null);
                    break;
                case Keys.OemPeriod:
                case Keys.Decimal:
                    buttonDot_Click(buttonDot, null);
                    break;
                case Keys.Back:
                    buttonBack_Click(buttonBack, null);
                    break;
                case Keys.Escape:
                    buttonClear_Click(buttonClear, null);
                    break;
                case Keys.Delete:
                    buttonClearentry_Click(buttonClearentry, null);
                    break;
                case Keys.Tab:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    break;
                default:
                    base.ProcessDialogKey(keyData);
                    break;
            }
            return false;
        }
    }
}

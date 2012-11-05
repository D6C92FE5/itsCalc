using System;
//using System.Collections.Generic;
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
            equal,
        };

        //默认缓冲区大小
        private const int StringLength = 0x20;
        //输入缓冲区
        private string currentText = "0";
        //当前数字
        private double currentNumber = 0;
        //前一数字
        private double previousNumber = 0;
        //操作符
        private char binaryOperator = ' ';
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
        private string previousExpression = "";

        public FormMain()
        {
            InitializeComponent();
        }

        //更新labelResult的内容
        private void UpdateLabel(object objectToShow)
        {
            //格式化
            string text;
            if (objectToShow is double)
                text = ((double)objectToShow).ToString();
            else
                text = objectToShow.ToString();

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
            labelResult.Text = text;
        }

        //二元计算 previousNumber = previousNumber <binaryOperator> currentNumber
        private void BinaryCalc(ref double a, double b, char opt)
        {
            switch (opt)
            {
                case '+':
                    a += b;
                    break;
                case '-':
                    a -= b;
                    break;
                case '*':
                    a *= b;
                    break;
                case '/':
                    a /= b;
                    break;
                case ' ':
                    a = b;
                    break;
            }
        }
        private void BinaryCalc()
        {
            BinaryCalc(ref previousNumber, currentNumber, binaryOperator);
        }

        //输入数字
        private void buttonNumbers_Click(object sender, EventArgs e)
        {
            //按完等号再按数字相当于开始一次全新的计算
            if (typeLastInput == LastInput.equal)
                buttonClear_Click(buttonClear, null);

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

            //显示计算结果
            UpdateLabel(currentText);

            //按了数字键
            typeLastInput = LastInput.number;
        }
        private void buttonDot_Click(object sender, EventArgs e)
        {
            //最多可以有一个小数点
            if (!isInputedDot)
            {
                //上次按的不是数字先输入0
                if (typeLastInput != LastInput.number)
                    buttonNumbers_Click(buttonNumber0, null);

                //输入小数点
                currentText += ".";

                //显示计算结果
                UpdateLabel(currentText);

                //按了数字键
                typeLastInput = LastInput.number;
            }
            else
                System.Media.SystemSounds.Beep.Play();
        }

        //一元运算符(相反数 平方根 倒数 百分数)
        //一元运算符是操作currentNumber的
        private void buttonUnaryOperators_Click(object sender, EventArgs e)
        {
            //不同情况以不同方式设置currentNumber
            switch (typeLastInput)
            {
                case LastInput.number:
                    currentNumber = double.Parse(currentText);
                    break;
                case LastInput.unaryOperators:
                    break;
                case LastInput.binaryOperators:
                    break;
                case LastInput.equal:
                    currentNumber = previousNumber;
                    binaryOperator = ' ';
                    break;
            }

            //操作currentNumber
            if (sender == buttonInverse)
                currentNumber = -currentNumber;
            else if (sender == buttonRadical)
                currentNumber = System.Math.Sqrt(currentNumber);
            else if (sender == buttonReciprocal)
                currentNumber = 1 / currentNumber;
            else if (sender == buttonPercent)
                currentNumber = previousNumber * currentNumber * 0.01;

            //显示计算结果
            UpdateLabel(currentNumber);

            //取反之后可以继续输入数字
            if ((typeLastInput == LastInput.number) && (sender == buttonInverse))
            {
                if (currentText != "0")
                    if (currentText[0] != '-')
                        currentText = currentText.Insert(0, "-");
                    else
                        currentText = currentText.Remove(0, 1);
            }
            else
                //按了一元运算符
                typeLastInput = LastInput.unaryOperators;
        }

        //二元运算符(加 减 乘 除)
        //先计算并显示previousNumber, 再设置二元运算符
        private void buttonBinaryOperators_Click(object sender, EventArgs e)
        {
            //不同情况以不同方式设置currentNumber
            switch (typeLastInput)
            {
                case LastInput.number:
                    currentNumber = double.Parse(currentText);
                    break;
                case LastInput.unaryOperators:
                case LastInput.binaryOperators:
                    binaryOperator = ' ';
                    break;
                case LastInput.equal:
                    currentNumber = previousNumber;
                    binaryOperator = ' ';
                    break;
            }

            //计算previousNumber currentNumber
            BinaryCalc();
            currentNumber = previousNumber;

            //显示计算结果
            UpdateLabel(previousNumber);

            //设置二元运算符
            binaryOperator = (sender as Button).Text[0];

            //按了二元运算符
            typeLastInput = LastInput.binaryOperators;

            previousExpression += " " + previousExpression;
            labelExpression.Text = (previousExpression + " " + binaryOperator).Trim();
        }

        //等号
        private void buttonEqual_Click(object sender, EventArgs e)
        {
            //不同情况以不同方式设置currentNumber
            switch (typeLastInput)
            {
                case LastInput.number:
                    currentNumber = double.Parse(currentText);
                    break;
                case LastInput.unaryOperators:
                    break;
                case LastInput.binaryOperators:
                    break;
                case LastInput.equal:
                    break;
            }

            //计算
            BinaryCalc();

            //显示计算结果
            UpdateLabel(previousNumber);

            //按了等号
            typeLastInput = LastInput.equal;
        }

        //退格
        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (typeLastInput == LastInput.number)
            {
                currentText = currentText.Remove(currentText.Length - 1, 1);
                if (currentText.Length == 0) currentText += "0";
                UpdateLabel(currentText);
            }
            else
                System.Media.SystemSounds.Beep.Play();
        }

        //清除当前
        private void buttonClearentry_Click(object sender, EventArgs e)
        {
            currentText = "0";
            currentNumber = 0;
            typeLastInput = LastInput.number;
            UpdateLabel(currentText);
        }

        //清除所有
        private void buttonClear_Click(object sender, EventArgs e)
        {
            currentText = "0";
            currentNumber = 0;
            previousNumber = 0;
            binaryOperator = ' ';
            typeLastInput = LastInput.number;
            previousExpression = "";
            UpdateLabel(currentText);
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

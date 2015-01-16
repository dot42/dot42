using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Calculator")]

namespace Calculator
{
    [Activity(Icon = "Icon")]
    public class MainActivity : Activity
    {
        private enum Operations
        {
            None,
            Add,
            Sub,
            Mul,
            Div
        }

        private TextView tbValue;
        private Operations operation;
        private int rightOperand;
        private int leftOperand;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            tbValue = (TextView) FindViewById(R.Ids.tbValue);
            var btn = (Button)FindViewById(R.Ids.cmd0);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd1);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd2);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd3);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd4);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd5);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd6);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd7);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd8);
            btn.Click += BtnOnClick;
            btn = (Button)FindViewById(R.Ids.cmd9);
            btn.Click += BtnOnClick;

            btn = (Button)FindViewById(R.Ids.cmdAdd);
            btn.Click += (s, x) => Calc(Operations.Add);
            btn = (Button)FindViewById(R.Ids.cmdSub);
            btn.Click += (s, x) => Calc(Operations.Sub);
            btn = (Button)FindViewById(R.Ids.cmdMul);
            btn.Click += (s, x) => Calc(Operations.Mul);
            btn = (Button)FindViewById(R.Ids.cmdDiv);
            btn.Click += (s, x) => Calc(Operations.Div);
            btn = (Button)FindViewById(R.Ids.cmdEnter);
            btn.Click += BtnEnterClick;
            btn = (Button)FindViewById(R.Ids.cmdClear);
            btn.Click += (s, x) => Clear();
        }

        private void BtnOnClick(object sender, EventArgs eventArgs)
        {
            var btn = (Button) sender;
            var number = int.Parse(btn.Text.ToString());
            rightOperand = (rightOperand * 10) + number;
            FormatValue();
        }

        private void Clear()
        {
            leftOperand = 0;
            rightOperand = 0;
            operation = Operations.None;
            FormatValue();
        }

        private void Calc(Operations operation)
        {
            Calc();
            this.operation = operation;
            leftOperand = rightOperand;
            rightOperand = 0;
            FormatValue();
        }

        private void BtnEnterClick(object sender, EventArgs eventArgs)
        {
            Calc();
        }

        private void Calc()
        {
            try
            {
                switch (operation)
                {
                    case Operations.None:
                        leftOperand = rightOperand;
                        break;
                    case Operations.Add:
                        leftOperand += rightOperand;
                        break;
                    case Operations.Sub:
                        leftOperand -= rightOperand;
                        break;
                    case Operations.Mul:
                        leftOperand *= rightOperand;
                        break;
                    case Operations.Div:
                        leftOperand /= rightOperand;
                        break;
                }
                rightOperand = leftOperand;
                leftOperand = 0;
                operation = Operations.None;
                FormatValue();
            }catch(Exception ex)
            {
                
            }
        }

        private void FormatValue()
        {
            string text;
            var addLeftOperand = true;
            switch (operation)
            {
                default:
                    text = string.Empty;
                    addLeftOperand = false;
                    // Do nothing
                    break;
                case Operations.Add:
                    text = " + ";
                    break;
                case Operations.Sub:
                    text = " - ";
                    break;
                case Operations.Mul:
                    text = " * ";
                    break;
                case Operations.Div:
                    text = " / ";
                    break;
            }
            if (addLeftOperand)
            {
                text = leftOperand.ToString() + text;
                if (rightOperand != 0)
                    text += rightOperand;
            }
            else
            {
                text = rightOperand.ToString();
            }
            tbValue.Text = text;
        }
    }
}

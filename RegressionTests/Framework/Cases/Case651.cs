using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Junit.Framework;

namespace Cases.Case651
{
    public class Test : TestCase
    {
        string lblPesoValorText;
        string CodigoTextBox0Text;
        string CodigoTextBox1Text;
        string CodigoTextBox2Text;
        string CodigoTextBox3Text;
        string CodigoTextBox4Text;

        public void test651()
        {
            OnProgressChanged(this, new ProgressChangedEventArgs(50, false));
            AssertTrue(true);
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs x)
        {
            //var lblPesoValor = FindViewById<TextView>(R.Ids.lblPesoValor);
            lblPesoValorText = x.Progress.ToString();

            //// Dosis 
            double value = 0;
            double peso = (double)x.Progress;
            //var CodigoTextBox0 = FindViewById<TextView>(R.Ids.CodigoTextBox0);
            //value = ObternerValor(peso, 0.15D, 1); 
            value = peso * 0.15D / 1;
            CodigoTextBox0Text = String.Format("{0:0.00}", value) + " " + "ml";
            //var CodigoTextBox1 = FindViewById<TextView>(R.Ids.CodigoTextBox1);
            //value = ObternerValor(peso, 0.33D, 1); 
            value = peso * 0.33D / 1;
            CodigoTextBox1Text = String.Format("{0:0.00}", value) + " " + "ml";
            //var CodigoTextBox2 = FindViewById<TextView>(R.Ids.CodigoTextBox2);
            //value = ObternerValor(peso, 0.33D, 2); 
            value = peso * 0.33D / 1;
            CodigoTextBox2Text = String.Format("{0:0.00}", value) + " " + "ml";
            //var CodigoTextBox3 = FindViewById<TextView>(R.Ids.CodigoTextBox3);
            //value = ObternerValor(peso, 0.33D, 2); 
            value = peso * 0.33D / 2;
            CodigoTextBox3Text = String.Format("{0:0.00}", value) + " " + "ml";
            //var CodigoTextBox4 = FindViewById<TextView>(R.Ids.CodigoTextBox4);
            //value = ObternerValor(peso, 0.33D, 2); 
            value = peso * 0.33D / 2;
            CodigoTextBox4Text = String.Format("{0:0.00}", value) + " " + "ml";
        }
    }
}

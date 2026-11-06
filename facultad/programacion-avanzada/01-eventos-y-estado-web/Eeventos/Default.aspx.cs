using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eeventos
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TxCalendario.Text = DateTime.Now.ToString("yyyy-MM-dd");

                Timer2.Interval = 5000;
                Timer2.Enabled = true;

                Timer1.Interval = 1000;
                Timer1.Enabled = true;

            }
        }

        private void Temporizador1()
        {
            TxTimer1.Text = DateTime.Now.ToString();


        }

        private void Temporizador2()
        {
            TxTimer2.Text = DateTime.Now.ToString();

        }

        private void Temporizador3()
        {
            txborrar.Text = DateTime.Now.ToString();
            Timer3.Interval = 1000;
            Timer3.Enabled = true;
        }
        protected void BtSaludar_Click(object sender, EventArgs e)
        {
            //Response.Write("Hola " + TxIngresar.Text);
            if (TxIngresar.Text == string.Empty)
            {
                LbMensaje.Text = "Debe ingresar un Nombre";
            }
            else
            {
                LbMensaje.Text = "Hola " + TxIngresar.Text;
            }
        }

        public int CalcularEdad()
        {
            int edad = DateTime.Now.Year - Convert.ToDateTime(TxCalendario.Text).Year;
            return edad;
        }

        protected void BtCalcularEdad_Click(object sender, EventArgs e)
        {
            //CalcularEdad();
            //LbMensaje.Text = "Tu edad es: " + CalcularEdad().ToString();
            LbMensaje.Text = "Tu edad es: " + BIZ.CalcularEdad.Calcular(Convert.ToDateTime(TxCalendario.Text)).ToString();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            //Temporizador1();
            Temporizador1();
        }

        protected void Timer2_Tick1(object sender, EventArgs e)
        {
            Temporizador2();
        }

        protected void Timer3_Tick(object sender, EventArgs e)
        {
            //Temporizador3();
        }

        protected void BtFin1_Click(object sender, EventArgs e)
        {
            Timer1.Enabled = false;
        }

        protected void BtInicio1_Click(object sender, EventArgs e)
        {
            Timer1.Enabled=true;
        }

        protected void BtInicio2_Click(object sender, EventArgs e)
        {
            Timer2.Enabled = true;
        }

        protected void BtFin2_Click(object sender, EventArgs e)
        {
            Timer2.Enabled = false;
        }

    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CalculadoraWinForms
{
    internal static class Program
    {
        static void Main()
        {
            Application.Run(new FormCalculadora());
        }
    }

    class BaseDeDatos
    {
        string cadena;


        public BaseDeDatos(string c)
        {
            cadena = c;
        }

        public void Guardar(string expresion, double resultado)
        {
            using (var cn = new SqlConnection(cadena))
            {
                cn.Open();
                var cmd = new SqlCommand("INSERT INTO calculos(expresion,resultado) VALUES(@e,@r);", cn);
                cmd.Parameters.AddWithValue("@e", expresion);
                cmd.Parameters.AddWithValue("@r", resultado);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable ObtenerTodos()
        {
            using (var cn = new SqlConnection(cadena))
            {
                cn.Open();
                var cmd = new SqlCommand("SELECT * FROM calculos ORDER BY fecha DESC;", cn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    public class FormHistorial : Form
    {
        DataGridView tabla;

        public FormHistorial(DataTable datos)
        {
            Text = "Historial de cálculos";
            Width = 600;
            Height = 400;
            tabla = new DataGridView();
            tabla.Dock = DockStyle.Fill;
            tabla.ReadOnly = true;
            tabla.AllowUserToAddRows = false;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.DataSource = datos;
            Controls.Add(tabla);
        }
    }

    public class FormCalculadora : Form
    {
        string cadenaConexion = @"Server=.\sqlexpress;Database=calculadora;Trusted_Connection=True;Integrated Security=SSPI;";

        TextBox txtPantalla;
        TableLayoutPanel panel;
        double numero1 = 0;
        string operador = null;
        bool nuevoNumero = true;
        BaseDeDatos db;

        public FormCalculadora()
        {
            Text = "Calculadora";
            Font = new Font(FontFamily.GenericSansSerif, 12);
            Width = 360;
            Height = 520;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            txtPantalla = new TextBox();
            txtPantalla.ReadOnly = true;
            txtPantalla.Text = "0";
            txtPantalla.TextAlign = HorizontalAlignment.Right;
            txtPantalla.Dock = DockStyle.Top;
            txtPantalla.Font = new Font(Font, FontStyle.Bold);
            txtPantalla.Height = 50;

            panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 4;
            panel.RowCount = 6;
            for (int i = 0; i < panel.ColumnCount; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < panel.RowCount; i++) panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / panel.RowCount));

            Controls.Add(panel);
            Controls.Add(txtPantalla);

            db = new BaseDeDatos(cadenaConexion);

            AgregarBotones();
        }

        void AgregarBotones()
        {
            AgregarBoton("C", 0, 0, ClickC);
            AgregarBoton("CE", 1, 0, ClickCE);
            AgregarBoton("x²", 2, 0, ClickCuadrado);
            AgregarBoton("√", 3, 0, ClickRaiz);

            AgregarBoton("7", 0, 1, ClickNumero);
            AgregarBoton("8", 1, 1, ClickNumero);
            AgregarBoton("9", 2, 1, ClickNumero);
            AgregarBoton("/", 3, 1, ClickOperador);

            AgregarBoton("4", 0, 2, ClickNumero);
            AgregarBoton("5", 1, 2, ClickNumero);
            AgregarBoton("6", 2, 2, ClickNumero);
            AgregarBoton("*", 3, 2, ClickOperador);

            AgregarBoton("1", 0, 3, ClickNumero);
            AgregarBoton("2", 1, 3, ClickNumero);
            AgregarBoton("3", 2, 3, ClickNumero);
            AgregarBoton("-", 3, 3, ClickOperador);

            AgregarBoton(".", 0, 4, ClickPunto);
            AgregarBoton("0", 1, 4, ClickNumero);
            AgregarBoton("±", 2, 4, ClickSigno);
            AgregarBoton("+", 3, 4, ClickOperador);

            AgregarBoton("=", 0, 5, ClickIgual);
            AgregarBoton("1/x", 1, 5, ClickInverso);
            AgregarBoton("|x|", 2, 5, ClickAbsoluto);
            AgregarBoton("Historial", 3, 5, ClickHistorial);
        }

        void AgregarBoton(string texto, int col, int fila, EventHandler manejador)
        {
            var b = new Button();
            b.Text = texto;
            b.Dock = DockStyle.Fill;
            b.Margin = new Padding(4);
            b.Click += manejador;
            panel.Controls.Add(b, col, fila);
        }

        void ClickNumero(object sender, EventArgs e)
        {
            var t = ((Button)sender).Text;
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") { txtPantalla.Text = "0"; }
            if (nuevoNumero || txtPantalla.Text == "0")
            {
                txtPantalla.Text = t;
                nuevoNumero = false;
            }
            else
            {
                txtPantalla.Text += t;
            }
        }

        void ClickPunto(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") { txtPantalla.Text = "0"; }
            if (nuevoNumero)
            {
                txtPantalla.Text = "0.";
                nuevoNumero = false;
            }
            else if (!txtPantalla.Text.Contains("."))
            {
                txtPantalla.Text += ".";
            }
        }

        void ClickOperador(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") { return; }
            numero1 = LeerPantalla();
            operador = ((Button)sender).Text;
            nuevoNumero = true;
        }

        void ClickIgual(object sender, EventArgs e)
        {
            if (operador == null || txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            double numero2 = LeerPantalla();
            double r = 0;
            bool valido = true;
            switch (operador)
            {
                case "+": r = numero1 + numero2; break;
                case "-": r = numero1 - numero2; break;
                case "*": r = numero1 * numero2; break;
                case "/":
                    if (numero2 == 0) { MostrarError(); valido = false; }
                    else r = numero1 / numero2;
                    break;
            }
            string expr = numero1.ToString(CultureInfo.InvariantCulture) + " " + operador + " " + numero2.ToString(CultureInfo.InvariantCulture);
            if (valido)
            {
                MostrarNumero(r);
                Guardar(expr, r);
                numero1 = r;
                nuevoNumero = true;
            }
            operador = null;
        }

        void ClickCE(object sender, EventArgs e)
        {
            txtPantalla.Text = "0";
            nuevoNumero = true;
        }

        void ClickC(object sender, EventArgs e)
        {
            txtPantalla.Text = "0";
            numero1 = 0;
            operador = null;
            nuevoNumero = true;
        }

        void ClickSigno(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            if (txtPantalla.Text.StartsWith("-")) txtPantalla.Text = txtPantalla.Text.Substring(1);
            else if (txtPantalla.Text != "0") txtPantalla.Text = "-" + txtPantalla.Text;
        }

        void ClickCuadrado(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            double x = LeerPantalla();
            double r = x * x;
            MostrarNumero(r);
            Guardar(x.ToString(CultureInfo.InvariantCulture) + "^2", r);
            nuevoNumero = true;
        }

        void ClickRaiz(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            double x = LeerPantalla();
            if (x < 0) { MostrarError(); return; }
            double r = Math.Sqrt(x);
            MostrarNumero(r);
            Guardar("√(" + x.ToString(CultureInfo.InvariantCulture) + ")", r);
            nuevoNumero = true;
        }

        void ClickInverso(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            double x = LeerPantalla();
            if (x == 0) { MostrarError(); return; }
            double r = 1.0 / x;
            MostrarNumero(r);
            Guardar("1/(" + x.ToString(CultureInfo.InvariantCulture) + ")", r);
            nuevoNumero = true;
        }

        void ClickAbsoluto(object sender, EventArgs e)
        {
            if (txtPantalla.Text == "Error" || txtPantalla.Text == "∞") return;
            double x = LeerPantalla();
            double r = Math.Abs(x);
            MostrarNumero(r);
            Guardar("|" + x.ToString(CultureInfo.InvariantCulture) + "|", r);
            nuevoNumero = true;
        }

        void ClickHistorial(object sender, EventArgs e)
        {
            try
            {
                var dt = db.ObtenerTodos();
                new FormHistorial(dt).ShowDialog(this);
            }
            catch
            {
                MessageBox.Show("No se pudo cargar el historial. Verifique la conexión a la base de datos.");
            }
        }

        double LeerPantalla()
        {
            double.TryParse(txtPantalla.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double x);
            return x;
        }

        void MostrarNumero(double x)
        {
            txtPantalla.Text = x.ToString(CultureInfo.InvariantCulture);
        }

        void Guardar(string expresion, double r)
        {
            try { db.Guardar(expresion, r); } catch { }
        }

        void MostrarError()
        {
            txtPantalla.Text = "Error";
            nuevoNumero = true;
        }
    }
}

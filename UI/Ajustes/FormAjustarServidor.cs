using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Windows.Forms;
using BLL;
using Entity;
using System.Xml;
using Ldb;

namespace UI
{
    public partial class FormAjustarServidor : Form
    {
        CadenaConexionXMLService cadenaConexionService = new CadenaConexionXMLService();
        CadenaConexionXML cadenaConexion = new CadenaConexionXML();
        string cadenaConsultada;
        string cadena;
        string server;
        string db;
        string newServer;
        string newBd;
        string newUi;
        string connectionString;
        string originalConnection;
        string primeraCadena;
        string segundaCadenaModificada;
        string segundaCadenaOriginal;
        public FormAjustarServidor()
        {
            InitializeComponent();
            EncontrarCadenaDeConexion();
        }
        private string ObtenerCadenaConexion(string xml)
        {
            // Buscar el inicio de la cadena de conexión
            int startIndex = xml.IndexOf("connectionString=\"") + 18;

            // Buscar el final de la cadena de conexión
            int endIndex = xml.IndexOf("\"", startIndex);

            // Extraer la cadena de conexión completa
            string cadenaConexionCompleta = xml.Substring(startIndex, endIndex - startIndex);

            return cadenaConexionCompleta;
        }
        private void ConsultarCadena()
        {
            cadenaConsultada = cadenaConexionService.Consultar();
            string cadenaConexionCompleta = ObtenerCadenaConexion(cadenaConsultada);
            labelConnectionString.Text = cadenaConexionCompleta;
        }
        private void EncontrarCadenaDeConexion()
        { 
            connectionString = ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
            labelConnectionString.Text = connectionString;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach (XmlElement xmlElement in xmlDocument.DocumentElement)
            {
                if (xmlElement.Name.Equals("connectionStrings"))
                {
                    foreach (XmlNode node in xmlElement.ChildNodes)
                    {
                        if (node.Attributes[0].Value == "conexion")
                        {
                            cadena = node.Attributes[1].Value;
                            string[] partesConexion = cadena.Split(';');
                            foreach (string parte in partesConexion)
                            {
                                if (parte.StartsWith("Server="))
                                {
                                    server = parte.Substring("Server=".Length);
                                }
                                else if (parte.StartsWith("Database="))
                                {
                                    db = parte.Substring("Database=".Length);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void btnModificarConexion_Click(object sender, EventArgs e)
        {
            var respuesta = MessageBox.Show("Está seguro de Modificar la conexion a la base de datos?", "Mensaje de Modificacion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (respuesta == DialogResult.Yes)
            {
                ModificarCadenaConexion();
                ConsultarCadena();
            }
            var initializer = new InitDb();
            initializer.InitializeDatabase();
        }

        private void textCadenaConexion_TextChanged(object sender, EventArgs e)
        {
            newServer = textServidor.Text;
        }
        private void textBD_TextChanged(object sender, EventArgs e)
        {
            newBd = textBD.Text;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            newUi = textUI.Text;
        }
        private void ModificarCadenaConexion()
        {
            if (textServidor.Text != "" && textBD.Text!="")
            {
                primeraCadena = "                <add name=" + '"' + "conexion" + '"' + " connectionString=" + '"';
                segundaCadenaModificada = "server=" + newServer + ";Database="+newBd+";Trusted_Connection = True; MultipleActiveResultSets = true" + '"' + " />";
                segundaCadenaOriginal = server + '"' + " />";

                cadenaConexion.Cadena = primeraCadena + segundaCadenaModificada;
                originalConnection = primeraCadena + segundaCadenaOriginal;
                cadenaConexionService.Modificar(cadenaConexion, originalConnection, newUi);
            }
            else
            {
                if (textServidor.Text == "" && textBD.Text != "")
                {
                    primeraCadena = "                <add name=" + '"' + "conexion" + '"' + " connectionString=" + '"';
                    segundaCadenaModificada = "server=" + server + ";Database=" + newBd + ";Trusted_Connection = True; MultipleActiveResultSets = true" + '"' + " />";
                    segundaCadenaOriginal = server + '"' + " />";

                    cadenaConexion.Cadena = primeraCadena + segundaCadenaModificada;
                    originalConnection = primeraCadena + segundaCadenaOriginal;
                    cadenaConexionService.Modificar(cadenaConexion, originalConnection, newUi);
                }
                else
                {
                    if (textServidor.Text != "" && textBD.Text == "")
                    {
                        primeraCadena = "                <add name=" + '"' + "conexion" + '"' + " connectionString=" + '"';
                        segundaCadenaModificada = "server=" + newServer + ";Database=" + db + ";Trusted_Connection = True; MultipleActiveResultSets = true" + '"' + " />";
                        segundaCadenaOriginal = server + '"' + " />";

                        cadenaConexion.Cadena = primeraCadena + segundaCadenaModificada;
                        originalConnection = primeraCadena + segundaCadenaOriginal;
                        cadenaConexionService.Modificar(cadenaConexion, originalConnection, newUi);
                    }
                    else
                    {
                        if (textServidor.Text == "" && textBD.Text == "")
                        {
                            primeraCadena = "                <add name=" + '"' + "conexion" + '"' + " connectionString=" + '"';
                            segundaCadenaModificada = "server=" + server + ";Database=" + db + ";Trusted_Connection = True; MultipleActiveResultSets = true" + '"' + " />";
                            segundaCadenaOriginal = server + '"' + " />";

                            cadenaConexion.Cadena = primeraCadena + segundaCadenaModificada;
                            originalConnection = primeraCadena + segundaCadenaOriginal;
                            cadenaConexionService.Modificar(cadenaConexion, originalConnection, newUi);
                        }
                    }
                }
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}

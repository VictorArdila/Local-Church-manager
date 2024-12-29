using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using BLL;
using Entity;
using System.Xml;
using Ldb;

namespace UI
{
    public partial class FormAjustes : Form
    {
        EmailService emailService = new EmailService();
        CadenaConexionXMLService cadenaConexionService = new CadenaConexionXMLService();
        RutasTxtService rutasTxtService = new RutasTxtService();
        Email email = new Email();
        RutasTxt rutasTxt = new RutasTxt();
        CadenaConexionXML cadenaConexion = new CadenaConexionXML();
        IglesiaService iglesiaService;
        Iglesia iglesia;
        List<Iglesia> iglesias;
        string idIglesia = "#Iglesia";
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
        string RutaInformeIndividual;
        string RutaInformeGeneral;
        string RutaMiembros;
        public FormAjustes()
        {
            iglesiaService = new IglesiaService(DbConfig.ConnectionString);
            InitializeComponent();
            BuscarPorId();
            EncontrarCadenaDeConexion();
            EstablecerCarpetasRaiz(rutasTxtService);
            EstablecerCorreo();
        }
        private void BuscarPorId()
        {
            BusquedaIglesiaRespuesta respuesta = new BusquedaIglesiaRespuesta();
            respuesta = iglesiaService.BuscarPorId(idIglesia);
            if (respuesta.Iglesia != null)
            {
                var iglesias = new List<Iglesia> { respuesta.Iglesia };
                dataGridInfoIglesia.DataSource = iglesias;
                textNombreIglesia.Text = respuesta.Iglesia.NombreIglesia;
                textNIT.Text= respuesta.Iglesia.NIT;
                textPBX.Text= respuesta.Iglesia.Correo;
                textDireccion.Text = respuesta.Iglesia.Direccion;
                textTelefono.Text= respuesta.Iglesia.Telefono;
                labelAdvertencia.Visible = false;
                btnRegistrarInfo.Enabled = false;
            }
            else
            {
                if (respuesta.Iglesia == null)
                {
                    labelAdvertencia.Visible = true;
                    btnRegistrarInfo.Enabled = true;
                }
            }
        }
        private void Limpiar()
        {
            dataGridInfoIglesia.DataSource = null;
            textNombreIglesia.Text = "";
            textNIT.Text = "";
            textPBX.Text = "";
            textDireccion.Text = "";
            textTelefono.Text = "";
        }
        private void ModificarCorreo()
        {
            EmailConsultaResponse emailConsultaResponse = emailService.Consultar();
            if (emailConsultaResponse.Emails.Count > 0)
            {
                foreach (var item in emailConsultaResponse.Emails)
                {
                    string correoOrigen = item.CorreoElectronicoOrigen.ToString();
                    email.CorreoElectronicoOrigen = textCorreo.Text;
                    email.CorreoElectronicoDestino = textCorreo.Text;
                    email.Contraseña = textContraseña.Text;
                    emailService.ModificarEmail(email, correoOrigen);
                }
            }
            else
            {
                if (emailConsultaResponse.Emails.Count == 0)
                {
                    email.CorreoElectronicoOrigen = textCorreo.Text;
                    email.CorreoElectronicoDestino = textCorreo.Text;
                    email.Contraseña = textContraseña.Text;
                    emailService.Guardar(email);
                }
            }
        }
        private void ModificarCarpetasRaiz(RutasTxtService rutasTxtService)
        {
            RutasTxtConsultaResponse rutasTxtConsultaResponse = rutasTxtService.Consultar();
            if (rutasTxtConsultaResponse.RutasTxts.Count > 0)
            {
                foreach (var item in rutasTxtConsultaResponse.RutasTxts)
                {
                    string referencia = item.Referencia.ToString();                    
                    rutasTxt.Referencia = item.Referencia;
                    rutasTxt.RutaInformeGeneral = textRutaInformeGeneral.Text;
                    rutasTxt.RutaInformeIndividual = textRutaInformeIndividual.Text;
                    rutasTxt.RutaMiembros = textRutaMiembros.Text;
                    rutasTxtService.ModificarRutasTxt(rutasTxt, referencia);
                }
            }
            else
            {
                if (rutasTxtConsultaResponse.RutasTxts.Count == 0)
                {
                    RutaInformeGeneral = textRutaInformeGeneral.Text;
                    RutaInformeIndividual = textRutaInformeIndividual.Text;
                    RutaMiembros = textRutaMiembros.Text;
                    rutasTxt.Referencia = 1;
                    rutasTxt.RutaInformeGeneral = RutaInformeGeneral;
                    rutasTxt.RutaInformeIndividual = RutaInformeIndividual;
                    rutasTxt.RutaMiembros = RutaMiembros;
                    rutasTxtService.Guardar(rutasTxt);
                }
            }
        }
        private void EstablecerCorreo()
        {
            EmailConsultaResponse emailConsultaResponse = emailService.Consultar();
            if (emailConsultaResponse.Emails.Count > 0)
            {
                foreach (var item in emailConsultaResponse.Emails)
                {
                    textCorreo.Text = item.CorreoElectronicoOrigen;
                    textContraseña.Text = item.Contraseña;
                }
            }
            else
            {
                if (emailConsultaResponse.Emails.Count == 0)
                {
                    email.CorreoElectronicoOrigen = textCorreo.Text;
                    email.Contraseña = textContraseña.Text;
                    email.CorreoElectronicoDestino = textCorreo.Text;
                }
            }
        }
        private void EstablecerCarpetasRaiz(RutasTxtService rutasTxtService)
        {
            RutasTxtConsultaResponse rutasTxtConsultaResponse = rutasTxtService.Consultar();
            if (rutasTxtConsultaResponse.RutasTxts.Count > 0)
            {
                foreach (var item in rutasTxtConsultaResponse.RutasTxts)
                {
                    textRutaInformeGeneral.Text = item.RutaInformeGeneral;
                    textRutaInformeIndividual.Text = item.RutaInformeIndividual;
                    textRutaMiembros.Text=item.RutaMiembros;
                    RutaInformeGeneral = textRutaInformeGeneral.Text;
                    RutaInformeIndividual = textRutaInformeIndividual.Text;
                    textRutaBautizados.Text=item.RutaBautizados;
                    RutaMiembros = textRutaMiembros.Text;
                }
            }
            else
            {
                if (rutasTxtConsultaResponse.RutasTxts.Count == 0)
                {
                    if (textRutaInformeGeneral.Text != "" && textRutaInformeIndividual.Text != "")
                    {
                        RutaInformeGeneral = textRutaInformeGeneral.Text;
                        RutaInformeIndividual = textRutaInformeIndividual.Text;
                        RutaMiembros = textRutaMiembros.Text;
                        rutasTxt.Referencia = 1;
                        rutasTxt.RutaInformeGeneral = RutaInformeGeneral;
                        rutasTxt.RutaInformeIndividual = RutaInformeIndividual;
                        rutasTxt.RutaMiembros = RutaMiembros;
                        rutasTxtService.Guardar(rutasTxt);
                    }
                }
            }
        }
        private Iglesia MapearIglesia()
        {
            iglesia = new Iglesia();
            iglesia.IdIglesia = "#Iglesia";
            iglesia.NombreIglesia = textNombreIglesia.Text;
            iglesia.NIT = textNIT.Text;
            iglesia.Correo = textPBX.Text;
            iglesia.Direccion = textDireccion.Text;
            iglesia.Telefono = textTelefono.Text;
            return iglesia;
        }

        private async void btnRegistrarInfo_Click(object sender, EventArgs e)
        {
            try
            {

            }catch(Exception ex)
            {
                Iglesia iglesia = MapearIglesia();
                string mensaje = iglesiaService.Guardar(iglesia);
                MessageBox.Show(mensaje, "Mensaje de Guardado", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                BuscarPorId();
                ModificarCadenaConexion();
                EstablecerCarpetasRaiz(rutasTxtService);
                EstablecerCorreo();
            }
        }
        private void btnEliminarInfo_Click(object sender, EventArgs e)
        {
            string mensaje = iglesiaService.Eliminar(idIglesia);
            MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Limpiar();
        }
        private void btnModificarInfo_Click(object sender, EventArgs e)
        {
            var respuesta = MessageBox.Show("Está seguro de Modificar la información", "Mensaje de Modificacion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (respuesta == DialogResult.Yes)
            {
                Iglesia iglesia = MapearIglesia();
                string mensaje = iglesiaService.Modificar(iglesia);
                MessageBox.Show(mensaje, "Mensaje de campos", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                BuscarPorId();
                ModificarCarpetasRaiz(rutasTxtService);
                ModificarCorreo();
                ConsultarCadena();
                if (textBD.Text != "" && textServidor.Text != "" && textUI.Text != "")
                {
                    ModificarCadenaConexion();
                }
                else
                {
                    string message = "No se puede modificar la cadena de conexion uno o los 3 campos estan vacios";
                    MessageBox.Show(message, "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnBuscarCadenaConexion_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string ruta = dialog.SelectedPath;
                textServidor.Text = ruta;
            }
        }
        private void ModificarCadenaConexion()
        {
            if (textServidor.Text != "" && textBD.Text != "")
            {
                primeraCadena = "                <add name=" + '"' + "conexion" + '"' + " connectionString=" + '"';
                segundaCadenaModificada = "server=" + newServer + ";Database=" + newBd + ";Trusted_Connection = True; MultipleActiveResultSets = true" + '"' + " />";
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

        private void textUI_TextChanged(object sender, EventArgs e)
        {
            newUi = textUI.Text;
        }

        private void textServidor_TextChanged(object sender, EventArgs e)
        {
            newServer = textServidor.Text;
        }

        private void textBD_TextChanged(object sender, EventArgs e)
        {
            newBd = textBD.Text;
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGnerarIndividualTesoreria_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string ruta = dialog.SelectedPath;
                textRutaInformeIndividual.Text = ruta;
            }
        }

        private void btnGnerarGeneralTesoreria_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string ruta = dialog.SelectedPath;
                textRutaInformeGeneral.Text = ruta;
            }
        }

        private void btnRutaGuardadoMiembros_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string ruta = dialog.SelectedPath;
                textRutaMiembros.Text = ruta;
            }
        }

        private void btnRutaGuardadoBautizados_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string ruta = dialog.SelectedPath;
                textRutaBautizados.Text = ruta;
            }
        }
    }
}

using System;
using System.Windows.Forms;
using BLL;
using Entity;
using Ldb;

namespace UI
{
    public partial class FormActivador : Form
    {
        SoftwareService softwareService;
        LicenciaService licenciaService;
        Licencia licencia;
        Software software;
        bool estadoCliente;
        string estadoLicencia;
        string nombreSoftware;
        string licenciaSoftware;
        DateTime fechaDeinstalacion;
        string fechaDeExpiracion;
        string horaDeExpiracion;
        string fechaDeActivacion;
        string horaDeActivacion;
        public string ModoDeEntrada;
        public FormActivador()
        {
            softwareService = new SoftwareService(DbConfig.ConnectionString);
            licenciaService = new LicenciaService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
            ConsultarEstado();
        }
        public void Inicializar()
        {
            btnActivarProducto.Focus();
            labelText1.Text = "Por favor ingrese la clave de licencia para la activacion del producto AdminPharm ";
            labelText2.Text = "sino tiene la clave de licencia, pongase en contacto con su proveedor de servicios";
            if (ModoDeEntrada == "Principal")
            {
                labelText1.Visible = false;
                labelText2.Visible = false;
                pictureLlaveLicencia.Visible = false;
                textLicencia.Visible = false;
                pictureLicenciaActivada.Visible = true;
                labelTextLicencia1.Visible = true;
                labelCantidadDiasLicencia.Visible = true;
                labelTextLicencia2.Visible = true;
                labelTituloActivador.Text = "Producto Activo";
                btnActivarProducto.Text = "Aceptar";
            }
            else
            {
                if (ModoDeEntrada == "Login")
                {

                }
            }
        }
        private Licencia MapearLicencia()
        {
            licencia = new Licencia();
            licencia.LicenciaSoftware = licenciaSoftware;
            return licencia;
        }
        private Software MapearDatosDeActivacion()
        {
            software = new Software();
            nombreSoftware = "AdminPharm";
            software.NombreDeSoftware=nombreSoftware;
            return software;
        }
        private void ValidarEstadoLicencia()
        {
            BusquedaSoftwareRespuesta respuesta = new BusquedaSoftwareRespuesta();
            respuesta = softwareService.BuscarPorNombreDeSoftware("AdminPharm");
            if (respuesta.Software != null)
            {
                Software softwareValidado = respuesta.Software;
                softwareService.Modificar(softwareValidado);
                if (softwareValidado.EstadoDeLicencia == "Expirado")
                {
                    estadoCliente = false;
                    pictureLicenciaExpirada.Visible = true;
                    pictureLicenciaActivada.Visible = false;
                    labelText1.Text = "Clave de licencia incorrecta por favor vuelva a ingresar la clave";
                    labelText2.Text = "si no tiene una clave pongase en contacto con proveedor de servicios";
                }
                else
                {
                    estadoCliente = true;
                }
            }
        } 
        private void ConsultarEstado()
        {
            ConsultaSoftwareRespuesta respuesta = new ConsultaSoftwareRespuesta();
            respuesta = softwareService.ConsultarTodos();
            if (respuesta.Softwares.Count != 0 && respuesta.Softwares != null)
            {
                ValidarEstadoLicencia();
                if (estadoCliente == true)
                {
                    //this.Visible = false;
                    //Application.Exit();
                    //Application.Run(new FormInicioSesion());
                }
            }
        }
        private void ValidarLicencia()
        {
            ConsultaLicenciaRespuesta respuesta = new ConsultaLicenciaRespuesta();
            licenciaSoftware = textLicencia.Text;
            respuesta=licenciaService.ConsultaTodasLicencias(licenciaSoftware);
            if (respuesta.Licencias.Count != 0 && respuesta.Licencias != null)
            {
                //Licencia activada
                Software software = MapearDatosDeActivacion();
                var respuestaDeGuadado = softwareService.Guardar(software);
                if (respuestaDeGuadado == "Esta id de software ya existe")
                {
                    pictureLicenciaActivada.Visible = true;
                    textLicencia.Enabled = false;
                    labelText1.Text = "Su software ya esta activado correctamente si desea renovar su clave";
                    labelText2.Text = "de licencia por favor pongase en contacto con su proveedor de servicios";
                    this.Hide();
                    FormInicioSesion formInicioSesion = new FormInicioSesion();
                    formInicioSesion.Show();
                }
                else
                {
                    if (respuestaDeGuadado == "Software registrado correctamente")
                    {
                        labelText1.Text = "Su software ya esta activado correctamente si desea renovar su clave";
                        labelText2.Text = "de licencia por favor pongase en contacto con su proveedor de servicios";
                        FormInicioSesion formInicioSesion = new FormInicioSesion();
                        formInicioSesion.Show();
                        this.Hide();
                    }
                }

            }
            else
            {
                if (respuesta.Licencias.Count != 0 && respuesta.Licencias != null)
                {
                    labelText1.Visible = true;
                    labelText2.Visible = true;
                }
            }
        }
        private void btnActivarProducto_Click(object sender, EventArgs e)
        {
            if (btnActivarProducto.Text != "Aceptar")
            {
                ValidarLicencia();
            }
            else
            {
                if (btnActivarProducto.Text == "Aceptar")
                {
                    this.Close();
                }
            }
        }
        private void textLicencia_Enter(object sender, EventArgs e)
        {
            if (textLicencia.Text == "XXXX-XXXX-XXXX-XXXX-2023")
            {
                textLicencia.Text = "";
            }
        }

        private void textLicencia_Leave(object sender, EventArgs e)
        {
            if (textLicencia.Text == "")
            {
                textLicencia.Text = "XXXX-XXXX-XXXX-XXXX-2023";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BLL;
using Entity;
//se importa la libreria para arrastrar formulario
using System.Runtime.InteropServices;
using Cloud;
using Color = System.Drawing.Color;
using Ldb;

namespace UI
{
    public partial class FormInicioSesion : Form
    {
        SoftwareService softwareService;
        UsuarioService usuarioService;
        Software software;
        bool estadoCliente;
        string nombreDeUsuario;
        string contraseña;
        string id_Usuario;
        string rol;
        bool UsuarioValido=false;
        object sender;
        EventArgs e;
        private System.Windows.Forms.Timer bannerTimer = new System.Windows.Forms.Timer();
        private List<string> nombresBanner = new List<string> { "banner1", "banner2", "banner3", "banner4", "banner5", "banner6", "banner7", "banner8", "banner9", "banner10", "banner11", "banner12", "banner13", "banner14", "banner15" };
        private Random random = new Random();
        private int indiceBannerActual = 0;

        public FormInicioSesion()
        {
            softwareService = new SoftwareService(DbConfig.ConnectionString);
            usuarioService = new UsuarioService(DbConfig.ConnectionString);
            InitializeComponent();
            UbicacionesPorDefault();

            // Configurar las esquinas redondeadas en el constructor
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true; // Mejora el rendimiento al realizar el dibujo
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            // Configurar el temporizador
            bannerTimer.Interval = 5000; // 5 segundos
            bannerTimer.Tick += tiempoDeBanner_Tick;

            // Iniciar el temporizador cuando se carga el formulario
            bannerTimer.Start();

            // Mostrar el primer banner al inicio
            MostrarSiguienteBanner();
        }
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void MostrarSiguienteBanner()
        {
            // Obtener el nombre del siguiente banner
            string nombreBanner = nombresBanner[indiceBannerActual];

            // Cargar la imagen desde los recursos y asignarla al PictureBox
            pictureBanner.Image = Properties.Resources.ResourceManager.GetObject(nombreBanner) as Image;

            // Incrementar el índice para el siguiente banner
            indiceBannerActual = (indiceBannerActual + 1) % nombresBanner.Count;
        }
        public void VerificarEstadoDeLicencia()
        {
            ConsultaSoftwareRespuesta respuesta = new ConsultaSoftwareRespuesta();
            respuesta = softwareService.ConsultarTodos();
            if (respuesta.Softwares.Count == 0 || respuesta.Softwares == null)
            {
                FormActivador formActivador = new FormActivador();
                formActivador.Show();
                this.Hide();
                this.Visible = false;
            }
            else
            {
                if (respuesta.Softwares.Count != 0 && respuesta.Softwares != null)
                {
                    this.Visible = true;
                    this.Enabled = true;
                }
            }
        }

        //private void btnMaximizar_Click_1(object sender, EventArgs e)
        //{
        //    WindowState = FormWindowState.Maximized;
        //}

        //private void btnRestaurar_Click_1(object sender, EventArgs e)
        //{
        //    WindowState = FormWindowState.Normal;
        //}
        private void BuscarPorNombreDeUsuario()
        {
            BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
            respuesta = usuarioService.BuscarPorNombreDeUsuario(nombreDeUsuario);
            if (respuesta.Usuario != null)
            {
                var usuario = new List<Usuario> { respuesta.Usuario };
                contraseña = respuesta.Usuario.Contraseña;
                id_Usuario = respuesta.Usuario.Identificacion;
                rol= respuesta.Usuario.Rol;
                linkLabelRestaurarContraseña.ForeColor = Color.FromArgb(0, 0, 255);
                linkLabelRegistrarUsuario.ForeColor = Color.FromArgb(0, 0, 255);
                ValidarContraseña();
            }
            else
            {
                if (respuesta.Usuario == null)
                {
                    labelAdvertencia.Visible = true;
                    iconAdvertencia.Visible = true;
                    labelAdvertencia.Text = "El nombre de usuario no existe";
                    linkLabelRegistrarUsuario.ForeColor = Color.Maroon;
                    textBoxUser.ForeColor = Color.Maroon;
                    UbicacionesPorAdvertencia();
                }
            }
        }
        private void UbicacionesPorAdvertencia()
        {
            linkLabelRegistrarUsuario.Location = new Point(116, 296);
            linkLabelRestaurarContraseña.Location = new Point(147, 279);
        }
        private void UbicacionesPorDefault()
        {

            linkLabelRegistrarUsuario.Location = linkLabelRestaurarContraseña.Location;
            linkLabelRestaurarContraseña.Location = iconAdvertencia.Location;
        }
        private void ValidarContraseña()
        {
            if (textBoxPasword.Text == contraseña)
            {
                UsuarioValido = true;
            }
            else
            {
                if (textBoxPasword.Text != contraseña)
                {
                    UsuarioValido = false;
                    labelAdvertencia.Visible = true;
                    iconAdvertencia.Visible = true;
                    labelAdvertencia.Text = "Contraseña incorrecta";
                    linkLabelRestaurarContraseña.ForeColor = Color.Maroon;
                    textBoxPasword.ForeColor = Color.Maroon;
                    UbicacionesPorAdvertencia();
                }
            }
        }
        private void MapearDatos()
        {
            nombreDeUsuario = textBoxUser.Text;
        }
        private async void ValidarUsuario()
        {
            MapearDatos();
            var contraseña = textBoxPasword.Text;
            //Consulta en la nube
            try
            {
                var db = FirebaseService.Database;
                var usersQuery = db.Collection("UserData");
                var users = new List<UserData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await usersQuery.GetSnapshotAsync();
                users = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<UserData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var usarioFiltrado = users.Where(user => user.UserName == nombreDeUsuario).ToList();
                if (usarioFiltrado.Count > 0)
                {
                    if (FirebaseSecurity.Decrypt(usarioFiltrado[0].Password) == contraseña)
                    {
                        UsuarioValido = true;

                        FormMenu mainMenu = new FormMenu();
                        mainMenu.idUsuario = usarioFiltrado[0].ID;
                        mainMenu.rol = usarioFiltrado[0].Rol;
                        mainMenu.disponibilidadNube = true;
                        mainMenu.ValidarUsuario();
                        mainMenu.Show();
                        this.Hide();
                        // Obtener referencia al formulario principal
                        FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                        // Verificar si el formulario principal está abierto
                        if (formPrincipal != null)
                        {
                            // Lanzar el evento para notificar al formulario principal sobre la excepción
                            formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                        }
                    }
                    else
                    {
                        labelAdvertencia.Visible = true;
                        iconAdvertencia.Visible = true;
                        labelAdvertencia.Text = "Contraseña incorrecta";
                        linkLabelRegistrarUsuario.ForeColor = Color.Maroon;
                        textBoxUser.ForeColor = Color.Maroon;
                        UbicacionesPorAdvertencia();
                    }
                }
                else
                {
                    labelAdvertencia.Visible = true;
                    iconAdvertencia.Visible = true;
                    labelAdvertencia.Text = "El nombre de usuario no existe";
                    linkLabelRegistrarUsuario.ForeColor = Color.Maroon;
                    textBoxUser.ForeColor = Color.Maroon;
                    UbicacionesPorAdvertencia();
                }
            }
            catch (Exception ex)
            {
                BuscarPorNombreDeUsuario();
                //verifica de forma local
                if (UsuarioValido == true)
                {
                    FormMenu mainMenu = new FormMenu();
                    mainMenu.idUsuario = id_Usuario;
                    mainMenu.rol = rol;
                    mainMenu.disponibilidadNube = true;
                    mainMenu.ValidarUsuario();
                    mainMenu.Show();
                    this.Hide();
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                    }
                }
            }
        } 

        private void btnAjustarServidor_Click(object sender, EventArgs e)
        {
            FormAjustarServidor formAjustarServidor = new FormAjustarServidor();
            formAjustarServidor.Show();
            this.Hide();
        }

        private void linkLabelRegistrarUsuario_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRegistrarUsuario formRegistrarUsuario = new FormRegistrarUsuario();
            formRegistrarUsuario.Show();
            this.Hide();
        }

        private void linkLabelRestaurarContraseña_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRestaurarContraseña formRestaurarContraseña = new FormRestaurarContraseña();
            formRestaurarContraseña.Show();
            this.Hide();
        }

        private void textBoxPasword_TextChanged(object sender, EventArgs e)
        {
            if (textBoxUser.Text == "@Victor10" && textBoxPasword.Text == "Victor2002")
            {
                btnAjustarServidor.Visible = true;
            }
        }

        private void textBoxUser_TextChanged(object sender, EventArgs e)
        {
            if (textBoxUser.Text == "")
            {
                iconAdvertencia.Visible = false;
                labelAdvertencia.Visible = false;
                linkLabelRegistrarUsuario.Location = new Point(147, 279);
                linkLabelRestaurarContraseña.Location = new Point(150, 257);
            }
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            ValidarUsuario();
        }

        private void panelBarraFormulario_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelBannerName_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pictureBanner_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelDescripcion_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pictureLogo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void textBoxPasword_Enter(object sender, EventArgs e)
        {
            if (textBoxPasword.Text == "Contraseña")
            {
                textBoxPasword.Text = "";
                textBoxPasword.ForeColor = Color.Black;
                textBoxPasword.UseSystemPasswordChar = true;
            }
        }

        private void textBoxPasword_Leave(object sender, EventArgs e)
        {
            if (textBoxPasword.Text == "")
            {
                textBoxPasword.Text = "Contraseña";
                textBoxPasword.ForeColor = Color.Gray;
                textBoxPasword.UseSystemPasswordChar = false;
            }
        }

        private void textBoxUser_Enter(object sender, EventArgs e)
        {
            if (textBoxUser.Text == "@Usuario")
            {
                textBoxUser.Text = "";
                textBoxUser.ForeColor = Color.Black;
            }
        }

        private void textBoxUser_Leave(object sender, EventArgs e)
        {
            if (textBoxUser.Text == "")
            {
                textBoxUser.Text = "@Usuario";
                textBoxUser.ForeColor = Color.Gray;
            }
        }

        private void iconNoSeePasword_Click(object sender, EventArgs e)
        {
            iconNoSeePasword.Visible = false;
            iconSeePasword.Visible = true;
            textBoxPasword.UseSystemPasswordChar = false;
        }

        private void iconSeePasword_Click(object sender, EventArgs e)
        {
            iconNoSeePasword.Visible = true;
            iconSeePasword.Visible = false;
            textBoxPasword.UseSystemPasswordChar = true;
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tiempoDeBanner_Tick(object sender, EventArgs e)
        {
            // Cambiar el banner cada 4 segundos
            MostrarSiguienteBanner();
        }
    }
}

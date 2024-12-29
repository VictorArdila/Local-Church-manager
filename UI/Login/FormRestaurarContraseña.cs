using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//se importa la libreria para arrastrar formulario
using System.Runtime.InteropServices;
using BLL;
using Entity;
using Ldb;

namespace UI
{
    public partial class FormRestaurarContraseña : Form
    {
        UsuarioService usuarioService;
        UserMaps userMaps;
        List<Usuario> usuarios;
        Usuario usuario;
        string nombreDeUsuario;
        string identificacion;
        string tipoId;
        string nombres;
        string apellidos;
        DateTime fechaDeNacimiento;
        string direccion;
        string sexo;
        string telefono;
        string rol;
        string correo;
        string nombreUsuario;
        string contraseña;
        string codigoUsuario;
        bool UsuarioValido;
        bool identificacionValida;
        public FormRestaurarContraseña()
        {
            usuarioService = new UsuarioService(DbConfig.ConnectionString);
            userMaps = new UserMaps();
            InitializeComponent();
        }
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormRestaurarContraseña_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            FormInicioSesion login = new FormInicioSesion();
            login.Show();
            this.Hide();
        }
        private void MapearDatos()
        {
            nombreDeUsuario = textUsuario.Text;
            identificacion = textIdentificacion.Text;
        }
        private Usuario MapearDatosDeUsuario()
        {
            usuario = new Usuario();
            usuario.Identificacion = identificacion;
            usuario.TipoDeIdentificacion = tipoId;
            usuario.Nombres= nombres;
            usuario.Apellidos= apellidos;
            usuario.FechaDeNacimiento = fechaDeNacimiento;
            usuario.Direccion= direccion;
            usuario.Sexo= sexo;
            usuario.Telefono= telefono;
            usuario.Rol= rol;
            usuario.CorreoElectronico= correo;
            usuario.NombreUsuario = nombreUsuario;
            usuario.Contraseña= textContraseña.Text;
            usuario.CodigoUsuario = codigoUsuario;
            return usuario;
        }
        private void BuscarPorNombreDeUsuario()
        {
            BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
            respuesta = usuarioService.BuscarPorNombreDeUsuario(nombreDeUsuario);
            if (respuesta.Usuario != null)
            {
                var usuario = new List<Usuario> { respuesta.Usuario };
                
                UsuarioValido = true;
                labelAdvertencia.Visible = false;
                BuscarPorIdentificacion();
            }
            else
            {
                if (respuesta.Usuario == null)
                {
                    labelAdvertencia.Text = "El usuario no existe";
                    labelAdvertencia.Visible = true;
                }
            }
        }
        private void BuscarPorIdentificacion()
        {
            BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
            respuesta = usuarioService.BuscarPorIdentificacion(identificacion);
            if (respuesta.Usuario != null)
            {
                var usuarios = new List<Usuario> { respuesta.Usuario };
                identificacionValida = true;
                labelAdvertencia.Visible = false;
                identificacion = respuesta.Usuario.Identificacion;
                tipoId= respuesta.Usuario.TipoDeIdentificacion;
                nombres= respuesta.Usuario.Nombres;
                apellidos= respuesta.Usuario.Apellidos;
                fechaDeNacimiento= respuesta.Usuario.FechaDeNacimiento;
                direccion= respuesta.Usuario.Direccion;
                sexo= respuesta.Usuario.Sexo;
                telefono= respuesta.Usuario.Telefono;
                rol= respuesta.Usuario.Rol;
                correo= respuesta.Usuario.CorreoElectronico;
                nombreUsuario = respuesta.Usuario.NombreUsuario;
                contraseña= respuesta.Usuario.Contraseña;
                codigoUsuario= respuesta.Usuario.CodigoUsuario;
            }
            else
            {
                if (respuesta.Usuario == null)
                {
                    labelAdvertencia.Text = "Esta id no es de este usuario";
                    labelAdvertencia.Visible = true;
                }
            }
        }
        private void Limpiar()
        {
            textUsuario.Text = "";
            textContraseña.Text = "";
            textIdentificacion.Text = "";
        }
        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            Usuario usuario = MapearDatosDeUsuario();
            try
            {
                var msg = usuarioService.Modificar(usuario);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var user = userMaps.UserMap(usuario);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("UserData").Document(user.ID);
                docRef.SetAsync(user);
                MessageBox.Show(msg, "Modificacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                int count = ex.Message.Length;
                if (count > 0)
                {
                    var msg = usuarioService.Modificar(usuario);
                    MessageBox.Show(msg, "Modificacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Limpiar();
                }
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            MapearDatos();
            if (textUsuario.Text == "")
            {
                labelAdvertencia.Text = "No ha llenado el campo usuario";
                labelAdvertencia.Visible = true;
            }
            else
            {
                if (textIdentificacion.Text == "")
                {
                    labelAdvertencia.Text = "No ha llenado el campo identificacion";
                    labelAdvertencia.Visible = true;
                }
                else
                {
                    BuscarPorNombreDeUsuario();
                    if (UsuarioValido == true && identificacionValida == true)
                    {
                        labelContraseña.Enabled = true;
                        textContraseña.Enabled = true;
                        btnBuscar.Visible = false;
                        btnRestaurar.Visible = true;
                    }
                }
            }
        }

        private void textContraseña_Enter(object sender, EventArgs e)
        {
            if (textContraseña.Text == "Contraseña")
            {
                textContraseña.Text = "";
                textContraseña.ForeColor = Color.Black;
                textContraseña.UseSystemPasswordChar = true;
            }
        }

        private void textContraseña_Leave(object sender, EventArgs e)
        {
            if (textContraseña.Text == "")
            {
                textContraseña.Text = "Contraseña";
                textContraseña.ForeColor = Color.Gray;
                textContraseña.UseSystemPasswordChar = false;
            }
        }

        private void iconSeePasword_Click(object sender, EventArgs e)
        {
            iconNoSeePasword.Visible = true;
            iconSeePasword.Visible = false;
            textContraseña.UseSystemPasswordChar = true;
        }

        private void iconNoSeePasword_Click(object sender, EventArgs e)
        {
            iconNoSeePasword.Visible = false;
            iconSeePasword.Visible = true;
            textContraseña.UseSystemPasswordChar = false;
        }
    }
}

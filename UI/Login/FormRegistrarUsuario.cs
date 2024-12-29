using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//se importa la libreria para arrastrar formulario
using System.Runtime.InteropServices;
using BLL;
using Entity;
using Ldb;

namespace UI
{
    public partial class FormRegistrarUsuario : Form
    {
        UsuarioService usuarioService;
        UserMaps userMaps;
        List<Usuario> usuarios;
        Usuario usuario;
        string rol = "Administrador";
        string programador = "Programador";
        bool rolExistenteValidado;
        bool programadorExistenteValido;
        public FormRegistrarUsuario()
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

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void FormRegistrarUsuario_MouseDown(object sender, MouseEventArgs e)
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
        private void BuscarPorProgramador()
        {
            BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
            respuesta = usuarioService.BuscarPorNombreDeUsuario(programador);
            if (respuesta.Usuario != null)
            {
                var usuario = new List<Usuario> { respuesta.Usuario };
                rolExistenteValidado = true;
                programadorExistenteValido = true;
                labelAdvertencia.Text = "No puede registrarse como programador";
                labelAdvertencia.Visible = true;
            }
            else
            {
                if (respuesta.Usuario == null)
                {
                    rolExistenteValidado = false;
                    labelAdvertencia.Visible = false;
                }
            }
        }
        private void BuscarPorRol()
        {
            BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
            respuesta = usuarioService.BuscarPorNombreDeUsuario(rol);
            if (respuesta.Usuario != null)
            {
                var usuario = new List<Usuario> { respuesta.Usuario };
                rolExistenteValidado = true;
                programadorExistenteValido = true;
                labelAdvertencia.Text = "El rol de administrador ya esta registrado";
                labelAdvertencia.Visible = true;
            }
            else
            {
                if (respuesta.Usuario == null)
                {
                    rolExistenteValidado = false;
                    labelAdvertencia.Visible = false;
                }
            }
        }
        private Usuario MapearUsuario()
        {
            usuario = new Usuario();
            usuario.Nombres = textNombre.Text;
            usuario.Apellidos = textApellido.Text;
            usuario.Identificacion = textIdentificacion.Text;
            usuario.TipoDeIdentificacion = comboTipoDeId.Text;
            usuario.FechaDeNacimiento = DateTime.Parse(dateTimeFechaDeNacimiento.Text);
            usuario.Sexo = comboSexo.Text;
            usuario.Direccion = textDireccion.Text;
            usuario.Telefono = textTelefono.Text;
            usuario.Rol = comboRol.Text;
            usuario.CorreoElectronico = textCorreo.Text;
            usuario.NombreUsuario = textUsuario.Text;
            usuario.Contraseña = textContraseña.Text;
            return usuario;
        }
        private void Limpiar()
        {
            textCorreo.Text = "";
            textUsuario.Text = "";
            textContraseña.Text = "";
            textIdentificacion.Text = "";
            comboTipoDeId.Text = "CC";
            comboRol.Text = "Administrador";
            textNombre.Text = "";
            textApellido.Text = "";
            dateTimeFechaDeNacimiento.Value = DateTime.Now;
            comboSexo.Text = "M";
            textDireccion.Text = "";
            textTelefono.Text = "";
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            BuscarPorProgramador();
            BuscarPorRol();
            if (comboRol.Text == "Secretario(a)" || comboRol.Text == "Tesorero(a)" || comboRol.Text == "Pastor" || comboRol.Text == "Programador")
            {
                if (programadorExistenteValido != true)
                {
                    if (rolExistenteValidado != true)
                    {
                        //Obtenemos los datos del usuario y construimos el dato de la nube
                        Usuario usuario = MapearUsuario();
                        try
                        {
                            var msg = usuarioService.Guardar(usuario);
                            //Guardamos en la nube
                            var db = FirebaseService.Database;
                            var user = userMaps.UserMap(usuario);
                            Google.Cloud.Firestore.DocumentReference docRef = db.Collection("UserData").Document(user.ID);
                            docRef.SetAsync(user);
                            // Guardamos localmente
                            MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            if (count > 0) {
                                // Guardamos localmente
                                var msg = usuarioService.Guardar(usuario);
                                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Limpiar();
                            }
                        }
                    }
                }
            }
            else
            {
                string msg = "Formato del combo de roles incorrecto";
                MessageBox.Show(msg, "Combo Roles", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textContraseña_Enter(object sender, EventArgs e)
        {
            if (textContraseña.Text == "Mayor a 6 caracteres")
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
                textContraseña.Text = "Mayor a 6 caracteres";
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

        private void textUsuario_Enter(object sender, EventArgs e)
        {
            if (textUsuario.Text == "@Bryan10")
            {
                textUsuario.Text = "";
                textUsuario.ForeColor = Color.Black;
            }
        }

        private void textUsuario_Leave(object sender, EventArgs e)
        {
            if (textUsuario.Text == "")
            {
                textUsuario.Text = "@Bryan10";
                textUsuario.ForeColor = Color.Gray;
            }
        }

        private void textCorreo_Enter(object sender, EventArgs e)
        {
            if (textCorreo.Text == "@gmail.com")
            {
                textCorreo.Text = "";
                textCorreo.ForeColor = Color.Black;
            }
        }

        private void textCorreo_Leave(object sender, EventArgs e)
        {
            if (textCorreo.Text == "")
            {
                textCorreo.Text = "@gmail.com";
                textCorreo.ForeColor = Color.Gray;
            }
        }
    }
}

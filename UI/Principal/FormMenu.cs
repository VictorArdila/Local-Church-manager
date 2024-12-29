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
using BLL;
using Entity;
using System.Runtime.InteropServices;
//se importa la libreria para arrastrar formulario
using System.Runtime.InteropServices;
using System.Threading;
using Cloud;
using Ldb;

namespace UI
{
    public partial class FormMenu : Form
    {
        UsuarioService empleadoService;
        IdUsuarioTxtService idUsuarioTxtService = new IdUsuarioTxtService();
        int cantidadFormularios = 4;
        string rutaInformeIndividualTesoreria;
        string rutaInformeGeneralTesoreria;
        string rutaReporteMiembros;
        string rutaReporteBautizados;
        public string idUsuario;
        public string rol;
        public bool disponibilidadNube;
        private readonly EventArgs e;
        private readonly object sender;
        public FormMenu()
        {
            empleadoService = new UsuarioService(DbConfig.ConnectionString);
            InitializeComponent();
            ValidarUsuario();
            //ValidarDisponibilidadNube();
            customizeDesign();
            EliminarIdSesionDeUsuario();
            this.Text = string.Empty;
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
        }
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        


        // Delegado para manejar eventos de cambios de conexión
        public delegate void SuccesfulOperacionHandler(object sender, SuccesfullEventArgs e);
        // Evento que se activa cuando todo esta bien para guardar en la nube
        public event SuccesfulOperacionHandler OperacionesExitosas;

        // Delegado para manejar eventos de excepción
        public delegate void ExcepcionEventHandler(object sender, ExcepcionEventArgs e);
        // Evento que se activa cuando ocurre una excepción
        public event ExcepcionEventHandler ExcepcionOcurrida;
        // Método para invocar el evento desde el formulario principal

        public virtual void OnExcepcionOcurrida(ExcepcionEventArgs e)
        {
            ExcepcionOcurrida?.Invoke(this, e);
            ValidarExcepcionesNube(e);
        }
        public virtual void OnSuccesfulOperations(SuccesfullEventArgs e)
        {
            OperacionesExitosas?.Invoke(this, e);
            ValidarDisponibilidadNube(e);
        }
        public void ValidarExcepcionesNube(ExcepcionEventArgs e)
        {
            if (e.Mensaje.Contains("Unavailable"))
            {
                labelEstadoNube.Text = "Sin conexión";
                labelEstadoNube.ForeColor = Color.Red;
                iconNube.IconColor = Color.Red;
            }
            else
            {
                if (e.Mensaje.Contains("ResourceExhausted"))
                {
                    labelEstadoNube.Text = "Nube saturada";
                    labelEstadoNube.ForeColor = Color.Red;
                    iconNube.IconColor = Color.Red;
                }
                else
                {
                    if (e.Mensaje.Contains("Unauthenticated"))
                    {
                        labelEstadoNube.Text = "Error temporal en nube";
                        labelEstadoNube.ForeColor = Color.Red;
                        iconNube.IconColor = Color.Red;
                    }
                }
            }
        }
        public void ValidarDisponibilidadNube(SuccesfullEventArgs e)
        {
            if (e.Mensaje.Contains("Succesfull"))
            {
                labelEstadoNube.Text = "Nube habilitada";
                labelEstadoNube.ForeColor = Color.Green;
                iconNube.IconColor = Color.Green;
            }
        }



        public async void ValidarUsuario()
        {
            if (idUsuario != null)
            {
                try
                {
                    var db = FirebaseService.Database;
                    var usersQuery = db.Collection("UserData");
                    var users = new List<UserData>();
                    // Realizar la suma directamente en la consulta Firestore
                    var snapshot = await usersQuery.GetSnapshotAsync();
                    users = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<UserData>()).ToList();
                    // Filtrar elementos según el campo Valor y la variable id
                    var usersPorId = users.Where(egreso => egreso.ID == idUsuario).ToList();
                    if (usersPorId[0] != null)
                    {
                        var rolConsultado = usersPorId[0].Rol;
                        if (rol == "Programador" || rolConsultado == "Programador")
                        {
                            btnGestionSecretaria.Enabled = true;
                            btnGestionSecretaria.Visible = true;
                            btnGestionTesoreria.Enabled = true;
                            btnGestionTesoreria.Visible = true;
                            btnGestionBD.Visible = true;
                            btnAjustes.Enabled = true;
                        }
                        else
                        {
                            if (rol == "Secretario(a)" || rolConsultado == "Secretario(a)")
                            {
                                btnGestionSecretaria.Visible = true;
                                btnGestionSecretaria.Enabled = true;
                                btnGestionTesoreria.Enabled = false;
                                btnGestionTesoreria.Visible = false;
                                btnGestionBD.Visible = false;
                                btnAjustes.Enabled = false;
                            }
                            else
                            {
                                if (rol == "Tesorero(a)" || rolConsultado == "Tesorero(a)")
                                {
                                    btnGestionTesoreria.Visible = true;
                                    btnGestionTesoreria.Enabled = true;
                                    btnGestionSecretaria.Enabled = false;
                                    btnGestionSecretaria.Visible = false;
                                    btnGestionBD.Visible = false;
                                    btnAjustes.Enabled = false;
                                }
                            }
                        }
                    }
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
                    BusquedaUsuarioRespuesta respuesta = new BusquedaUsuarioRespuesta();
                    respuesta = empleadoService.BuscarPorIdentificacion(idUsuario);
                    if (respuesta.Usuario != null)
                    {
                        var rolConsultado = respuesta.Usuario.Rol;
                        if (rol == "Programador" || rolConsultado == "Programador")
                        {
                            btnGestionSecretaria.Enabled = true;
                            btnGestionSecretaria.Visible = true;
                            btnGestionTesoreria.Enabled = true;
                            btnGestionTesoreria.Visible = true;
                            btnGestionBD.Visible = true;
                            btnAjustes.Enabled = true;
                        }
                        else
                        {
                            if (rol == "Secretario(a)" || rolConsultado == "Secretario(a)")
                            {
                                btnGestionSecretaria.Visible = true;
                                btnGestionSecretaria.Enabled = true;
                                btnGestionTesoreria.Enabled = false;
                                btnGestionTesoreria.Visible = false;
                                btnGestionBD.Visible = false;
                                btnAjustes.Enabled = false;
                            }
                            else
                            {
                                if (rol == "Tesorero(a)" || rolConsultado == "Tesorero(a)")
                                {
                                    btnGestionTesoreria.Visible = true;
                                    btnGestionTesoreria.Enabled = true;
                                    btnGestionSecretaria.Enabled = false;
                                    btnGestionSecretaria.Visible = false;
                                    btnGestionBD.Visible = false;
                                    btnAjustes.Enabled = false;
                                }
                            }
                        }
                    }
                }
                GuardarIdUsuarioSesion(idUsuario, rol);
            }
        }

        public void GuardarIdUsuarioSesion(string idUsuario, string rol)
        {
            IdUsuarioTxt idUsuarioTxt = new IdUsuarioTxt(idUsuario, rol);
            string mensaje = idUsuarioTxtService.Guardar(idUsuarioTxt);
            if (idUsuarioTxt.Identificacion == "1003377848")
            {
                labelTiempoLicencia.Visible = true;
                btnVerLicencia.Visible = true;
                textTiempoLicencia.Visible = true;
                labelTheme.Visible = true;
                btnModeLight.Visible = true;
                iconThemeSun.Visible = true;
            }
        }
        private void btnGestionSecretaria_Click(object sender, EventArgs e)
        {
            labelHeaderRuta.Text = "Inicio > Gestion Secretaria";
            showSubMenu(subMenuSecretaria);
            panelSelectionSecretaria.Location = btnGestionSecretaria.Location;
            panelSelectionSecretaria.Visible = true;
            panelSelectionBD.Visible = false;
            panelSelectionTesoreria.Visible = false;
            panelSelectionAjustes.Visible = false;
            panelSelectionSalir.Visible = false;
        }

        private void btnGestionTesoreria_Click(object sender, EventArgs e)
        {
            labelHeaderRuta.Text = "Inicio > Gestion Tesoreria";
            showSubMenu(subMenuTesoreria);
            panelSelectionTesoreria.Location = btnGestionTesoreria.Location;
            panelSelectionTesoreria.Visible = true;
            panelSelectionBD.Visible = false;
            panelSelectionSecretaria.Visible = false;
            panelSelectionAjustes.Visible = false;
            panelSelectionSalir.Visible = false;
        }
        private void EliminarIdSesionDeUsuario()
        {
            string mensaje = idUsuarioTxtService.EliminarHistorial();
        }

        private void btnWindowCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnWindowMaximize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            btnWindowMaximize.Visible = false;
            btnWindowRestore.Visible = true;
        }

        private void btnWindowMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnWindowRestore_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            btnWindowRestore.Visible = false;
            btnWindowMaximize.Visible = true;
        }
        private void customizeDesign()
        {
            subMenuSecretaria.Visible = false;
            subMenuTesoreria.Visible = false;
            btnCloseSidebar_Click(sender, e);
            AbrirSumadorDelSistema();
            //AbrirFormulario<FormGestionarCaja>();
        }
        private void hideSubMenu()
        {
            if (subMenuSecretaria.Visible == true)
                subMenuSecretaria.Visible = false;
            if (subMenuTesoreria.Visible == true)
                subMenuTesoreria.Visible = false;
        }
        private void showSubMenu(Panel subMenu)
        {
            if (panelSidebar.Width == 186)
            {
                if (subMenu.Visible == false)
                {
                    hideSubMenu();
                    subMenu.Visible = true;
                }
                else
                {
                    subMenu.Visible = false;
                }
            }
        }
        private void btnCloseSidebar_Click(object sender, EventArgs e)
        {
            panelSidebar.Width = 46;
            hideSubMenu();
            btnOpenSidebar.Visible = true;
            btnCloseSidebar.Visible = false;
            labelLogoName.Visible = false;
            btnGestionSecretaria.Text = "";
            btnGestionTesoreria.Text = "";
            btnGestionBD.Text = "";
            btnAjustes.Text = "";
            btnCerrarSesion.Text = "";
            panelSidebarClose.Visible = true;
        }

        private void btnOpenSidebar_Click(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }
        private void AbrirSumadorDelSistema()
        {
            labelHeaderRuta.Text = "Inicio";
            AbrirFormulario<InicioResumen>();
        }
        private void CerrarFormulariosCiclo()
        {
            foreach (Form form in panelContenedorInterno.Controls.OfType<Form>().ToList())
            {
                if (!(form is InicioResumen))
                {
                    form.Close();
                }
            }
        }
        //funcion Abrir Formularios
        private void AbrirFormulario<MiForm>() where MiForm : Form, new()
        {
            CerrarFormulariosCiclo();

            Form formulario = panelContenedorInterno.Controls.OfType<MiForm>().FirstOrDefault();

            if (formulario == null)
            {
                formulario = new MiForm();
                formulario.TopLevel = false;
                formulario.FormBorderStyle = FormBorderStyle.None;
                formulario.Dock = DockStyle.Fill;
                panelContenedorInterno.Controls.Add(formulario);
                panelContenedorInterno.Tag = formulario;
                formulario.Show();
                formulario.BringToFront();
            }
            else
            {
                formulario.BringToFront();
            }
        }
        //Funcion cerrar Formularios
        private void CerrarFormulario<MiForm>() where MiForm : Form, new()
        {
            Form formulario;
            formulario = panelContenedorInterno.Controls.OfType<MiForm>().FirstOrDefault();
            if (formulario != null)
            {
                formulario.Close();
            }
        }
        private void CerrarFormularioSwicth(int FormularioCerrar)
        {
            switch (FormularioCerrar)
            {
                case 1:
                    CerrarFormulario<FormDirectorio>();
                    break;
                case 2:
                    CerrarFormulario<FormMiembros>();
                    break;
                case 3:
                    CerrarFormulario<FormReuniones>();
                    break;
                case 4:
                    CerrarFormulario<FormDirectivas>();
                    break;
                case 5:
                    CerrarFormulario<FormIngresos>();
                    break;
                case 6:
                    CerrarFormulario<FormEgresos>();
                    break;
                case 7:
                    CerrarFormulario<FormLiquidacion>();
                    break;
                case 8:
                    CerrarFormulario<FormEnviables>();
                    break;
                case 9:
                    CerrarFormulario<FormBaseDeDatos>();
                    break;
                case 10:
                    CerrarFormulario<FormAjustes>();
                    break;
            }
        }
        private void btnDirectorio_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormDirectorio>();
        }

        private void btnMiembros_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormMiembros>();
        }

        private void btnReuniones_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormReuniones>();
        }

        private void btnDirectivas_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormDirectivas>();
        }

        private void btnApuntes_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormApuntes>();
        }
        private void btnAjustes_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormAjustes>();
            labelHeaderRuta.Text = "Inicio > Ajustes";
            panelSelectionAjustes.Location = btnAjustes.Location;
            panelSelectionAjustes.Visible = true;
            panelSelectionTesoreria.Visible = false;
            panelSelectionSecretaria.Visible = false;
            panelSelectionBD.Visible = false;
            panelSelectionSalir.Visible = false;
        }
        private void panelHeaderbar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelLogoContainer_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelLogoContainer_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnGestionSecretaria_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }


        private void btnAjustes_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            panelSidebarClose.Visible = false;
        }

        private void panelSidebar_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnDirectorio_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnMiembros_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnDirectivas_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnReuniones_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnApuntes_MouseEnter(object sender, EventArgs e)
        {
            panelSidebar.Width = 186;
            btnOpenSidebar.Visible = false;
            btnCloseSidebar.Visible = true;
            labelLogoName.Visible = true;
            btnGestionSecretaria.Text = "Gestion de secretaría";
            btnGestionTesoreria.Text = "Gestion de tesoreria";
            btnGestionBD.Text = "Gestion de BD";
            btnAjustes.Text = "Ajustes";
            btnCerrarSesion.Text = "Cerrar sesion";
            panelSidebarClose.Visible = false;
        }

        private void btnPrsupuestos_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormPresupuesto>();
        }

        private void btnIngresos_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormIngresos>();
        }

        private void btnEgresos_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormEgresos>();
        }

        private void btnLiquidaciones_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormLiquidacion>();
        }

        private void btnEnviables_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormEnviables>();
        }

        private void btnGestionBD_Click(object sender, EventArgs e)
        {
            CerrarFormulariosCiclo();
            AbrirFormulario<FormBaseDeDatos>();
            labelHeaderRuta.Text = "Inicio > Gestion Base de datos";
            panelSelectionBD.Location = btnGestionBD.Location;
            panelSelectionBD.Visible = true;
            panelSelectionTesoreria.Visible = false;
            panelSelectionSecretaria.Visible = false;
            panelSelectionAjustes.Visible = false;
            panelSelectionSalir.Visible = false;
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            panelSelectionSalir.Location = btnCerrarSesion.Location;
            panelSelectionSalir.Visible = true;
            panelSelectionBD.Visible = false;
            panelSelectionSecretaria.Visible = false;
            panelSelectionAjustes.Visible = false;
            panelSelectionTesoreria.Visible = false;
            FormInicioSesion login = new FormInicioSesion();
            login.Show();
            this.Hide();
        }
    }
}
// Clase para almacenar información sobre la excepción
public class ExcepcionEventArgs : EventArgs
{
    public string Mensaje { get; private set; }

    public ExcepcionEventArgs(string mensaje)
    {
        Mensaje = mensaje;
    }
}

public class SuccesfullEventArgs : EventArgs
{
    public string Mensaje { get; private set; }

    public SuccesfullEventArgs(string mensaje)
    {
        Mensaje = mensaje;
    }
}
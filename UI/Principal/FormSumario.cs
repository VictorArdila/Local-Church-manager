using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using BLL;
using Cloud;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using Entity;
using Google.Cloud.Firestore;
using Ldb;

namespace UI
{
    public partial class InicioResumen : Form
    {
        ContactoService contactoService;
        List<Contacto> contactos;
        Contacto contacto;
        MiembroService miembroService;
        List<Miembro> miembros;
        Miembro miembro;
        DirectivaService directivaService;
        List<Directiva> directivas;
        Directiva directiva;
        ReunionService reunionService;
        List<Reunion> reuniones;
        Reunion reunion;
        ApunteService apunteService;
        List<Apunte> apuntes;
        Apunte apunte;
        public InicioResumen()
        {
            contactoService = new ContactoService(DbConfig.ConnectionString);
            miembroService = new MiembroService(DbConfig.ConnectionString);
            directivaService = new DirectivaService(DbConfig.ConnectionString);
            reunionService = new ReunionService(DbConfig.ConnectionString);
            apunteService = new ApunteService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
        }

        private void InicioResumen_Load(object sender, EventArgs e)
        {
            Tiempo.Interval = 1000; // Intervalo de 1 segundo
            Tiempo.Start();
        }

        private void Tiempo_Tick(object sender, EventArgs e)
        {
            lblhora.Text = DateTime.Now.ToString("hh:mm:ss ");
            lblFecha.Text = DateTime.Now.ToLongDateString();
        }
        void Inicializar()
        {
            ConsultarContactos();
            ConsultarMiembros();
            ConsultarDirectivas();
            ConsultarReuniones();
            ConsultarApuntes();
        }
        private void btnSyncUp_Click(object sender, EventArgs e)
        {
            ConsultarContactos();
            ConsultarMiembros();
        }

        private void SincronizarConNube()
        {

        }
        private async void ConsultarApuntes()
        {
            try
            {
                var db = FirebaseService.Database;
                var notesQuery = db.Collection("NotesData");
                var apuntes = new List<NotesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await notesQuery.GetSnapshotAsync();
                apuntes = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<NotesData>()).ToList();
                labelApuntes.Text = apuntes.Count.ToString();
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
                ConsultaApunteRespuesta respuesta = new ConsultaApunteRespuesta();
                respuesta = apunteService.ConsultarTodos();
                apuntes = respuesta.Apuntes.ToList();
                if (respuesta.Apuntes.Count != 0 && respuesta.Apuntes != null)
                {
                    labelApuntes.Text = contactoService.Totalizar().Cuenta.ToString();
                }
                else
                {
                    labelApuntes.Text = "0";
                }
            }
        }
        private async void ConsultarReuniones()
        {
            try
            {
                var db = FirebaseService.Database;
                var meetQuery = db.Collection("MeetingsData");
                var meets = new List<MeetingsData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await meetQuery.GetSnapshotAsync();
                meets = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MeetingsData>()).ToList();
                labelReuniones.Text = meets.Count.ToString();
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
                ConsultaReunionRespuesta respuesta = new ConsultaReunionRespuesta();
                respuesta = reunionService.ConsultarTodos();
                reuniones = respuesta.Reuniones.ToList();
                if (respuesta.Reuniones.Count != 0 && respuesta.Reuniones != null)
                {
                    labelReuniones.Text = contactoService.Totalizar().Cuenta.ToString();
                }
                else
                {
                    labelReuniones.Text = "0";
                }
            }
        }
        private async void ConsultarDirectivas()
        {
            try
            {
                var db = FirebaseService.Database;
                var directivesQuery = db.Collection("DirectivesData");
                var directives = new List<DirectivesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await directivesQuery.GetSnapshotAsync();
                directives = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<DirectivesData>()).ToList();
                labelDirectivas.Text = directives.Count.ToString();
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
                ConsultaDirectivaRespuesta respuesta = new ConsultaDirectivaRespuesta();
                respuesta = directivaService.ConsultarTodos();
                directivas = respuesta.Directivas.ToList();
                if (respuesta.Directivas.Count != 0 && respuesta.Directivas != null)
                {
                    labelDirectivas.Text = contactoService.Totalizar().Cuenta.ToString();
                }
                else
                {
                    labelDirectivas.Text = "0";
                }
            }
        }
        private async void ConsultarContactos()
        {
            try
            {
                var db = FirebaseService.Database;
                var contactQuery = db.Collection("ContactsData");
                var contacts = new List<ContactData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await contactQuery.GetSnapshotAsync();
                contacts = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ContactData>()).ToList();
                labelDirectorio.Text = contacts.Count.ToString();
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
                ConsultaContactoRespuesta respuesta = new ConsultaContactoRespuesta();
                respuesta = contactoService.ConsultarTodos();
                contactos = respuesta.Contactos.ToList();
                if (respuesta.Contactos.Count != 0 && respuesta.Contactos != null)
                {
                    labelDirectorio.Text = contactoService.Totalizar().Cuenta.ToString();
                }
                else
                {
                    labelDirectorio.Text = "0";
                }
            }
        }
        private async void ConsultarMiembros()
        {
            try
            {
                var db = FirebaseService.Database;
                var members = new List<MemberData>();
                Google.Cloud.Firestore.Query membersQuery = db.Collection("MembersData").WhereEqualTo("Bautizado", "Si");
                QuerySnapshot snap = await membersQuery.GetSnapshotAsync();
                members = snap.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                labelMiembros.Text = members.Count.ToString();
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
                ConsultaMiembroRespuesta respuesta = new ConsultaMiembroRespuesta();
                respuesta = miembroService.ConsultarTodos();
                miembros = respuesta.Miembros.ToList();
                if (respuesta.Miembros.Count != 0 && respuesta.Miembros != null)
                {
                    labelMiembros.Text = miembroService.Totalizar().Cuenta.ToString();
                }
                else
                {
                    labelMiembros.Text = "0";
                }
            }
        }

        private void linkDeveloper_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://portafolio-web-profesional.web.app";

            // Abre la URL en un navegador web predeterminado
            System.Diagnostics.Process.Start(url);
        }

        private void linkDeveloper_Click(object sender, EventArgs e)
        {
            string url = "https://portafolio-web-profesional.web.app";

            // Abre la URL en un navegador web predeterminado
            System.Diagnostics.Process.Start(url);
        }
    }
}

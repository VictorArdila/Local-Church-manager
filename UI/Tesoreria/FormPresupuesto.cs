using BLL;
using Cloud;
using Cloud.FirebaseData;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Ldb;

namespace UI
{
    public partial class FormPresupuesto : Form
    {
        Validaciones validaciones;
        PresupuestoService presupuestoService;
        TabPage tabPage;
        PresupuestoComite presupuestoComite;
        PresupuestoIngresoLocal presupuestoIngresoLocal;
        PresupuestoEgresoLocal presupuestoEgresoLocal;
        List<PresupuestoComite> presupuestosComites;
        BudgetMaps budgetMaps;
        BudgetLocalMaps budgetLocalMaps;
        List<BudgetData> presupuestosComite;
        List<BudgetData> presupuestosGeneral;
        List<BudgetIngressLocalData> budgetIngressLocal;
        List<BudgetEgressLocalData> budgetEgressLocal;
        List<IngressData> ingressData;
        List<EgressData> egressData;
        int id = 0;
        int sumPresupuestos = 0;
        int ultimoId;
        int ofrendasIngresos;
        int votosIngresos;
        int activIdadesIngresos;
        int otrosIngresos;
        int sumEgresos;
        int sumIgresos;
        int sumIngresosLocales;
        int sumEgresosLocales;
        string comite = "";
        bool detallo = false;
        bool encontrado = false;
        bool nuevoConcepto = false;
        public FormPresupuesto()
        {
            validaciones = new Validaciones();
            presupuestoService = new PresupuestoService(DbConfig.ConnectionString);
            budgetMaps = new BudgetMaps();
            budgetLocalMaps = new BudgetLocalMaps();
            InitializeComponent();
            ObtenerUltimoRegistro();
            ConsultarIngresosFondosLocales();
            ConsultarEgresosFondosLocales();
            ConsultarPresupuestos();
            ObtenerIngresosYEgresos();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private string LecturaCifra(int totalDeIngresos)
        {
            // Convierte el total de ingresos a una cadena con separadores de miles
            string cifraFormateada = totalDeIngresos.ToString("N0");

            // Muestra la cifra formateada en el TextBox o donde desees
            string valorFormateado = $"${cifraFormateada}";
            return valorFormateado;
        }
        private async void ConsultarPresupuestos()
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetData");
                var presupuestosLocalesQuery = db.Collection("BudgetLocalData");
                presupuestosGeneral = new List<BudgetData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                presupuestosGeneral = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetData>()).ToList();
                if (presupuestosGeneral.Count > 0)
                {
                    presupuestosGeneral = presupuestosGeneral.OrderBy(budgetData => int.Parse(budgetData.Id)).ToList();
                    dataGridPresupuestos.DataSource = null;
                    dataGridPresupuestos.DataSource = presupuestosGeneral;
                    textTotal.Text = presupuestosGeneral.Count.ToString();
                    sumPresupuestos= snapshot.Documents.Sum(doc => doc.ConvertTo<BudgetData>().TotalPresupuesto);
                    textTotalPresupuestos.Text = LecturaCifra(snapshot.Documents.Sum(doc => doc.ConvertTo<BudgetData>().TotalPresupuesto));
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    textTotal.Text = "0";
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.ConsultarTodos();
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    dataGridPresupuestos.DataSource = presupuestosComites;
                    Borrar.Visible = true;
                    textTotal.Text = presupuestoService.Totalizar().Cuenta.ToString();
                }
            }
        }

        private void FormPresupuesto_Load(object sender, EventArgs e)
        {
            if (tabPresupuestos.TabCount > 0)
            {
                tabPage = tabPresupuestos.TabPages["tabPorcentajes"];
                tabPresupuestos.TabPages.RemoveAt(3);
            }
            comboFiltroAño.Text = DateTime.Now.Year.ToString();
            comboAño.Text = DateTime.Now.Year.ToString();
        }
        private async void EliminarPresupuesto(int id)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetData").Document(id.ToString());
                await docRef.DeleteAsync();
                //string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show("El registro "+id+" se ha eliminado satisfactoriamente", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridPresupuestos.DataSource = null;
                ConsultarPresupuestos();
                ObtenerDatosGenerales();
                ObtenerDatosIndividuales();
                ObtenerUltimoRegistro();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarPresupuestos();
            }
        }
        private void AgregarAGridRubros()
        {
            List<double> porcentajes = CalcularPorcentajeRubros();
            double porcentajeOfrenda = porcentajes[0];
            double porcentajeActividad = porcentajes[1];
            double porcentajeVoto = porcentajes[2];
            double porcentajeOtroConcepto = porcentajes[3];
            double porcentajeOfrendaRestante = porcentajes[4];
            double porcentajeActividadesRestante = porcentajes[5];
            double porcentajeVotosRestante = porcentajes[6];
            double porcentajeOtrosRestante = porcentajes[7];
            
            // Agregar filas al DataGridView con los valores necesarios
            dataGridRubros.Rows.Clear();
            dataGridRubros.Rows.Add("1", "Ofrenda", LecturaCifra(ofrendasIngresos), porcentajeOfrenda.ToString() + "%", porcentajeOfrendaRestante.ToString()+"%");
            dataGridRubros.Rows.Add("2", "Voto", LecturaCifra(votosIngresos), porcentajeActividad.ToString() + "%", porcentajeActividadesRestante.ToString()+"%");
            dataGridRubros.Rows.Add("3", "Actividades", LecturaCifra(activIdadesIngresos), porcentajeVoto.ToString() + "%", porcentajeVotosRestante.ToString()+"%");
            dataGridRubros.Rows.Add("4", "Otros", LecturaCifra(otrosIngresos), porcentajeOtroConcepto.ToString() + "%", porcentajeOtrosRestante.ToString()+"%");

            //Agregar a las graficas
            //limpia los puntos anteriores
            chartOfrendas.Series[0].Points.Clear();
            chartActividades.Series[0].Points.Clear();
            chartVotos.Series[0].Points.Clear();
            chartOtroConcepto.Series[0].Points.Clear();

            // Agrega los nuevos puntos al gráfico
            chartOfrendas.Series[0].Points.AddXY("Porcentaje de Ofrendas ingresadas ", porcentajeOfrenda);
            chartOfrendas.Series[0].Points.AddXY("Porcentaje de Ofrenda restante", porcentajeOfrendaRestante);
            chartActividades.Series[0].Points.AddXY("Porcentaje de Actividades ingresadas ", porcentajeActividad);
            chartActividades.Series[0].Points.AddXY("Porcentaje de Actividades restante", porcentajeActividadesRestante);
            chartVotos.Series[0].Points.AddXY("Porcentaje de Votos ingresados ", porcentajeVoto);
            chartVotos.Series[0].Points.AddXY("Porcentaje de Votos restante", porcentajeVotosRestante);
            chartOtroConcepto.Series[0].Points.AddXY("Porcentaje de Otros conceptos ingresados ",porcentajeOtroConcepto);
            chartOtroConcepto.Series[0].Points.AddXY("Porcentaje de Otro conceptos restante", porcentajeOtrosRestante);

            // Configura el gráfico para ofrendas
            chartOfrendas.Series[0].ChartType = SeriesChartType.Doughnut;
            chartOfrendas.Series[0].IsValueShownAsLabel = true;
            chartOfrendas.Series[0].LabelFormat = "#,##";
            chartOfrendas.Titles.Clear();
            chartOfrendas.Titles.Add("Porcentaje de ofrendas para " + comite);
            // Configura el gráfico para actividades
            chartActividades.Series[0].ChartType = SeriesChartType.Doughnut;
            chartActividades.Series[0].IsValueShownAsLabel = true;
            chartActividades.Series[0].LabelFormat = "#,##";
            chartActividades.Titles.Clear();
            chartActividades.Titles.Add("Porcentaje de actividades para " + comite);
            // Configura el gráfico para actividades
            chartVotos.Series[0].ChartType = SeriesChartType.Doughnut;
            chartVotos.Series[0].IsValueShownAsLabel = true;
            chartVotos.Series[0].LabelFormat = "#,##";
            chartVotos.Titles.Clear();
            chartVotos.Titles.Add("Porcentaje de votos para " + comite);
            // Configura el gráfico para Otros conceptos
            chartOtroConcepto.Series[0].ChartType = SeriesChartType.Doughnut;
            chartOtroConcepto.Series[0].IsValueShownAsLabel = true;
            chartOtroConcepto.Series[0].LabelFormat = "#,##";
            chartOtroConcepto.Titles.Clear();
            chartOtroConcepto.Titles.Add("Porcentaje de Otros conceptos para " + comite);

            tabPresupuestos.TabPages.Add(tabPage);
            tabPresupuestos.SelectedIndex = 3;
            detallo = true;
        }
        private List<double> CalcularPorcentajeRubros()
        {
            // Porcentajes del presupuesto
            int ofrenda = presupuestosComite.Sum(p => p.Ofrenda);
            int actividad = presupuestosComite.Sum(p => p.Actividad);
            int voto = presupuestosComite.Sum(p => p.Voto);
            int otroConcepto = presupuestosComite.Sum(p => p.ValorOtroConcepto);

            double porcentajeOfrenda = ofrenda!=0?ofrendasIngresos * 100 / ofrenda:0;
            double porcentajeActividad = actividad != 0?activIdadesIngresos * 100 / actividad:0;
            double porcentajeVoto = voto!=0?votosIngresos * 100 / voto:0;
            double porcentajeOtroConcepto = otroConcepto != 0 ? otrosIngresos * 100 / otroConcepto : 0;

            double diferenciaOfrenda = porcentajeOfrenda > 100 ? Math.Abs(100 - porcentajeOfrenda) : 100 - porcentajeOfrenda;
            double diferenciaActividad = porcentajeActividad > 100 ? Math.Abs(100 - porcentajeActividad) : 100 - porcentajeActividad;
            double diferenciaVoto = porcentajeVoto > 100 ? Math.Abs(100 - porcentajeVoto) : 100 - porcentajeVoto;
            double diferenciaOtroConcepto = porcentajeOtroConcepto > 100 ? Math.Abs(100 - porcentajeOtroConcepto) : 100 - porcentajeOtroConcepto;

            //Verifico si se supera el 100% en cada rubro
            porcentajeOfrenda = porcentajeOfrenda > 100 ? 100 : porcentajeOfrenda;
            porcentajeActividad = porcentajeActividad > 100 ? 100 : porcentajeActividad;
            porcentajeVoto = porcentajeVoto > 100 ? 100 : porcentajeVoto;
            porcentajeOtroConcepto = porcentajeOtroConcepto > 100 ? 100 : porcentajeOtroConcepto;

            return new List<double> { 
                porcentajeOfrenda, 
                porcentajeActividad, 
                porcentajeVoto, 
                porcentajeOtroConcepto,
                diferenciaOfrenda,
                diferenciaActividad,
                diferenciaVoto,
                diferenciaOtroConcepto
            };
        }
        private async void FiltrarPresupuestosPorComite(string comite)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetData");
                var presupuestos = new List<BudgetData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                presupuestos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                presupuestosComite = presupuestos.Where(presupuesto => presupuesto.Comite == comite).ToList();
                ObtenerIngresosYEgresos();
                dataGridPresupuestos.DataSource = presupuestosComite;
                Borrar.Visible = true;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.FiltrarPresupuestosPorComite(comite);
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                    {
                        dataGridPresupuestos.DataSource = respuesta.Presupuestos;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                }
            }
        }
        private async void FiltroPorId(int id)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestoQuery = db.Collection("BudgetData");
                var presupuestos = new List<BudgetData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestoQuery.GetSnapshotAsync();
                presupuestos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var presupuestosFiltrados = presupuestos.Where(presupuesto => presupuesto.Id == id.ToString()).ToList();
                if (presupuestosFiltrados.Any())
                {
                    var presupuestoFiltrado = presupuestosFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    comboAño.Text = presupuestoFiltrado.AñoPresupuesto;
                    comboInicioIntervalo.Text = presupuestoFiltrado.InicioIntervalo;
                    comboFinIntervalo.Text = presupuestoFiltrado.FinIntervalo;
                    comboComite.Text = presupuestoFiltrado.Comite;
                    textOfrendas.Text = LecturaCifra(presupuestoFiltrado.Ofrenda);
                    textActividades.Text = LecturaCifra(presupuestoFiltrado.Actividad);
                    textVotos.Text = LecturaCifra(presupuestoFiltrado.Voto);
                    textPresupuesto.Text= LecturaCifra(presupuestoFiltrado.TotalPresupuesto);
                    textEgresos.Text= LecturaCifra(presupuestoFiltrado.TotalEgresos);
                    encontrado = true;
                    tabPresupuestos.SelectedIndex = 1;
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaPresupuestoRespuesta respuesta = new BusquedaPresupuestoRespuesta();
                respuesta = presupuestoService.BuscarPorIdentificacion(id);
                var registro = respuesta.Presupuesto;
                if (registro != null)
                {
                    encontrado = true;
                    var presupuestos = new List<PresupuestoComite> { registro };
                    comboInicioIntervalo.Text = presupuestos[0].InicioIntervalo;
                    comboFinIntervalo.Text = presupuestos[0].FinIntervalo;
                    comboComite.Text = presupuestos[0].Comite;
                    textOfrendas.Text = presupuestos[0].Ofrenda.ToString();
                    textActividades.Text = presupuestos[0].Actividad.ToString();
                    textVotos.Text = presupuestos[0].Voto.ToString();
                    textPresupuesto.Text = presupuestos[0].TotalPresupuesto.ToString();
                    textEgresos.Text= presupuestos[0].TotalEgresos.ToString();
                }
            }
        }
        static string FormatearFecha(string fechaOriginal)
        {
            string fechaFormateada = "";
            if (fechaOriginal.Contains(" p. m."))
            {
                fechaFormateada = fechaOriginal.Replace(" p. m.", "");
                fechaFormateada = fechaFormateada + " PM";
            }
            else
            {
                if (fechaOriginal.Contains(" a. m."))
                {
                    fechaFormateada = fechaOriginal.Replace(" a. m.", "");
                    fechaFormateada = fechaFormateada + " AM";
                }
                else
                {
                    fechaFormateada = fechaOriginal;
                }
            }
            return fechaFormateada;
        }
        private async void CalculoDeSaldo()
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetData");
                // Realizar la suma directamente en la consulta Firestore
                var snapshotPresupuesto = await presupuestosQuery.GetSnapshotAsync();
                sumPresupuestos = snapshotPresupuesto.Documents.Sum(doc => doc.ConvertTo<BudgetData>().TotalPresupuesto);
                textTotalPresupuestos.Text = LecturaCifra(sumPresupuestos);
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                Console.WriteLine($"Error al calcular el saldo: {ex.Message}");
                // Manejar la excepción según tus necesidades
            }
        }
        private void Limpiar()
        {
            comboAño.Text = DateTime.Now.Year.ToString();
            comboInicioIntervalo.Text = "Enero";
            comboFinIntervalo.Text = "Enero";
            comboComite.Text = "Comite";
            textOfrendas.Text = "$ 000.00";
            textActividades.Text = "$ 000.00";
            textVotos.Text = "$ 000.00";
            textNuevoConcepto.Text = "Nuevo concepto";
            textOtroValor.Text = "$ 000.00";
            textPresupuesto.Text = "$ 000.00";
            textEgresos.Text = "$ 000.00";
        }
        private void ValidarNuevoConcepto()
        {
            if (textNuevoConcepto.Text != "" && textNuevoConcepto.Text == "Nuevo concepto")
            {
                nuevoConcepto = true;
            }
        }
        private int ObtenerCantidadEntera(string cantidad)
        {
            string cantidadSinSigno = cantidad.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos);
            return cantidadEntera;
        }
        private PresupuestoComite MapearPresupuestoComite()
        {
            presupuestoComite = new PresupuestoComite();
            presupuestoComite.Id = ultimoId;
            presupuestoComite.AñoPresupuesto = comboAño.Text;
            presupuestoComite.InicioIntervalo = comboInicioIntervalo.Text;
            presupuestoComite.FinIntervalo = comboFinIntervalo.Text;
            presupuestoComite.Comite = comboComite.Text;
            presupuestoComite.Ofrenda = ObtenerCantidadEntera(textOfrendas.Text);
            presupuestoComite.Actividad = ObtenerCantidadEntera(textActividades.Text);
            presupuestoComite.Voto = ObtenerCantidadEntera(textVotos.Text);
            presupuestoComite.OtroConcepto = nuevoConcepto ? "Ninguno" : textNuevoConcepto.Text;
            presupuestoComite.ValorOtroConcepto = nuevoConcepto ? 0 : ObtenerCantidadEntera(textOtroValor.Text);
            presupuestoComite.TotalEgresos = ObtenerCantidadEntera(textEgresos.Text);
            presupuestoComite.TotalPresupuesto = ObtenerCantidadEntera(textPresupuesto.Text);
            return presupuestoComite;
        }
        private PresupuestoIngresoLocal MapearPresupuestoIngresoLocal()
        {
            presupuestoIngresoLocal = new PresupuestoIngresoLocal();
            presupuestoIngresoLocal.Id = ultimoId;
            presupuestoIngresoLocal.AñoPresupuesto = comboAñoFondoLocal.Text;
            presupuestoIngresoLocal.InicioIntervalo = comboInicioIntervalo2.Text;
            presupuestoIngresoLocal.FinIntervalo = comboFinIntervalo2.Text;
            presupuestoIngresoLocal.Comite = comboComite2.Text;
            presupuestoIngresoLocal.Concepto = comboConceptoIngreso.Text;
            string cantidadConSigno = textValorIngreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos); // Convierte el valor a un entero
            presupuestoIngresoLocal.Valor = cantidadEntera;
            return presupuestoIngresoLocal;
        }
        private PresupuestoEgresoLocal MapearPresupuestoEgresoLocal()
        {
            presupuestoEgresoLocal = new PresupuestoEgresoLocal();
            presupuestoEgresoLocal.Id = ultimoId;
            presupuestoEgresoLocal.AñoPresupuesto = comboAñoFondoLocal.Text;
            presupuestoEgresoLocal.InicioIntervalo = comboInicioIntervalo2.Text;
            presupuestoEgresoLocal.FinIntervalo = comboFinIntervalo2.Text;
            presupuestoEgresoLocal.Comite = comboComite2.Text;
            presupuestoEgresoLocal.Concepto = comboConceptoEgreso.Text;
            string cantidadConSigno = textValorEgreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos); // Convierte el valor a un entero
            presupuestoEgresoLocal.Valor = cantidadEntera;
            return presupuestoEgresoLocal;
        }
        private void LimpiarFondoLocal()
        {
            comboAñoFondoLocal.Text = DateTime.Now.Year.ToString();
            comboInicioIntervalo2.Text="Enero";
            comboFinIntervalo2.Text="Enero";
            comboComite2.Text="Junta Local";
            comboConceptoIngreso.Text="Aporte del 12%";
            comboConceptoEgreso.Text="Santa cena";
            textValorEgreso.Text= "$ 000.00";
            textValorIngreso.Text= "$ 000.00";
        }
        //private void textPresupuesto_Validated(object sender, EventArgs e)
        //{
        //    if (textPresupuesto.Text != "" && textPresupuesto.Text != "$ 000.00")
        //    {
        //        int sumPresupuesto = int.Parse(textPresupuesto.Text);
        //        textPresupuesto.Text = LecturaCifra(sumPresupuesto);
        //    }
        //}

        private void TotalizarConceptos()
        {
            int ofrendas = ObtenerCantidadEntera(textOfrendas.Text);
            int actividades = ObtenerCantidadEntera(textActividades.Text);
            int votos = ObtenerCantidadEntera(textVotos.Text);
            int otro = ObtenerCantidadEntera(textOtroValor.Text);
            int totalConceptos = ofrendas + actividades + votos + otro;
            textPresupuesto.Text = LecturaCifra(totalConceptos);
        }
        private async void ObtenerIngresosYEgresos()
        {
            if (comite != "Comite" && comite!="")
            {
                try
                {
                    var db = FirebaseService.Database;
                    var ingresosQuery = db.Collection("IngressData").WhereEqualTo("Comite", comite);
                    ingressData = new List<IngressData>();
                    // Realizar la suma directamente en la consulta Firestore
                    var snapshot = await ingresosQuery.GetSnapshotAsync();
                    ingressData = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                    // Filtrar ingresos por concepto y contar la cantidad de cada uno
                    sumIgresos = ingressData.Sum(p => p.Valor);
                    ofrendasIngresos = ingressData.Where(i => i.Concepto == "Ofrenda").Sum(p => p.Valor);
                    votosIngresos = ingressData.Where(i => i.Concepto == "Voto").Sum(p => p.Valor);
                    activIdadesIngresos = ingressData.Where(i => i.Concepto == "Actividades").Sum(p => p.Valor);
                    otrosIngresos = ingressData.Where(i => i.Concepto == "Otros").Sum(p => p.Valor);
                    var egresosQuery = db.Collection("EgressData").WhereEqualTo("Comite", comite);
                    egressData = new List<EgressData>();
                    // Realizar la suma directamente en la consulta Firestore
                    snapshot = await egresosQuery.GetSnapshotAsync();
                    egressData = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                    sumEgresos = egressData.Sum(p => p.Valor);
                    ObtenerDatosGenerales();
                    ObtenerDatosIndividuales();
                    AgregarAGridRubros();
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                try
                {
                    var db = FirebaseService.Database;
                    var ingresosQuery = db.Collection("IngressData");
                    ingressData = new List<IngressData>();
                    // Realizar la suma directamente en la consulta Firestore
                    var snapshots = await ingresosQuery.GetSnapshotAsync();
                    ingressData = snapshots.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                    // Filtrar ingresos por concepto y contar la cantidad de cada uno
                    var egresosQuery = db.Collection("EgressData");
                    egressData = new List<EgressData>();
                    snapshots = await egresosQuery.GetSnapshotAsync();
                    egressData = snapshots.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                }
                catch
                {

                }
            }
        }
        private void ObtenerDatosIndividuales()
        {
            //Este se representará con los datos individuales de cada comite que son ofrendas, actividades, votos y otro.
            if (presupuestosComite != null && presupuestosComite.Any())
            {
                // Presupuestos 100% por conceptos
                int ofrenda = presupuestosComite.Sum(p => p.Ofrenda);
                int actividad = presupuestosComite.Sum(p => p.Actividad);
                int voto = presupuestosComite.Sum(p => p.Voto);
                int otroConcepto = presupuestosComite.Sum(p => p.ValorOtroConcepto);
                int egresos = presupuestosComite.Sum(p => p.TotalEgresos);

                // Calcula los porcentajes con respecto a los ingresos
                double porcentajeOfrenda = ofrenda!=0 ? ofrendasIngresos * 100 / ofrenda:0;
                double porcentajeActividad = actividad!=0?activIdadesIngresos * 100 / actividad:0;
                double porcentajeVoto = voto!=0?votosIngresos * 100 / voto:0;
                double porcentajeOtroConcepto = otrosIngresos != 0 ? otrosIngresos * 100 / otroConcepto : 0;

                double porcentajeEgresos = sumEgresos * 100 / egresos;
                double porcentajeEgresosRestante = 100 - porcentajeEgresos;

                //Calcula los nuevos porcentajes con respecto al presupuesto
                var OfrendaRebasada = "";
                var ActividadRebasada = "";
                var VotoRebasado = "";
                var OtroConceptoRebosado = "";
                var EgresosRebasados = "";
                if (porcentajeOfrenda > 100)
                {
                    OfrendaRebasada = "rebasada en un " + (porcentajeOfrenda - 100).ToString() + "%";
                    porcentajeOfrenda = 100;
                }
                if (porcentajeActividad > 100)
                {
                    ActividadRebasada = "rebasada en un " + (porcentajeActividad - 100).ToString() + "%";
                    porcentajeActividad = (actividad * 100) / presupuestosComite[0].TotalPresupuesto;
                }
                if (porcentajeVoto > 100)
                {
                    VotoRebasado = "rebasado en un " + (porcentajeVoto - 100).ToString() + "%";
                    porcentajeVoto = (voto * 100) / presupuestosComite[0].TotalPresupuesto;
                }
                if (porcentajeOtroConcepto > 100)
                {
                    OtroConceptoRebosado = "rebasado en un " + (porcentajeOtroConcepto - 100).ToString() + "%";
                    porcentajeOtroConcepto = (otroConcepto * 100) / presupuestosComite[0].TotalPresupuesto;
                }
                if (porcentajeEgresos > 100)
                {
                    EgresosRebasados = "rebasados en un " + (porcentajeEgresos - 100).ToString() + "%";
                    porcentajeEgresos = (egresos * 100) / presupuestosComite[0].TotalEgresos;

                }

                // Limpia los puntos anteriores
                chartIngresoIndividual.Series[0].Points.Clear();
                chartEgresoIndividual.Series[0].Points.Clear();

                // Agrega los nuevos puntos al gráfico
                chartIngresoIndividual.Series[0].Points.AddXY("Ofrenda " + OfrendaRebasada, porcentajeOfrenda);
                chartIngresoIndividual.Series[0].Points.AddXY("Actividad " + ActividadRebasada, porcentajeActividad);
                chartIngresoIndividual.Series[0].Points.AddXY("Voto " + VotoRebasado, porcentajeVoto);
                chartIngresoIndividual.Series[0].Points.AddXY("Otro Concepto " + OtroConceptoRebosado, porcentajeOtroConcepto);

                chartEgresoIndividual.Series[0].Points.AddXY("Egresos " + EgresosRebasados, porcentajeEgresos);
                chartEgresoIndividual.Series[0].Points.AddXY("Egresos restantes", porcentajeEgresosRestante);

                // Configura el gráfico ingresos
                chartIngresoIndividual.Series[0].ChartType = SeriesChartType.Doughnut;
                chartIngresoIndividual.Series[0].IsValueShownAsLabel = true;
                chartIngresoIndividual.Series[0].LabelFormat = "#,##";
                chartIngresoIndividual.Titles.Clear();
                chartIngresoIndividual.Titles.Add("Distribución de ingresos para " + comite);
                // Configura el gráfico egresos
                chartEgresoIndividual.Series[0].ChartType = SeriesChartType.Doughnut;
                chartEgresoIndividual.Series[0].IsValueShownAsLabel = true;
                chartEgresoIndividual.Series[0].LabelFormat = "#,##";
                chartEgresoIndividual.Titles.Clear();
                chartEgresoIndividual.Titles.Add("Distribución de egresos para " + comite);
            }
        }
        private void ObtenerDatosGenerales()
        {
            if (presupuestosGeneral != null && presupuestosGeneral.Any())
            {
                double porcentajeActual = sumIgresos * 100/ sumPresupuestos;
                double porcentajeRestante = Math.Abs(100 - porcentajeActual);
                string porcentajeRebasado="";
                if (porcentajeActual > 100)
                {
                    porcentajeRebasado = "rebasado en un " + (porcentajeActual - 100).ToString() + "%";
                    porcentajeActual = 100;
                }
                chartGeneral.Series[0].Points.Clear();
                chartGeneral.Series[0].Points.AddXY("Presupuesto acomulado" + porcentajeRebasado, porcentajeActual);
                chartGeneral.Series[0].Points.AddXY("Presupuesto restante", porcentajeRestante);
                // Configura el gráfico presupuesto
                chartGeneral.Series[0].ChartType = SeriesChartType.Doughnut;
                chartGeneral.Series[0].IsValueShownAsLabel = true;
                chartGeneral.Series[0].LabelFormat = "#,##";
                chartGeneral.Titles.Clear();
                chartGeneral.Titles.Add("Progreso de presupuestos generales");
            }
        }

        private void tabPresupuestos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabPresupuestos.SelectedIndex != 3 && detallo != false)
            {
                tabPage = tabPresupuestos.TabPages["tabPorcentajes"];
                tabPresupuestos.TabPages.RemoveAt(3);
                ConsultarPresupuestos();
                detallo = false;
                Limpiar();
            }
        }

        private void dataGridPresupuestos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridPresupuestos.DataSource != null)
            {
                if (dataGridPresupuestos.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToInt32(dataGridPresupuestos.CurrentRow.Cells["Id"].Value.ToString());
                    EliminarPresupuesto(id);
                    ConsultarPresupuestos();
                }
                else
                {
                    if (dataGridPresupuestos.Columns[e.ColumnIndex].Name == "Detallar")
                    {
                        comite = Convert.ToString(dataGridPresupuestos.CurrentRow.Cells["Comite"].Value.ToString());
                        FiltrarPresupuestosPorComite(comite);
                    }
                    else
                    {
                        if (dataGridPresupuestos.Columns[e.ColumnIndex].Name == "Editar")
                        {
                            id = Convert.ToInt32(dataGridPresupuestos.CurrentRow.Cells["Id"].Value.ToString());
                            btnRegistrar.Enabled = false;
                            btnModificar.Enabled = true;
                            FiltroPorId(id);
                        }
                        else
                        {
                            if (dataGridPresupuestos.Columns[e.ColumnIndex].Name == "Seleccionar")
                            {
                                // Verificar si el clic se realizó en la columna de CheckBoxColumn
                                if (e.ColumnIndex == dataGridPresupuestos.Columns["Seleccionar"].Index && e.RowIndex != -1)
                                {
                                    // Obtener la celda de CheckBox clicada
                                    DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)dataGridPresupuestos.Rows[e.RowIndex].Cells["Seleccionar"];
                                    // Cambiar el estado del CheckBox
                                    checkBoxCell.Value = !Convert.ToBoolean(checkBoxCell.Value);
                                    // Consulta
                                    comite = Convert.ToString(dataGridPresupuestos.CurrentRow.Cells["Comite"].Value.ToString());
                                    FiltrarPresupuestosPorComite(comite);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.ingress = ingressData;
            formGenerarDocumento.egress = egressData;
            formGenerarDocumento.presupuestosGeneral = presupuestosGeneral;
            formGenerarDocumento.budgetIngressLocals = budgetIngressLocal;
            formGenerarDocumento.budgetEgressLocals = budgetEgressLocal;
            formGenerarDocumento.Show();
        }
        private async void FiltroPorAño(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetData");
                presupuestosGeneral = new List<BudgetData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                presupuestosGeneral = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetData>()).ToList();
                var presupuestosFiltrados = presupuestosGeneral.Where(presupuesto => presupuesto.AñoPresupuesto == filtro).ToList();
                dataGridPresupuestos.DataSource = presupuestosFiltrados;
                Borrar.Visible = true;
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
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.FiltrarPresupuestosPorAño(filtro);
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                    {
                        dataGridPresupuestos.DataSource = respuesta.Presupuestos;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                }
            }
        }
        private async void FiltroPorComite(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetData");
                presupuestosGeneral = new List<BudgetData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                presupuestosGeneral = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetData>()).ToList();
                var presupuestosFiltrados= presupuestosGeneral.Where(presupuesto => presupuesto.Comite == filtro).ToList();
                dataGridPresupuestos.DataSource = presupuestosFiltrados;
                Borrar.Visible = true;
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
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.FiltrarPresupuestosPorComite(filtro);
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                    {
                        dataGridPresupuestos.DataSource = respuesta.Presupuestos;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                }
            }
        }
        private void comboFiltroComite_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroComite.Text;
            if (filtro == "Todos")
            {
                ConsultarPresupuestos();
            }
            else
            {
                comboFiltroAño.Text = DateTime.Now.Year.ToString();
                FiltroPorComite(filtro);
            }
        }

        private void comboFiltroAño_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroAño.Text;
            if (filtro == "Todos")
            {
                ConsultarPresupuestos();
            }
            else
            {
                comboFiltroComite.Text = "Todos";
                FiltroPorAño(filtro);
            }
        }
        private async void ObtenerUltimoRegistro()
        {
            try
            {
                var db = FirebaseService.Database;

                // Obtener el máximo número de comprobante directamente desde Firestore
                var budgetQuery = db.Collection("BudgetData").OrderByDescending("Id");
                var budgetIngressLocal = db.Collection("BudgetIngressLocalData").OrderByDescending("Id");
                var budgetEgressLocal = db.Collection("BudgetEgressLocalData").OrderByDescending("Id");
                var budgetSnapshot = await budgetQuery.GetSnapshotAsync();
                var budgetIngressLocalSnapshot = await budgetIngressLocal.GetSnapshotAsync();
                var budgetEgressLocalSnapshot = await budgetEgressLocal.GetSnapshotAsync();
                if (budgetSnapshot.Documents.Count > 0)
                {
                    var idPresupuestoCurrent = budgetSnapshot.Documents.Count!=0?budgetSnapshot.Documents.Max(p => Convert.ToInt32(p.Id)):1;
                    var idIngresosFondosCurrent = budgetIngressLocalSnapshot.Documents.Count!=0?budgetIngressLocalSnapshot.Documents.Max(p => Convert.ToInt32(p.Id)):1;
                    var idEgresosFondosCurrent = budgetEgressLocalSnapshot.Documents.Count!=0?budgetEgressLocalSnapshot.Documents.Max(p => Convert.ToInt32(p.Id)):1;
                    // Usar LINQ para encontrar el máximo
                    int maximo = new[] { idPresupuestoCurrent, idIngresosFondosCurrent, idEgresosFondosCurrent }.Max();
                    int numeroMayor = maximo + 1;
                    ultimoId = numeroMayor;
                }
                else
                {
                    ultimoId = 1;
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
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.ConsultarTodos();
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    var ultimopresupuesto = respuesta.Presupuestos.Last();
                    ultimoId = ultimopresupuesto.Id;
                }
            }
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            ValidarNuevoConcepto();
            PresupuestoComite presupuesto = MapearPresupuestoComite();
            try
            {
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetNuevo = budgetMaps.BudgetMap(presupuesto);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetData").Document(budgetNuevo.Id);
                docRef.SetAsync(budgetNuevo);
                // Guardamos localmente
                string msg = "Presupuesto agregado correctamente";
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarPresupuestos();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                Limpiar();
                tabPresupuestos.SelectedIndex = 0;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    var msg = presupuestoService.Guardar(presupuesto);
                    MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    //Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }
        private void btnModificar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            PresupuestoComite presupuesto = MapearPresupuestoComite();
            try
            {
                var msg = presupuestoService.Guardar(presupuesto);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetNuevo = budgetMaps.BudgetMap(presupuesto);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetData").Document(budgetNuevo.Id);
                docRef.SetAsync(budgetNuevo);
                // Guardamos localmente
                MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarPresupuestos();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                Limpiar();
                tabPresupuestos.SelectedIndex = 0;
                btnRegistrar.Enabled = true;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    var msg = presupuestoService.Guardar(presupuesto);
                    MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }
        private async void ConsultarIngresosFondosLocales()
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetIngressLocalData");
                budgetIngressLocal = new List<BudgetIngressLocalData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                budgetIngressLocal = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetIngressLocalData>()).ToList();
                if (budgetIngressLocal.Count > 0)
                {
                    budgetIngressLocal = budgetIngressLocal.OrderBy(budgetData => int.Parse(budgetData.Id)).ToList();
                    dataGridIngresos.DataSource = null;
                    dataGridIngresos.DataSource = budgetIngressLocal;
                    sumIngresosLocales = snapshot.Documents.Sum(doc => Convert.ToInt32(doc.ConvertTo<BudgetIngressLocalData>().Valor));
                    textTotalPresupuesto.Text = LecturaCifra(sumIngresosLocales);
                    sumPresupuestos = sumPresupuestos + sumIngresosLocales;
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    textTotalPresupuesto.Text = "$ 000.00";
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.ConsultarTodos();
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    dataGridPresupuestos.DataSource = presupuestosComites;
                    Borrar.Visible = true;
                    textTotal.Text = presupuestoService.Totalizar().Cuenta.ToString();
                }
            }
        }
        private async void ConsultarEgresosFondosLocales()
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("BudgetEgressLocalData");
                budgetEgressLocal = new List<BudgetEgressLocalData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                budgetEgressLocal = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetEgressLocalData>()).ToList();
                if (budgetEgressLocal.Count > 0)
                {
                    budgetEgressLocal = budgetEgressLocal.OrderBy(budgetData => int.Parse(budgetData.Id)).ToList();
                    dataGridEgresos.DataSource = null;
                    dataGridEgresos.DataSource = budgetEgressLocal;
                    sumEgresosLocales = snapshot.Documents.Sum(doc => Convert.ToInt32(doc.ConvertTo<BudgetEgressLocalData>().Valor));
                    textEgresosTotales.Text = LecturaCifra(sumEgresosLocales);
                }
                else
                {
                    dataGridPresupuestos.DataSource = null;
                    textTotalPresupuesto.Text = "$ 000.00";
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaPresupuestoRespuesta respuesta = new ConsultaPresupuestoRespuesta();
                respuesta = presupuestoService.ConsultarTodos();
                if (respuesta.Presupuestos.Count != 0 && respuesta.Presupuestos != null)
                {
                    dataGridPresupuestos.DataSource = null;
                    presupuestosComites = respuesta.Presupuestos.ToList();
                    dataGridPresupuestos.DataSource = presupuestosComites;
                    Borrar.Visible = true;
                    textTotal.Text = presupuestoService.Totalizar().Cuenta.ToString();
                }
            }

        }
        private void btnGuardarIngreso_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            PresupuestoIngresoLocal presupuestoIngresoLocal = MapearPresupuestoIngresoLocal();
            try
            {
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetIngressLocalNuevo = budgetLocalMaps.BudgetIngressLocalMap(presupuestoIngresoLocal);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetIngressLocalData").Document(budgetIngressLocalNuevo.Id);
                docRef.SetAsync(budgetIngressLocalNuevo);
                // Guardamos localmente
                string msg = "Ingreso agregado correctamente al presupuesto de junta local";
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresosFondosLocales();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                LimpiarFondoLocal();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    //var msg = presupuestoService.Guardar(presupuestoIngresoLocal);
                    //MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    //Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }

        private void btnModificarIngreso_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            PresupuestoIngresoLocal presupuestoIngresoLocal = MapearPresupuestoIngresoLocal();
            try
            {
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetIngressLocalNuevo = budgetLocalMaps.BudgetIngressLocalMap(presupuestoIngresoLocal);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetIngressLocalData").Document(budgetIngressLocalNuevo.Id);
                docRef.SetAsync(budgetIngressLocalNuevo);
                // Mostramos el mensaje de modificacion
                string msg = "Se ha modificado el registro "+ budgetIngressLocalNuevo.Id+" correctamente";
                MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresosFondosLocales();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                LimpiarFondoLocal();
                tabPresupuestos.SelectedIndex = 0;
                btnGuardarIngreso.Enabled = true;
                btnModificarIngreso.Enabled = false;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    //var msg = presupuestoService.Guardar(presupuestoIngresoLocal);
                    //MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }

        private void btnGuardarEgreso_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            PresupuestoEgresoLocal presupuestoEgresoLocal = MapearPresupuestoEgresoLocal();
            try
            {
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetEgressLocalNuevo = budgetLocalMaps.BudgetEgressLocalMap(presupuestoEgresoLocal);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetEgressLocalData").Document(budgetEgressLocalNuevo.Id);
                docRef.SetAsync(budgetEgressLocalNuevo);
                // Guardamos localmente
                string msg = "Egreso agregado correctamente al presupuesto de junta local";
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresosFondosLocales();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                LimpiarFondoLocal();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    //var msg = presupuestoService.Guardar(presupuestoIngresoLocal);
                    //MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    //Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }

        private void btnModificarEgreso_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            PresupuestoEgresoLocal presupuestoEgresoLocal = MapearPresupuestoEgresoLocal();
            try
            {
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var budgetEgressLocalNuevo = budgetLocalMaps.BudgetEgressLocalMap(presupuestoEgresoLocal);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetEgressLocalData").Document(budgetEgressLocalNuevo.Id);
                docRef.SetAsync(budgetEgressLocalNuevo);
                // Mostramos el mensaje de modificacion
                string msg = "Se ha modificado el registro " + budgetEgressLocalNuevo.Id + " correctamente";
                MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresosFondosLocales();
                CalculoDeSaldo();
                ObtenerUltimoRegistro();
                LimpiarFondoLocal();
                tabPresupuestos.SelectedIndex = 0;
                btnGuardarEgreso.Enabled = true;
                btnModificarEgreso.Enabled = false;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
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
                    // Guardamos localmente
                    //var msg = presupuestoService.Guardar(presupuestoEgresoLocal);
                    //MessageBox.Show(msg, "Modificado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarPresupuestos();
                    Limpiar();
                    tabPresupuestos.SelectedIndex = 0;
                }
            }
        }
        private void textOfrendas_Enter(object sender, EventArgs e)
        {
            if (textOfrendas.Text != "")
            {
                textOfrendas.Text = "";
            }
        }
        private void textOfrendas_Leave(object sender, EventArgs e)
        {
            if (textOfrendas.Text == "")
            {
                textOfrendas.Text = "$ 000.00";
            }
        }
        private void textOfrendas_Validated(object sender, EventArgs e)
        {
            if (textOfrendas.Text != "" && textOfrendas.Text != "$ 000.00")
            {
                int sumOfrendas = int.Parse(textOfrendas.Text);
                textOfrendas.Text = LecturaCifra(sumOfrendas);
            }
        }

        private void textActividades_Validated(object sender, EventArgs e)
        {
            if (textActividades.Text != "" && textActividades.Text != "$ 000.00")
            {
                int sumActividades = int.Parse(textActividades.Text);
                textActividades.Text = LecturaCifra(sumActividades);
            }
        }

        private void textActividades_Enter(object sender, EventArgs e)
        {
            if (textActividades.Text != "")
            {
                textActividades.Text = "";
            }
        }

        private void textActividades_Leave(object sender, EventArgs e)
        {
            if (textActividades.Text == "")
            {
                textActividades.Text = "$ 000.00";
            }
        }

        private void textVotos_Validated(object sender, EventArgs e)
        {
            if (textVotos.Text != "" && textVotos.Text != "$ 000.00")
            {
                int sumVotos = int.Parse(textVotos.Text);
                textVotos.Text = LecturaCifra(sumVotos);
            }
        }

        private void textOtroValor_Validated(object sender, EventArgs e)
        {
            if (textOtroValor.Text != "" && textOtroValor.Text != "$ 000.00")
            {
                int sumOtroValor = int.Parse(textOtroValor.Text);
                textOtroValor.Text = LecturaCifra(sumOtroValor);
            }
        }

        private void textNuevoConcepto_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNuevoConcepto.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNuevoConcepto.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNuevoConcepto_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNuevoConcepto.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNuevoConcepto.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textVotos_Enter(object sender, EventArgs e)
        {
            if (textVotos.Text != "")
            {
                textVotos.Text = "";
            }
        }

        private void textVotos_Leave(object sender, EventArgs e)
        {
            if (textVotos.Text == "")
            {
                textVotos.Text = "$ 000.00";
            }
        }

        private void textOtroValor_Enter(object sender, EventArgs e)
        {
            if (textOtroValor.Text != "")
            {
                textOtroValor.Text = "";
            }
        }

        private void textOtroValor_Leave(object sender, EventArgs e)
        {
            if (textOtroValor.Text == "")
            {
                textOtroValor.Text = "$ 000.00";
            }
        }

        private void textPresupuesto_Enter(object sender, EventArgs e)
        {
            if (textPresupuesto.Text != "")
            {
                textPresupuesto.Text = "";
            }
            TotalizarConceptos();
        }

        private void textPresupuesto_Leave(object sender, EventArgs e)
        {
            if (textPresupuesto.Text == "")
            {
                textPresupuesto.Text = "$ 000.00";
            }
        }

        private void textEgresos_Enter(object sender, EventArgs e)
        {
            if (textEgresos.Text != "")
            {
                textEgresos.Text = "";
            }
        }

        private void textEgresos_Leave(object sender, EventArgs e)
        {
            if (textEgresos.Text == "")
            {
                textEgresos.Text = "$ 000.00";
            }
        }

        private void btnInformePorcentaje_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }

        private void btnRegistrarFondos_Click(object sender, EventArgs e)
        {
            tabPresupuestos.SelectedIndex = 2;
        }

        private void btnRegistrarPresupuestos_Click(object sender, EventArgs e)
        {
            tabPresupuestos.SelectedIndex = 1;
        }

        private void textValorIngreso_Enter(object sender, EventArgs e)
        {
            if (textValorIngreso.Text != "")
            {
                textValorIngreso.Text = "";
            }
        }

        private void textValorIngreso_Leave(object sender, EventArgs e)
        {
            if (textValorIngreso.Text == "")
            {
                textValorIngreso.Text = "$ 000.00";
            }
        }

        private void textValorIngreso_Validated(object sender, EventArgs e)
        {
            if (textValorIngreso.Text != "" && textValorIngreso.Text != "$ 000.00")
            {
                int valorIngreso = int.Parse(textValorIngreso.Text);
                textValorIngreso.Text = LecturaCifra(valorIngreso);
            }
        }

        private void textTotalPresupuesto_Enter(object sender, EventArgs e)
        {
            if (textTotalPresupuesto.Text != "")
            {
                textTotalPresupuesto.Text = "";
            }
        }

        private void textTotalPresupuesto_Leave(object sender, EventArgs e)
        {
            if (textTotalPresupuesto.Text == "")
            {
                textTotalPresupuesto.Text = "$ 000.00";
            }
        }

        private void textTotalPresupuesto_Validated(object sender, EventArgs e)
        {
            if (textTotalPresupuesto.Text != "" && textTotalPresupuesto.Text != "$ 000.00")
            {
                int valorIngreso = int.Parse(textTotalPresupuesto.Text);
                textTotalPresupuesto.Text = LecturaCifra(valorIngreso);
            }
        }

        private void textValorEgreso_Enter(object sender, EventArgs e)
        {
            if (textValorEgreso.Text != "")
            {
                textValorEgreso.Text = "";
            }
        }

        private void textValorEgreso_Leave(object sender, EventArgs e)
        {
            if (textValorEgreso.Text == "")
            {
                textValorEgreso.Text = "$ 000.00";
            }
        }

        private void textValorEgreso_Validated(object sender, EventArgs e)
        {
            if (textValorEgreso.Text != "" && textValorEgreso.Text != "$ 000.00")
            {
                int valorIngreso = int.Parse(textValorEgreso.Text);
                textValorEgreso.Text = LecturaCifra(valorIngreso);
            }
        }

        private void textEgresosTotales_Enter(object sender, EventArgs e)
        {
            if (textEgresosTotales.Text != "")
            {
                textEgresosTotales.Text = "";
            }
        }

        private void textEgresosTotales_Leave(object sender, EventArgs e)
        {
            if (textEgresosTotales.Text == "")
            {
                textEgresosTotales.Text = "$ 000.00";
            }
        }

        private void textEgresosTotales_Validated(object sender, EventArgs e)
        {
            if (textEgresosTotales.Text != "" && textEgresosTotales.Text != "$ 000.00")
            {
                int valorIngreso = int.Parse(textEgresosTotales.Text);
                textEgresosTotales.Text = LecturaCifra(valorIngreso);
            }
        }
        private async void EliminarIngresoFondo(int id)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetIngressLocalData").Document(id.ToString());
                await docRef.DeleteAsync();
                //string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show("El registro " + id + " se ha eliminado satisfactoriamente", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridIngresos.DataSource = null;
                ConsultarIngresosFondosLocales();
                ObtenerDatosGenerales();
                ObtenerDatosIndividuales();
                ObtenerUltimoRegistro();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresosFondosLocales();
            }
        }
        private async void EliminarEgresoFondo(int id)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetEgressLocalData").Document(id.ToString());
                await docRef.DeleteAsync();
                //string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show("El registro " + id + " se ha eliminado satisfactoriamente", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridEgresos.DataSource = null;
                ConsultarEgresosFondosLocales();
                ObtenerDatosGenerales();
                ObtenerDatosIndividuales();
                ObtenerUltimoRegistro();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                string mensaje = presupuestoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresosFondosLocales();
            }
        }
        private async void FiltrarIngresosLocales(int id)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestoIngressLocalQuery = db.Collection("BudgetIngressLocalData");
                var presupuestos = new List<BudgetIngressLocalData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestoIngressLocalQuery.GetSnapshotAsync();
                presupuestos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetIngressLocalData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var presupuestosFiltrados = presupuestos.Where(presupuesto => presupuesto.Id == id.ToString()).ToList();
                if (presupuestosFiltrados.Any())
                {
                    var presupuestoFiltrado = presupuestosFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    ultimoId = Convert.ToInt32(presupuestoFiltrado.Id);
                    comboAñoFondoLocal.Text = presupuestoFiltrado.AñoPresupuesto;
                    comboInicioIntervalo2.Text = presupuestoFiltrado.InicioIntervalo;
                    comboFinIntervalo2.Text = presupuestoFiltrado.FinIntervalo;
                    comboComite2.Text = presupuestoFiltrado.Comite;
                    comboConceptoIngreso.Text = presupuestoFiltrado.Concepto;
                    textValorIngreso.Text = LecturaCifra(presupuestoFiltrado.Valor);
                    encontrado = true;
                    tabPresupuestos.SelectedIndex = 2;
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaPresupuestoRespuesta respuesta = new BusquedaPresupuestoRespuesta();
                respuesta = presupuestoService.BuscarPorIdentificacion(id);
                var registro = respuesta.Presupuesto;
                if (registro != null)
                {
                    encontrado = true;
                    var presupuestos = new List<PresupuestoComite> { registro };
                    comboInicioIntervalo.Text = presupuestos[0].InicioIntervalo;
                    comboFinIntervalo.Text = presupuestos[0].FinIntervalo;
                    comboComite.Text = presupuestos[0].Comite;
                    textOfrendas.Text = presupuestos[0].Ofrenda.ToString();
                    textActividades.Text = presupuestos[0].Actividad.ToString();
                    textVotos.Text = presupuestos[0].Voto.ToString();
                    textPresupuesto.Text = presupuestos[0].TotalPresupuesto.ToString();
                    textEgresos.Text = presupuestos[0].TotalEgresos.ToString();
                }
            }
        }
        private async void FiltrarEgresosLocales(int id)
        {
            try
            {
                var db = FirebaseService.Database;
                var presupuestoEgressLocalQuery = db.Collection("BudgetEgressLocalData");
                var presupuestos = new List<BudgetEgressLocalData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestoEgressLocalQuery.GetSnapshotAsync();
                presupuestos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<BudgetEgressLocalData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var presupuestosFiltrados = presupuestos.Where(presupuesto => presupuesto.Id == id.ToString()).ToList();
                if (presupuestosFiltrados.Any())
                {
                    var presupuestoFiltrado = presupuestosFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    ultimoId = Convert.ToInt32(presupuestoFiltrado.Id);
                    comboAñoFondoLocal.Text = presupuestoFiltrado.AñoPresupuesto;
                    comboInicioIntervalo2.Text = presupuestoFiltrado.InicioIntervalo;
                    comboFinIntervalo2.Text = presupuestoFiltrado.FinIntervalo;
                    comboComite2.Text = presupuestoFiltrado.Comite;
                    comboConceptoEgreso.Text = presupuestoFiltrado.Concepto;
                    textValorEgreso.Text = LecturaCifra(Convert.ToInt32(presupuestoFiltrado.Valor));
                    encontrado = true;
                    tabPresupuestos.SelectedIndex = 2;
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
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaPresupuestoRespuesta respuesta = new BusquedaPresupuestoRespuesta();
                respuesta = presupuestoService.BuscarPorIdentificacion(id);
                var registro = respuesta.Presupuesto;
                if (registro != null)
                {
                    encontrado = true;
                    var presupuestos = new List<PresupuestoComite> { registro };
                    comboInicioIntervalo.Text = presupuestos[0].InicioIntervalo;
                    comboFinIntervalo.Text = presupuestos[0].FinIntervalo;
                    comboComite.Text = presupuestos[0].Comite;
                    textOfrendas.Text = presupuestos[0].Ofrenda.ToString();
                    textActividades.Text = presupuestos[0].Actividad.ToString();
                    textVotos.Text = presupuestos[0].Voto.ToString();
                    textPresupuesto.Text = presupuestos[0].TotalPresupuesto.ToString();
                    textEgresos.Text = presupuestos[0].TotalEgresos.ToString();
                }
            }
        }
        private void dataGridIngresos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridIngresos.DataSource != null)
            {
                if (dataGridIngresos.Columns[e.ColumnIndex].Name == "EliminarIngreso")
                {
                    id = Convert.ToInt32(dataGridIngresos.CurrentRow.Cells["Id"].Value.ToString());
                    EliminarIngresoFondo(id);
                    ConsultarIngresosFondosLocales();
                }
                else
                {
                    if (dataGridIngresos.Columns[e.ColumnIndex].Name == "EditarIngreso")
                    {
                        id = Convert.ToInt32(dataGridIngresos.CurrentRow.Cells["Id"].Value.ToString());
                        btnGuardarIngreso.Enabled = false;
                        btnModificarIngreso.Enabled = true;
                        FiltrarIngresosLocales(id);
                    }
                }
            }
        }
        private void dataGridEgresos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridEgresos.DataSource != null)
            {
                if (dataGridEgresos.Columns[e.ColumnIndex].Name == "EliminarEgreso")
                {
                    id = Convert.ToInt32(dataGridEgresos.CurrentRow.Cells["Id"].Value.ToString());
                    EliminarEgresoFondo(id);
                    ConsultarEgresosFondosLocales();
                }
                else
                {
                    if (dataGridEgresos.Columns[e.ColumnIndex].Name == "EditarEgreso")
                    {
                        id = Convert.ToInt32(dataGridEgresos.CurrentRow.Cells["Id"].Value.ToString());
                        btnGuardarEgreso.Enabled = false;
                        btnModificarEgreso.Enabled = true;
                        FiltrarEgresosLocales(id);
                    }
                }
            }
        }
    }
}
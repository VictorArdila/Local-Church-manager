using BLL;
using Cloud;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Entity;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Ldb;

namespace UI
{
    public partial class FormEgresos : Form
    {
        Egreso egreso;
        EgresoService egresoService;
        List<Egreso> egresos;
        List<EgressData> egress;
        EgressMaps egressMaps;
        TabPage tabPage;
        string originalText;
        string comprobante = "";
        string id;
        string comite;
        bool encontrado = false;
        bool detallo = false;
        int sumIngreso = 0;
        int sumEgreso = 0;
        int saldo = 0;
        string filtro = "";
        public FormEgresos()
        {
            egresoService = new EgresoService(DbConfig.ConnectionString);
            egressMaps = new EgressMaps();
            InitializeComponent();
            ConsultarEgresos();
            CalcularComprobante();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnGestionarEgresos_Click(object sender, EventArgs e)
        {
            tabEgresos.SelectedIndex = 1;
        }
        private string LecturaCifra(int totalDeIngresos)
        {
            // Convierte el total de ingresos a una cadena con separadores de miles
            string cifraFormateada = totalDeIngresos.ToString("N0");

            // Muestra la cifra formateada en el TextBox o donde desees
            string valorFormateado = $"${cifraFormateada}";
            return valorFormateado;
        }
        //private async void CalculoDeSaldo()
        //{
        //    try
        //    {
        //        var db = FirebaseService.Database;
        //        var ingresosQuery = db.Collection("IngressData");
        //        var egresosQuery = db.Collection("EgressData");
        //        // Realizar la suma directamente en la consulta Firestore
        //        var snapshotIngress = await ingresosQuery.GetSnapshotAsync();
        //        var snapshotEgress = await egresosQuery.GetSnapshotAsync();
        //        sumIngreso = snapshotIngress.Documents.Sum(doc => doc.ConvertTo<IngressData>().Valor);
        //        sumEgreso = snapshotEgress.Documents.Sum(doc => doc.ConvertTo<EgressData>().Valor);
        //        // Calcular el saldo después de procesar todos los documentos
        //        // Obtener referencia al formulario principal
        //        FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //        // Verificar si el formulario principal está abierto
        //        if (formPrincipal != null)
        //        {
        //            // Lanzar el evento para notificar al formulario principal sobre la excepción
        //            formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Obtener referencia al formulario principal
        //        FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //        // Verificar si el formulario principal está abierto
        //        if (formPrincipal != null)
        //        {
        //            // Lanzar el evento para notificar al formulario principal sobre la excepción
        //            formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
        //        }
        //        Console.WriteLine($"Error al calcular el saldo: {ex.Message}");
        //        // Manejar la excepción según tus necesidades
        //    }
        //}
        private async void TotalizarEgresosYCalculoDeSaldo(string filtro)
        {
            try
            {
                sumEgreso = 0;
                var db = FirebaseService.Database;
                var ingresosQuery = db.Collection("IngressData");
                var egresosQuery = db.Collection("EgressData");
                // Realizar la suma directamente en la consulta Firestore
                var snapshotIngress = await ingresosQuery.GetSnapshotAsync();
                var snapshotEgress = await egresosQuery.GetSnapshotAsync();
                var ingresos = snapshotIngress.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                var egresos = snapshotEgress.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                var egresosPorAño = egresos.Where(registro => registro.FechaDeEgreso.Contains(filtro)).ToList();
                var ingresosPorAño = ingresos.Where(registro => registro.FechaDeIngreso.Contains(filtro)).ToList();
                sumIngreso = ingresosPorAño.Sum(doc => doc.Valor);
                sumEgreso = egresosPorAño.Sum(doc => doc.Valor);
                // Actualizar el texto después de procesar todos los documentos
                textTotalEgresos.Text = LecturaCifra(sumEgreso);
                saldo = sumEgreso > 0 ? sumIngreso - sumEgreso : 0;
                textSaldo.Text = LecturaCifra(saldo);
                // Calcular el saldo después de totalizar los ingresos
                if(textSaldo.Text!="" && textSaldo.Text != "0")
                {
                    //CalculoDeSaldo();
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.ConsultarTodos();
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    egresos = respuesta.Egresos.ToList();
                    for (int i = 0; i < egresos.Count; i++)
                    {
                        sumEgreso = sumEgreso + respuesta.Egresos[i].Valor;
                    }
                }
            }
        }
        //private async void TotalizarRegistros()
        //{
        //    ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
        //    respuesta = egresoService.ConsultarTodos();
        //    if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
        //    {
        //        dataGridEgresos.DataSource = null;
        //        egresos = respuesta.Egresos.ToList();
        //        if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
        //        {
        //            try
        //            {
        //                var egresos = new List<EgressData>();
        //                var db = FirebaseService.Database;
        //                var egresosQuery = db.Collection("EgressData");
        //                var snapshot = await egresosQuery.GetSnapshotAsync();
        //                textTotalNube.Text = snapshot.Documents.Count().ToString();
        //                textTotalLocal.Text = egresoService.Totalizar().Cuenta.ToString();
        //                // Obtener referencia al formulario principal
        //                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //                // Verificar si el formulario principal está abierto
        //                if (formPrincipal != null)
        //                {
        //                    // Lanzar el evento para notificar al formulario principal sobre la excepción
        //                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
        //                }
        //            }
        //            catch(Exception ex)
        //            {
        //                // Obtener referencia al formulario principal
        //                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //                // Verificar si el formulario principal está abierto
        //                if (formPrincipal != null)
        //                {
        //                    // Lanzar el evento para notificar al formulario principal sobre la excepción
        //                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
        //                }
        //                textTotalLocal.Text = egresoService.Totalizar().Cuenta.ToString();
        //                textTotalNube.Text = "0";
        //            }
        //        }
        //        else
        //        {
        //            textTotalLocal.Text = "0";
        //            textTotalNube.Text = "0";
        //        }
        //    }
        //}
        private void FormEgresos_Load(object sender, EventArgs e)
        {
            if (tabEgresos.TabCount > 0)
            {
                tabPage = tabEgresos.TabPages["tabDetalle"];
                tabEgresos.TabPages.RemoveAt(2);
            }
            filtro = comboFiltroAño.Text;
        }
        private async void CalcularComprobante()
        {
            try
            {
                var db = FirebaseService.Database;
                string numeroComprobanteFinal = "";

                // Obtener el máximo número de comprobante directamente desde Firestore
                var egressQuery = db.Collection("EgressData").OrderByDescending("CodigoComprobante").Limit(1);
                var snapshot = await egressQuery.GetSnapshotAsync();

                if (snapshot.Documents.Count > 0)
                {
                    var egressData = snapshot.Documents[0].ConvertTo<IngressData>();
                    int numeroMayor = int.Parse(egressData.CodigoComprobante) + 1;
                    numeroComprobanteFinal = numeroMayor.ToString("0000");
                }
                else
                {
                    numeroComprobanteFinal = "0001";
                }
                textNumeroComprobante.Text = numeroComprobanteFinal;
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.ConsultarTodos();
                egresos = respuesta.Egresos.ToList();
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    List<int> comprobantes = new List<int>();
                    string numeroComprobanteFinal = "";

                    for (int i = 0; i < respuesta.Egresos.Count; i++)
                    {
                        // Obtener el número del comprobante del egreso actual
                        int numeroComprobanteActual = int.Parse(respuesta.Egresos[i].CodigoComprobante);

                        // Agregarlo a la lista de comprobantes
                        comprobantes.Add(numeroComprobanteActual);
                    }

                    // Verificar si hay elementos en la lista de comprobantes
                    if (comprobantes.Count > 0)
                    {
                        // Obtener el número mayor de la lista
                        int numeroMayor = comprobantes.Max();
                        numeroMayor = numeroMayor + 1;
                        // Asignar el número mayor a la variable final
                        // Supongamos que tienes una variable final llamada 'numeroComprobanteFinal'
                        numeroComprobanteFinal = numeroMayor.ToString("0000");
                        textNumeroComprobante.Text = numeroComprobanteFinal;
                    }
                }
                else
                {
                    var totalFolio = 1;
                    comprobante = totalFolio.ToString("0000");
                    textNumeroComprobante.Text = comprobante;
                }
            }
        }
        private async void ConsultarEgresos()
        {
            //TotalizarRegistros();
            try
            {
                var db = FirebaseService.Database;
                var egresosQuery = db.Collection("EgressData");
                var egresos = new List<EgressData>();
                var egresosPorAñoActual = new List<EgressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await egresosQuery.GetSnapshotAsync();
                egresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                sumEgreso = snapshot.Documents.Sum(doc => doc.ConvertTo<EgressData>().Valor);
                if (egresos.Count > 0)
                {
                    for(int i = 0; i < egresos.Count; i++)
                    {
                        if (egresos[i].FechaDeEgreso.Contains(filtro))
                        {
                            egresosPorAñoActual.Add(egresos[i]);
                        }
                    }
                    // Actualizar el texto después de procesar todos los documentos
                    TotalizarEgresosYCalculoDeSaldo(filtro);
                    dataGridEgresos.DataSource = null;
                    dataGridEgresos.DataSource = egresosPorAñoActual;
                    textTotalNube.Text = egresosPorAñoActual.Count.ToString();
                }
                else
                {
                    dataGridEgresos.DataSource = null;
                    textTotalLocal.Text = "0";
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.ConsultarTodos();
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    dataGridDetalle.DataSource = null;
                    egresos = respuesta.Egresos.ToList();
                    dataGridDetalle.DataSource = respuesta.Egresos;
                    Borrar.Visible = true;
                    textTotalLocal.Text = egresoService.Totalizar().Cuenta.ToString();
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
        private async void FiltroPorId(string id)
        {
            try
            {
                var db = FirebaseService.Database;
                var egresosQuery = db.Collection("EgressData");
                var egresos = new List<EgressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await egresosQuery.GetSnapshotAsync();
                egresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var egresosFiltrados = egresos.Where(egreso =>egreso.CodigoComprobante == id).ToList();
                if (egresosFiltrados.Any())
                {
                    var egresoFiltrado = egresosFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    textNumeroComprobante.Text = egresoFiltrado.CodigoComprobante;
                    dateTimeEgreso.Value = DateTime.Parse(FormatearFecha(egresoFiltrado.FechaDeEgreso));
                    comboComite.Text = egresoFiltrado.Comite;
                    comboConcepto.Text = egresoFiltrado.Concepto;
                    textDineroIngreso.Text = egresoFiltrado.Valor.ToString();
                    textDetalle.Text = egresoFiltrado.Detalle;
                    tabEgresos.SelectedIndex = 1;
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
                BusquedaEgresoRespuesta respuesta = new BusquedaEgresoRespuesta();
                respuesta = egresoService.BuscarPorIdentificacion(id);
                var registro = respuesta.Egreso;
                if (registro != null)
                {
                    encontrado = true;
                    var egresos = new List<Egreso> { registro };
                    textNumeroComprobante.Text = egresos[0].CodigoComprobante;
                    dateTimeEgreso.Value = egresos[0].FechaDeEgreso;
                    comboComite.Text = egresos[0].Comite;
                    comboConcepto.Text = egresos[0].Concepto;
                    textDineroIngreso.Text = egresos[0].Valor.ToString();
                    textDetalle.Text = egresos[0].Detalle;
                }
            }
        }
        private async void FiltroPorComite(string comite)
        {
            try
            {
                var db = FirebaseService.Database;
                var egresosQuery = db.Collection("EgressData");
                var egresos = new List<EgressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await egresosQuery.GetSnapshotAsync();
                egresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var egresosComite = egresos.Where(egreso => egreso.Comite == comite).ToList();
                var egresosPorAñoComite = egresosComite.Where(egreso => egreso.FechaDeEgreso.Contains(filtro)).ToList();
                dataGridEgresos.DataSource = egresosPorAñoComite;
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.FiltrarEgresosPorComite(comite);
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    dataGridEgresos.DataSource = null;
                    egresos = respuesta.Egresos.ToList();
                    if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                    {
                        dataGridEgresos.DataSource = respuesta.Egresos;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridEgresos.DataSource = null;
                    egresos = respuesta.Egresos.ToList();
                }
            }
        }
        private async void FiltrarEgresosPorComite(string comite)
        {
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var egressQuery = db.Collection("EgressData").WhereEqualTo("Comite", comite);
                QuerySnapshot snap = await egressQuery.GetSnapshotAsync();

                var egressFilterData = snap.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                sumTotal = egressFilterData.Sum(egreso => egreso.Valor);

                textTotalComite.Text = egressFilterData.Count.ToString();
                textValorConcepto.Text = LecturaCifra(sumTotal);

                dataGridDetalle.DataSource = null;
                dataGridDetalle.DataSource = egressFilterData;
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.FiltrarEgresosPorComite(comite);
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    dataGridDetalle.DataSource = null;
                    egresos = respuesta.Egresos.ToList();
                    sumTotal = 0;
                    dataGridDetalle.DataSource = respuesta.Egresos;
                    Borrar.Visible = true;
                    textTotalComite.Text = egresoService.TotalizarTipo(comite).Cuenta.ToString();
                    for (int i = 0; i < respuesta.Egresos.Count; i++)
                    {
                        sumTotal = sumTotal + respuesta.Egresos[i].Valor;
                        textValorConcepto.Text = sumTotal.ToString();
                    }
                }
                else
                {
                    textTotalComite.Text = "0";
                }
            }
        }
        private async void FiltrarPorConcepto(string concepto)
        {
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var egressQuery = db.Collection("EgressData").WhereEqualTo("Comite", comite).WhereEqualTo("Concepto", concepto);
                QuerySnapshot snap = await egressQuery.GetSnapshotAsync();

                var egresos = snap.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                sumTotal = egresos.Sum(egreso => egreso.Valor);

                textValorConcepto.Text = LecturaCifra(sumTotal);
                dataGridDetalle.DataSource = egresos;
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
                ConsultaEgresoRespuesta respuesta = new ConsultaEgresoRespuesta();
                respuesta = egresoService.FiltrarEgresosPorConcepto(concepto);
                if (respuesta.Egresos.Count != 0 && respuesta.Egresos != null)
                {
                    var conceptos = new List<Egreso>();
                    sumTotal = 0;
                    dataGridDetalle.DataSource = null;
                    egresos = respuesta.Egresos.ToList();
                    for (int i = 0; i < egresos.Count; i++)
                    {
                        if (egresos[i].Comite == comite)
                        {
                            conceptos.Add(egresos[i]);
                            sumTotal = sumTotal + egresos[i].Valor;
                        }
                    }
                    textValorConcepto.Text = sumTotal.ToString();
                    dataGridDetalle.DataSource = conceptos;
                    Borrar.Visible = true;
                }
                else
                {
                    textValorConcepto.Text = "0";
                    dataGridDetalle.DataSource = null;
                }
            }
        }
        private void dataGridEgresos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridEgresos.DataSource != null)
            {
                if (dataGridEgresos.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridEgresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                    EliminarIngreso(id);
                    ConsultarEgresos();
                }
                else
                {
                    if (dataGridEgresos.Columns[e.ColumnIndex].Name == "Detallar")
                    {
                        comite = Convert.ToString(dataGridEgresos.CurrentRow.Cells["Comite"].Value.ToString());
                        FiltrarEgresosPorComite(comite);
                        tabEgresos.TabPages.Add(tabPage);
                        tabEgresos.SelectedIndex = 2;
                        detallo = true;
                    }
                    else
                    {
                        if (dataGridEgresos.Columns[e.ColumnIndex].Name == "Editar")
                        {
                            id = Convert.ToString(dataGridEgresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                            FiltroPorId(id);
                            btnRegistrar.Enabled = false;
                        }
                    }
                }
            }
        }
        private void dataGridDetalle_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridDetalle.DataSource != null)
            {
                if (dataGridDetalle.Columns[e.ColumnIndex].Name == "Borrar2")
                {
                    id = Convert.ToString(dataGridDetalle.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                    EliminarIngreso(id);
                    ConsultarEgresos();
                    tabEgresos.SelectedIndex = 0;
                }
                else
                {
                    if (dataGridDetalle.Columns[e.ColumnIndex].Name == "Editar2")
                    {
                        id = Convert.ToString(dataGridDetalle.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                        FiltroPorId(id);
                        if (encontrado == true)
                        {
                            tabEgresos.SelectedIndex = 1;
                        }
                    }
                }
            }
        }
        private void comboConcepto_Enter(object sender, EventArgs e)
        {
            if (comboConcepto.Text == "Concepto")
            {
                comboConcepto.Text = "";
            }
        }

        private void comboConcepto_Leave(object sender, EventArgs e)
        {
            if (comboConcepto.Text == "")
            {
                comboConcepto.Text = "Concepto";
            }
        }

        private void textDineroIngreso_Enter(object sender, EventArgs e)
        {
            originalText = textDineroIngreso.Text;
            if (textDineroIngreso.Text == "$ 000.00")
            {
                textDineroIngreso.Text = "";
            }
            else
            {

                if (textDineroIngreso.Text.StartsWith("$ "))
                {
                    // Borra el contenido del TextBox si comienza con "$ "
                    textDineroIngreso.Text = "";
                }
            }
        }

        private void textDineroIngreso_Leave(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text == "")
            {
                textDineroIngreso.Text = "$ 000.00";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textDineroIngreso.Text))
                {
                    // Restaura el contenido original al salir del TextBox
                    textDineroIngreso.Text = originalText;
                }
            }
        }

        private void textDetalle_Enter(object sender, EventArgs e)
        {
            if (textDetalle.Text == "Detalle")
            {
                textDetalle.Text = "";
            }
        }

        private void textDetalle_Leave(object sender, EventArgs e)
        {
            if (textDetalle.Text == "")
            {
                textDetalle.Text = "Detalle";
            }
        }

        private void textDineroIngreso_TextChanged(object sender, EventArgs e)
        {

        }

        private void textDineroIngreso_Validated(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text != "" && textDineroIngreso.Text != "$ 000.00")
            {
                int sumEgreso = int.Parse(textDineroIngreso.Text);
                textDineroIngreso.Text = LecturaCifra(sumEgreso);
            }
        }

        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            textSerachLibreta.Visible = true;
            btSearchLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = true;
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            textSerachLibreta.Visible = false;
            btSearchLibreta.Visible = true;
            btnCloseSearchLibreta.Visible = false;
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por fecha";
            }
        }

        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "Buscar por fecha")
            {
                textSerachLibreta.Text = "";
            }
        }

        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por fecha";
            }
        }
        private Egreso MapearEgreso()
        {
            egreso = new Egreso();
            egreso.CodigoComprobante = textNumeroComprobante.Text;
            egreso.FechaDeEgreso = dateTimeEgreso.Value;
            egreso.Comite = comboComite.Text;
            egreso.Concepto = comboConcepto.Text;
            string cantidadConSigno = textDineroIngreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos); // Convierte el valor a un entero
            egreso.Valor = cantidadEntera;
            egreso.Detalle = textDetalle.Text;
            return egreso;
        }
        private void Limpiar()
        {
            int seguimiento = int.Parse(textNumeroComprobante.Text);
            textNumeroComprobante.Text = (seguimiento + 1).ToString();
            CalcularComprobante();
            dateTimeEgreso.Value = DateTime.Now;
            comboComite.Text = "Comite";
            comboConcepto.Text = "Concepto";
            textDineroIngreso.Text = "$ 000.00";
            textDetalle.Text = "Detalle";
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            Egreso egreso = MapearEgreso();
            try
            {
                var msg = egresoService.Guardar(egreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var egress = egressMaps.EgressMap(egreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("EgressData").Document(egress.CodigoComprobante);
                docRef.SetAsync(egress);
                // Guardamos localmente
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresos();
                TotalizarEgresosYCalculoDeSaldo(filtro);
                //CalculoDeSaldo();
                Limpiar();
                tabEgresos.SelectedIndex = 0;
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
                    var msg = egresoService.Guardar(egreso);
                    MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarEgresos();
                    Limpiar();
                    tabEgresos.SelectedIndex = 0;
                }
            }
        }
        private void EliminarIngreso(string codigo)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("EgressData").Document(codigo);
                docRef.DeleteAsync();
                //string mensaje = egresoService.Eliminar(codigo);
                MessageBox.Show("Se ha eliminado satisfactoriamente el registro", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresos();
                TotalizarEgresosYCalculoDeSaldo(filtro);
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
                string mensaje = egresoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEgresos();
            }
        }

        private void tabEgresos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabEgresos.SelectedIndex != 2 && detallo != false)
            {
                tabPage = tabEgresos.TabPages["tabDetalle"];
                tabEgresos.TabPages.RemoveAt(2);
                detallo = false;
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Egreso nuevoEgreso = MapearEgreso();
            try
            {
                nuevoEgreso.CodigoComprobante = id;
                //string mensaje = egresoService.Modificar(nuevoEgreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var ingress = egressMaps.EgressMap(nuevoEgreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("EgressData").Document(ingress.CodigoComprobante.ToString());
                docRef.SetAsync(ingress);
                MessageBox.Show("Se ha modificado satisfactoriamente el registro", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarEgresos();
                TotalizarEgresosYCalculoDeSaldo(filtro);
                Limpiar();
                tabEgresos.SelectedIndex = 0;
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
                string mensaje = egresoService.Modificar(nuevoEgreso);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarEgresos();
                Limpiar();
                tabEgresos.SelectedIndex = 0;
            }
        }

        private void comboFiltroComite_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroComite.Text;
            if (filtro == "Todos" || filtro == "Comite")
            {
                ConsultarEgresos();
            }
            else
            {
                if(comboFiltroAño.Text != "2024")
                {
                    comboFiltroAño.Text = "2024";
                }
                FiltroPorComite(filtro);
            }
        }

        private void comboConceptoDetalle_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboConceptoDetalle.Text;
            if (filtro == "Ninguno")
            {
                FiltrarEgresosPorComite(comite);
            }
            else
            {
                FiltrarPorConcepto(filtro);
            }
        }

        private void btnImprimirDetalle_Click(object sender, EventArgs e)
        {

        }

        private void btnImprimirDetalle_Click_1(object sender, EventArgs e)
        {

        }
        private async void FiltroPorAño(string filtro)
        {
            try
            {
                var egresosPorAño = new List<EgressData>();
                var db = FirebaseService.Database;
                var presupuestosQuery = db.Collection("EgressData");
                egress = new List<EgressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await presupuestosQuery.GetSnapshotAsync();
                egress = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<EgressData>()).ToList();
                for (int i = 0; i < egress.Count; i++)
                {
                    if (egress[i].FechaDeEgreso.Contains(filtro))
                    {
                        egresosPorAño.Add(egress[i]);
                    }
                }
                dataGridEgresos.DataSource = egresosPorAño;
                TotalizarEgresosYCalculoDeSaldo(filtro);
                textTotalNube.Text = egresosPorAño.Count.ToString();
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
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }

        private void comboFiltroAño_SelectedIndexChanged(object sender, EventArgs e)
        {
            filtro = comboFiltroAño.Text;
            if (filtro == "2024" && filtro=="")
            {
                ConsultarEgresos();
            }
            else
            {
                if (comboFiltroComite.Text != "Comite" || comboFiltroComite.Text != "Todos")
                {
                    comboFiltroComite.Text = "Comite";
                }
                FiltroPorAño(filtro);
            }
        }
    }
}

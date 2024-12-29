using BLL;
using Cloud;
using Entity;
using Google.Cloud.Firestore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using Ldb;

namespace UI
{
    public partial class FormIngresos : Form
    {
        Ingreso ingreso;
        IngresoService ingresoService;
        List<Ingreso> ingresos;
        List<IngressData> ingress;
        Egreso egreso;
        EgresoService egresoService;
        List<Egreso> egresos;
        IngressMaps ingressMaps;
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
        public FormIngresos()
        {
            ingresoService = new IngresoService(DbConfig.ConnectionString);
            ingressMaps = new IngressMaps();
            InitializeComponent();
            ConsultarIngresos();
            CalcularComprobante();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGestionarIngresos_Click_1(object sender, EventArgs e)
        {
            tabLibroIngresos.SelectedIndex = 1;
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
        //        textSaldo.Text=(sumIngreso-sumEgreso).ToString();
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
        //    }
        //}
        private async void TotalizarIngresosYCalculoDeSaldo(string filtro)
        {
            try
            {
                sumIngreso = 0;
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
                textTotalIngresos.Text = LecturaCifra(sumIngreso);
                saldo = sumIngreso > 0 ? sumIngreso - sumEgreso : 0;
                textSaldo.Text = LecturaCifra(saldo);

                if (textSaldo.Text != "" && textSaldo.Text != "0")
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.ConsultarTodos();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    ingresos = respuesta.Ingresos.ToList();
                    for(int i=0; i< ingresos.Count; i++)
                    {
                        sumIngreso = sumIngreso + respuesta.Ingresos[i].Valor;
                    }
                }
            }
        }
        //private async void TotalizarRegistros()
        //{
        //    ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
        //    respuesta = ingresoService.ConsultarTodos();
        //    if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
        //    {
        //        dataGridIngresos.DataSource = null;
        //        ingresos = respuesta.Ingresos.ToList();
        //        if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
        //        {
        //            try
        //            {
        //                var ingresos = new List<IngressData>();
        //                var db = FirebaseService.Database;
        //                var ingresosQuery = db.Collection("IngressData");
        //                var snapshot = await ingresosQuery.GetSnapshotAsync();
        //                textTotalNube.Text=snapshot.Documents.Count().ToString();
        //                textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
        //                // Obtener referencia al formulario principal
        //                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //                // Verificar si el formulario principal está abierto
        //                if (formPrincipal != null)
        //                {
        //                    // Lanzar el evento para notificar al formulario principal sobre la excepción
        //                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Obtener referencia al formulario principal
        //                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
        //                // Verificar si el formulario principal está abierto
        //                if (formPrincipal != null)
        //                {
        //                    // Lanzar el evento para notificar al formulario principal sobre la excepción
        //                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
        //                }
        //                textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
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
        private void FormIngresos_Load(object sender, EventArgs e)
        {
            if (tabLibroIngresos.TabCount > 0)
            {
                tabPage = tabLibroIngresos.TabPages["tabDetalle"];
                tabLibroIngresos.TabPages.RemoveAt(2);
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
                var ingressQuery = db.Collection("IngressData").OrderByDescending("CodigoComprobante").Limit(1);
                var snapshot = await ingressQuery.GetSnapshotAsync();

                if (snapshot.Documents.Count > 0)
                {
                    var ingressData = snapshot.Documents[0].ConvertTo<IngressData>();
                    int numeroMayor = int.Parse(ingressData.CodigoComprobante) + 1;
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.ConsultarTodos();
                ingresos = respuesta.Ingresos.ToList();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    List<int> comprobantes = new List<int>();
                    string numeroComprobanteFinal = "";
                    for (int i = 0; i < respuesta.Ingresos.Count; i++)
                    {
                        // Obtener el número del comprobante del ingreso actual
                        int numeroComprobanteActual = int.Parse(respuesta.Ingresos[i].CodigoComprobante);

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
        private async void ConsultarIngresos()
        {
            //TotalizarRegistros();
            try
            {
                var db = FirebaseService.Database;
                var ingresosQuery = db.Collection("IngressData");
                var ingresos = new List<IngressData>();
                var ingresosPorAñoActual = new List<IngressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await ingresosQuery.GetSnapshotAsync();
                ingresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                if (ingresos.Count > 0)
                {
                    for (int i = 0; i < ingresos.Count; i++)
                    {
                        if (ingresos[i].FechaDeIngreso.Contains(filtro))
                        {
                            ingresosPorAñoActual.Add(ingresos[i]);
                        }
                    }
                    TotalizarIngresosYCalculoDeSaldo(filtro);
                    dataGridIngresos.DataSource = null;
                    dataGridIngresos.DataSource = ingresosPorAñoActual;
                    textTotalNube.Text = ingresosPorAñoActual.Count.ToString();
                }
                else
                {
                    dataGridIngresos.DataSource = null;
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.ConsultarTodos();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    dataGridIngresos.DataSource = null;
                    ingresos = respuesta.Ingresos.ToList();
                    dataGridIngresos.DataSource = respuesta.Ingresos;
                    Borrar.Visible = true;
                    textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
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
                var ingresosQuery = db.Collection("IngressData");
                var ingresos = new List<IngressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await ingresosQuery.GetSnapshotAsync();
                ingresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var ingresosFiltrados = ingresos.Where(ingreso => ingreso.CodigoComprobante == id).ToList();
                if (ingresosFiltrados.Any())
                {
                    var ingresoFiltrado = ingresosFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    textNumeroComprobante.Text = ingresoFiltrado.CodigoComprobante;
                    dateFechaIngreso.Value = DateTime.Parse(FormatearFecha(ingresoFiltrado.FechaDeIngreso));
                    comboComite.Text = ingresoFiltrado.Comite;
                    comboConcepto.Text = ingresoFiltrado.Concepto;
                    textDineroIngreso.Text = ingresoFiltrado.Valor.ToString();
                    textDetalle.Text = ingresoFiltrado.Detalle;
                    tabLibroIngresos.SelectedIndex = 1;
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
                BusquedaIngresoRespuesta respuesta = new BusquedaIngresoRespuesta();
                respuesta = ingresoService.BuscarPorIdentificacion(id);
                var registro = respuesta.Ingreso;
                if (registro != null)
                {
                    encontrado = true;
                    var ingresos = new List<Ingreso> { registro };
                    textNumeroComprobante.Text = ingresos[0].CodigoComprobante;
                    dateFechaIngreso.Value = ingresos[0].FechaDeIngreso;
                    comboComite.Text = ingresos[0].Comite;
                    comboConcepto.Text = ingresos[0].Concepto;
                    textDineroIngreso.Text = ingresos[0].Valor.ToString();
                    textDetalle.Text = ingresos[0].Detalle;
                }
            }
        }
        private async void FiltroPorComite(string comite)
        {
            try
            {
                var db = FirebaseService.Database;
                var ingresosQuery = db.Collection("IngressData");
                var ingresos = new List<IngressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await ingresosQuery.GetSnapshotAsync();
                ingresos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var ingresosComite = ingresos.Where(ingreso => ingreso.Comite == comite).ToList();
                var ingresosPorAñoComite = ingresosComite.Where(ingreso => ingreso.FechaDeIngreso.Contains(filtro)).ToList();
                dataGridIngresos.DataSource = ingresosPorAñoComite;
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.FiltrarIngresosPorComite(comite);
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    dataGridIngresos.DataSource = null;
                    ingresos = respuesta.Ingresos.ToList();
                    if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                    {
                        dataGridIngresos.DataSource = respuesta.Ingresos;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridIngresos.DataSource = null;
                    ingresos = respuesta.Ingresos.ToList();
                }
            }
        }
        private async void FiltrarIngresosPorComite(string comite)
        {
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var ingressQuery = db.Collection("IngressData").WhereEqualTo("Comite", comite);
                QuerySnapshot snap = await ingressQuery.GetSnapshotAsync();

                var ingressFilterData = snap.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                sumTotal = ingressFilterData.Sum(ingreso => ingreso.Valor);

                textTotalComite.Text = ingressFilterData.Count.ToString();
                textValorConcepto.Text = LecturaCifra(sumTotal);

                dataGridDetalle.DataSource = null;
                dataGridDetalle.DataSource = ingressFilterData;
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.FiltrarIngresosPorComite(comite);
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    dataGridDetalle.DataSource = null;
                    sumTotal = 0;
                    ingresos = respuesta.Ingresos.ToList();
                    dataGridDetalle.DataSource = respuesta.Ingresos;
                    Borrar.Visible = true;
                    textTotalComite.Text = ingresoService.TotalizarTipo(comite).Cuenta.ToString();
                    for (int i = 0; i < respuesta.Ingresos.Count; i++)
                    {
                        sumTotal = sumTotal + respuesta.Ingresos[i].Valor;
                        textValorConcepto.Text = LecturaCifra(sumTotal);
                    }
                }
            }
        }
        private async void FiltrarPorConcepto(string concepto)
        {
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var ingressQuery = db.Collection("IngressData").WhereEqualTo("Comite", comite).WhereEqualTo("Concepto", concepto);
                QuerySnapshot snap = await ingressQuery.GetSnapshotAsync();

                var ingresos = snap.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                sumTotal = ingresos.Sum(ingreso => ingreso.Valor);

                textValorConcepto.Text = LecturaCifra(sumTotal);
                dataGridDetalle.DataSource = ingresos;
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
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.FiltrarIngresosPorConcepto(concepto);
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    var conceptos = new List<Ingreso>();
                    sumTotal = 0;
                    dataGridDetalle.DataSource = null;
                    ingresos = respuesta.Ingresos.ToList();
                    for (int i = 0; i < ingresos.Count; i++)
                    {
                        if (ingresos[i].Comite == comite)
                        {
                            conceptos.Add(ingresos[i]);
                            sumTotal = sumTotal + ingresos[i].Valor;
                        }
                    }
                    textValorConcepto.Text = LecturaCifra(sumTotal);
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
        private void dataGridIngresos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridIngresos.DataSource != null)
            {
                if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridIngresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                    EliminarIngreso(id);
                    ConsultarIngresos();
                }
                else
                {
                    if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Detallar")
                    {
                        comite = Convert.ToString(dataGridIngresos.CurrentRow.Cells["Comite"].Value.ToString());
                        FiltrarIngresosPorComite(comite);
                        tabLibroIngresos.TabPages.Add(tabPage);
                        tabLibroIngresos.SelectedIndex = 2;
                        detallo = true;
                    }
                    else
                    {
                        if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Editar")
                        {
                            id = Convert.ToString(dataGridIngresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                            FiltroPorId(id);
                            tabLibroIngresos.SelectedIndex = 1;
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
                    ConsultarIngresos();
                    tabLibroIngresos.SelectedIndex = 0;
                }
                else
                {
                    if (dataGridDetalle.Columns[e.ColumnIndex].Name == "Editar2")
                    {
                        id = Convert.ToString(dataGridDetalle.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                        FiltroPorId(id);
                        if (encontrado == true)
                        {
                            tabLibroIngresos.SelectedIndex = 1;
                        }
                    }
                }
            }
        }
        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if(textSerachLibreta.Text== "Buscar por detalle")
            {
                textSerachLibreta.Text = "";
            }
        }
        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por detalle";
            }
        }
        private void textDineroIngreo_Enter(object sender, EventArgs e)
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
        private void textDineroIngreo_Leave(object sender, EventArgs e)
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
        private void comboComite_Enter(object sender, EventArgs e)
        {
            if (comboComite.Text == "Comite")
            {
                comboComite.Text = "";
            }
        }
        private void comboComite_Leave(object sender, EventArgs e)
        {
            if (comboComite.Text == "")
            {
                comboComite.Text = "Comite";
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
        private void textDineroIngreso_Validated(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text != "" && textDineroIngreso.Text != "$ 000.00")
            {
                int sumIngreso = int.Parse(textDineroIngreso.Text);
                textDineroIngreso.Text = LecturaCifra(sumIngreso);
            }
        }
        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            if (textSerachLibreta.Visible == false)
            {
                textSerachLibreta.Visible = true;
                btSearchLibreta.Visible = false;
                btnCloseSearchLibreta.Visible = true;
            }
        }
        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            if (textSerachLibreta.Visible == true)
            {
                textSerachLibreta.Visible = false;
                btSearchLibreta.Visible = true;
                btnCloseSearchLibreta.Visible = false;
            }
        }
        private Ingreso MapearIngreso()
        {
            ingreso = new Ingreso();
            ingreso.CodigoComprobante = textNumeroComprobante.Text;
            ingreso.FechaDeIngreso = dateFechaIngreso.Value;
            ingreso.Comite = comboComite.Text;
            ingreso.Concepto = comboConcepto.Text;
            string cantidadConSigno = textDineroIngreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos); // Convierte el valor a un entero
            ingreso.Valor = cantidadEntera;
            ingreso.Detalle = textDetalle.Text;
            return ingreso;
        }
        private void Limpiar()
        {
            int seguimiento =int.Parse(textNumeroComprobante.Text );
            textNumeroComprobante.Text = (seguimiento+1).ToString();
            CalcularComprobante();
            dateFechaIngreso.Value = DateTime.Now;
            comboComite.Text = "Comite";
            comboConcepto.Text = "Concepto";
            textDineroIngreso.Text = "$ 000.00";
            textDetalle.Text="Detalle";
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            Ingreso ingreso = MapearIngreso();
            try
            {
                var msg = ingresoService.Guardar(ingreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var ingress = ingressMaps.IngressMap(ingreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(ingress.CodigoComprobante.ToString());
                docRef.SetAsync(ingress);
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
                TotalizarIngresosYCalculoDeSaldo(filtro);
                //CalculoDeSaldo();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
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
                    var msg = ingresoService.Guardar(ingreso);
                    MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarIngresos();
                    Limpiar();
                    tabLibroIngresos.SelectedIndex = 0;
                }
            }
        }
        private void EliminarIngreso(string codigo)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(codigo);
                docRef.DeleteAsync();
                //string mensaje = ingresoService.Eliminar(codigo);
                MessageBox.Show("Se ha eliminado satisfactoriamente el registro", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
                TotalizarIngresosYCalculoDeSaldo(filtro);
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
                string mensaje = ingresoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
            }
        }

        private void tabLibroIngresos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabLibroIngresos.SelectedIndex != 2 && detallo!=false)
            {
                tabPage = tabLibroIngresos.TabPages["tabDetalle"];
                tabLibroIngresos.TabPages.RemoveAt(2);
                detallo = false;
            }
        }
        private void btnModificar_Click(object sender, EventArgs e)
        {
            Ingreso nuevoIngreso = MapearIngreso();
            try
            {
                nuevoIngreso.CodigoComprobante = id;
                //string mensaje = ingresoService.Modificar(nuevoIngreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var ingress = ingressMaps.IngressMap(nuevoIngreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(ingress.CodigoComprobante.ToString());
                docRef.SetAsync(ingress);
                MessageBox.Show("Se ha modificado satisfactoriamente el registro", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarIngresos();
                TotalizarIngresosYCalculoDeSaldo(filtro);
                //CalculoDeSaldo();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
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
                string mensaje = ingresoService.Modificar(nuevoIngreso);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarIngresos();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
            }
        }

        private void comboFiltroComite_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroComite.Text;
            if (filtro == "Todos")
            {
                ConsultarIngresos();
            }
            else
            {
                if (comboFiltroAño.Text != "2024")
                {
                    comboFiltroAño.Text = "2024";
                    FiltroPorComite(filtro);
                }
            }
        }

        private void comboConceptoDetalle_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboConceptoDetalle.Text;
            if (filtro == "Ninguno")
            {
                FiltrarIngresosPorComite(comite);
            }
            else
            {
                FiltrarPorConcepto(filtro);
            }
        }

        private void btnImprimirDetalle_Click(object sender, EventArgs e)
        {
            // Ruta relativa del archivo .docx que quieres editar
            string filePath = "Doc's\\Tesoreria\\InformeIndividualTesoreria.docx";

            try
            {
                // Obtén la ruta completa del archivo dentro del directorio del proyecto
                string directorioProyecto = AppDomain.CurrentDomain.BaseDirectory;
                string rutaCompleta = Path.Combine(directorioProyecto, filePath);

                using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaCompleta, true))
                {
                    // Obtén el cuerpo del documento
                    Body body = doc.MainDocumentPart.Document.Body;

                    // Busca la posición donde agregar la tabla (puedes ajustar esto según tu necesidad)
                    Paragraph ingresosParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("INGRESOS"));

                    if (ingresosParagraph != null)
                    {
                        // Crea una nueva tabla con 2 columnas y 5 filas
                        Table nuevaTabla = new Table(
                            new TableProperties(
                                new TableLayout { Type = TableLayoutValues.Fixed },
                                new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto }
                            )
                        );

                        for (int i = 0; i < 5; i++)
                        {
                            TableRow fila = new TableRow();
                            fila.Append(
                                new TableCell(new Paragraph(new Run(new Text($"Celda {i + 1}")))),
                                new TableCell(new Paragraph(new Run(new Text($"Otra Celda {i + 1}"))))
                            );
                            nuevaTabla.Append(fila);
                        }

                        // Inserta la tabla después del párrafo de "INGRESOS"
                        body.InsertAfter(nuevaTabla, ingresosParagraph);
                    }

                    // Guarda los cambios en el documento
                    doc.MainDocumentPart.Document.Save();
                }

                MessageBox.Show("Tabla agregada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Error al editar el documento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void FiltroPorAño(string filtro)
        {
            try
            {
                var ingresosPorAño = new List<IngressData>();
                var db = FirebaseService.Database;
                var ingresosQuery = db.Collection("IngressData");
                ingress = new List<IngressData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await ingresosQuery.GetSnapshotAsync();
                ingress = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<IngressData>()).ToList();
                for(int i = 0; i < ingress.Count; i++)
                {
                    if (ingress[i].FechaDeIngreso.Contains(filtro))
                    {
                        ingresosPorAño.Add(ingress[i]);
                    }
                }
                dataGridIngresos.DataSource = ingresosPorAño;
                TotalizarIngresosYCalculoDeSaldo(filtro);
                textTotalNube.Text = ingresosPorAño.Count.ToString();
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
            string filtro = comboFiltroAño.Text;
            if (filtro == "2024" || filtro == "")
            {
                ConsultarIngresos();
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

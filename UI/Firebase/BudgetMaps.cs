﻿using Cloud;
using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class BudgetMaps
    {
        BudgetData budgetData;
        public BudgetData BudgetMap(Presupuesto presupuesto)
        {
            if (CheckIfBudgetAlreadyExist(presupuesto.Id.ToString()))
            {
                return new BudgetData()
                {
                    Id = presupuesto.Id.ToString().Trim(),
                    AñoPresupuesto = presupuesto.AñoPresupuesto.ToString(),
                    InicioIntervalo = presupuesto.InicioIntervalo,
                    FinIntervalo = presupuesto.FinIntervalo,
                    Comite = presupuesto.Comite,
                    Ofrenda = presupuesto.Ofrenda,
                    Actividad = presupuesto.Actividad,
                    Voto = presupuesto.Voto,
                    OtroConcepto = presupuesto.OtroConcepto,
                    ValorOtroConcepto = presupuesto.ValorOtroConcepto,
                    TotalEgresos =presupuesto.TotalEgresos,
                    TotalPresupuesto=presupuesto.TotalPresupuesto
                };
            }
            else
            {
                return new BudgetData()
                {
                    Id = presupuesto.Id.ToString().Trim(),
                    AñoPresupuesto = presupuesto.AñoPresupuesto.ToString(),
                    InicioIntervalo = presupuesto.InicioIntervalo,
                    FinIntervalo = presupuesto.FinIntervalo,
                    Comite = presupuesto.Comite,
                    Ofrenda = presupuesto.Ofrenda,
                    Actividad = presupuesto.Actividad,
                    Voto = presupuesto.Voto,
                    OtroConcepto = presupuesto.OtroConcepto,
                    ValorOtroConcepto = presupuesto.ValorOtroConcepto,
                    TotalEgresos = presupuesto.TotalEgresos,
                    TotalPresupuesto = presupuesto.TotalPresupuesto
                };
            }
        }
        private bool CheckIfBudgetAlreadyExist(string comprobante)
        {
            //Consulta en la nube
            var db = FirebaseService.Database;
            Google.Cloud.Firestore.DocumentReference docRef = db.Collection("BudgetData").Document(comprobante);
            BudgetData data = docRef.GetSnapshotAsync().Result.ConvertTo<BudgetData>();
            if (data != null)
            {
                return true;
            }
            return false;
        }
    }
}
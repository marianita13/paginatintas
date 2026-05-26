using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IDetalleFormula DetalleFormulas {get;}
        IEmpresa Empresas {get;}
        IFormula Formulas {get;}
        ILogotipoColor LogotipoColors {get;}
        ILogotipo Logotipos {get;}
        IOrdenImpresion OrdenImpresions {get;}
        ITintaBase TintaBases {get;}
        IUsuario Usuarios {get;}
        IRol Roles {get;}
        Task<int> SaveAsync();
    }
}
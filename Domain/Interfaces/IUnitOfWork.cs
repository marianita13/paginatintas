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
        IOrdenImpresion OrdenImpresions {get;}
        ITintaBase TintaBases {get;}
        IUsuario Usuarios {get;}
        IRol Roles {get;}
        IInventarioTinta InventarioTintas {get;}
        IOrdenFormula OrdenFormulas {get;}
        Task<int> SaveAsync();
    }
}
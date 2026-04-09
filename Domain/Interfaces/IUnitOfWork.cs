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
        ILogotipo Logotipos {get;}
        IordenImpresion Rols {get;}
        ITintaBase TintaBases {get;}
        IUsuario Users {get;}
        IRol Roles {get;}
        Task<int> SaveAsync();
    }
}
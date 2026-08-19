using System;
using Domain.Interfaces;
using Persistence.Data;
using Application.Repository;

namespace Application.UnitOfWork;
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly paginatintasContext _context;

    private IDetalleFormula _DetalleFormula;
    private IEmpresa _Empresa;
    private IFormula _Formula;
    private IOrdenImpresion _OrdenImpresion;
    private ITintaBase _TintaBase;
    private IUsuario _Usuario;
    private IRol _Rol;
    private IInventarioTinta _InventarioTinta;
    private IOrdenFormula _OrdenFormula;

    public UnitOfWork(paginatintasContext context)
    {
        _context = context;
    }

    public IDetalleFormula DetalleFormulas{
        get {
            if (_DetalleFormula == null) {
                _DetalleFormula = new DetalleFormulaRepository(_context);
            }
            return _DetalleFormula;
        }
    }

    public IEmpresa Empresas {
        get {
            if (_Empresa == null) {
                _Empresa = new EmpresaRepository(_context);
            }
            return _Empresa;
        }
    }

    public IFormula Formulas {
        get {
            if (_Formula == null) {
                _Formula = new FormulaRepository(_context);
            }
            return _Formula;
        }
    }

    public IOrdenImpresion OrdenImpresions{
        get {
            if (_OrdenImpresion == null) {
                _OrdenImpresion = new OrdenImpresionRepository(_context);
            }
            return _OrdenImpresion;
        }
    }

    public ITintaBase TintaBases{
        get {
            if (_TintaBase == null) {
                _TintaBase = new TintaBaseRepository(_context);
            }
            return _TintaBase;
        }
    }

    public IUsuario Usuarios {
        get {
            if (_Usuario == null) {
                _Usuario = new UsuarioRepository(_context);
            }
            return _Usuario;
        }
    }

    public IRol Roles {
        get {
            if (_Rol == null) {
                _Rol = new RolRepository(_context);
            }
            return _Rol;
        }
    }

    public IInventarioTinta InventarioTintas {
        get {
            if (_InventarioTinta == null) {
                _InventarioTinta = new InventarioTintaRepository(_context);
            }
            return _InventarioTinta;
        }
    }

    public IOrdenFormula OrdenFormulas {
        get {
            if (_OrdenFormula == null) {
                _OrdenFormula = new OrdenFormulaRepository(_context);
            }
            return _OrdenFormula;
        }
    }


    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
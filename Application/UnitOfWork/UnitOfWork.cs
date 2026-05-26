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
    private ILogotipo _Logotipo;
    private ILogotipoColor _LogotipoColor;
    private IOrdenImpresion _OrdenImpresion;
    private ITintaBase _TintaBase;
    private IUsuario _Usuario;
    private IRol _Rol;
    private ILogotipoColor _LogotipoColors;

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

    public ILogotipo Logotipos {
        get {
            if (_Logotipo == null) {
                _Logotipo = new LogotipoRepository(_context);
            }
            return _Logotipo;
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

    public ILogotipoColor LogotipoColors {
        get {
            if (_LogotipoColor == null) {
                _LogotipoColor = new LogotipoColorRepository(_context);
            }
            return _LogotipoColor;
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
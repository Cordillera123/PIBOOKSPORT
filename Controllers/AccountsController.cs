using IntegradorP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

public class AccountsController : Controller
{
    private readonly BaseDatosProyectoContext _context;

    public AccountsController(BaseDatosProyectoContext context)
    {
        _context = context;
    }

    public ActionResult Registrar()
    {
        return View("Registrar");
    }

    [HttpPost]
public ActionResult Registrar(Usuario usuario)
{
    try
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection con = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                using (SqlCommand cmd = new("crear_usuario", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NombreUsu", SqlDbType.VarChar).Value = usuario.NombreUsu;
                    cmd.Parameters.Add("@CedulaUsu", SqlDbType.VarChar).Value = usuario.CedulaUsu;
                    cmd.Parameters.Add("@EmailUsu", SqlDbType.VarChar).Value = usuario.EmailUsu;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = usuario.Password;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToAction("Index", "Home");
        }
       
    }
    catch(Exception)
    {
       
        return View("Registrar");
    }
        ViewData["error"] = "Error al registrar el usuario";
        return View("Registrar");
    }
    //Lo de arriba esta exactamente igual que en el video sin fallas ni errores 


public ActionResult Login()
{
    return View("Login");
}

    [HttpPost]
    public ActionResult Login(LoginResult l)
    {
        try
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("credencialesv", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@EmailUsu", SqlDbType.VarChar).Value = l.EmailUsu;
                        cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = l.Password;

                        con.Open();

                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            Response.Cookies.Append("user", "BIENVENIDO " + l.EmailUsu);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewData["error"] = "Error de credenciales";
                            return View("Login", l);
                        }
                        con.Close();
                    }
                }
            }
        }
        catch (Exception)
        {
            return View("Login", l);
        }
        return View("Login", l);
    }
    public ActionResult Logout()
{
    Response.Cookies.Delete("user");  
    return RedirectToAction("Index", "Home");
}

}
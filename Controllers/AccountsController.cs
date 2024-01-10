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
                    int newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();

                    // Insertar el nuevo usuario en la tabla UsuarioPerfil con el perfil de ID 3
                    using (SqlCommand cmd2 = new SqlCommand("INSERT INTO UsuarioPerfil (UsuarioID, PerfilID) VALUES (@UsuarioId, 3)", con))
                    {
                        cmd2.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = newUserId;
                        con.Open();
                        cmd2.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            return RedirectToAction("Index", "Cliente");
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
                using (SqlCommand cmd = new SqlCommand("VerificarUsertwo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = l.EmailUsu;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = l.Password;
                    var outputParam = cmd.Parameters.Add("@resultado", SqlDbType.NVarChar, 20);
                    outputParam.Direction = ParameterDirection.Output;

                    con.Open();

                    cmd.ExecuteNonQuery();

                    var userType = outputParam.Value;
                    //No te olvides que Usuario no enncotrado va sin la o .Caso contrario no funciona cuando colocas mal la contrasenia 
                    if (userType != null && userType.ToString() != "Usuario no encontrad")
                    {
                        // Almacenar el tipo de usuario en una cookie
                       // Response.Cookies.Append("userType", userType.ToString());
                       // Response.Cookies.Append("user", "BIENVENIDO " + l.EmailUsu);

                        // Redirigir al usuario a diferentes páginas dependiendo de su tipo
                        if (userType.ToString() == "Cliente")
                        {
                            return RedirectToAction("Index", "Cliente");
                        }
                        else if (userType.ToString() == "Otro")
                        {
                            return RedirectToAction("Index", "Cliente");
                        }
                         else if (userType.ToString() == "Super admin")
                        {
                            return RedirectToAction("Index", "SuperAdmin");
                        }
                        else if (userType.ToString() == "Recepcionista")
                        {
                            return RedirectToAction("Index", "Recepcionista");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ViewData["error"] = "Error de credenciales";
                        return View("Login", l);
                    }
                }
            }
        }
    }
    catch (Exception)
    {
        ViewData["error"] = "Error de credenciales";
    }
    ViewData["error"] = "Error de credenciales";
    return View("Login", l);
}
    public ActionResult Logout()
{
    Response.Cookies.Delete("user");  
    return RedirectToAction("Index", "Home");
}

}
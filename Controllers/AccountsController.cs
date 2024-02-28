using IntegradorP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;


public class AccountsController : Controller
{
    private readonly BaseDatosProyectoContext _context;

    public AccountsController(BaseDatosProyectoContext context)
    {
        _context = context;
    }
[HttpGet]
    public ActionResult Registrar()
    {
        return View("Registrar");
    }
[HttpPost]
public async Task<ActionResult> Registrar(Usuario usuario)
{
    try
    {
        // Agrega la validación de la cédula aquí
    if (ValidacionCedula.EsCedulaValida(usuario.CedulaUsu) != ValidationResult.Success)
{
    ModelState.AddModelError("CedulaUsu", "La cédula no es válida");
}
        if (ModelState.IsValid)
        {
            using (SqlConnection con = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                // Verificar si el correo electrónico ya existe
                using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE EmailUsu = @EmailUsu", con))
                {
                    cmdCheck.Parameters.Add("@EmailUsu", SqlDbType.VarChar).Value = usuario.EmailUsu;
                    con.Open();
                    int userCount = (int)cmdCheck.ExecuteScalar();
                    con.Close();

                    if (userCount > 0)
                    {
                        ModelState.AddModelError("EmailUsu", "Este correo electrónico ya está en uso.");
                        ViewData["error"] = "Error al registrar el usuario";
                        return View("Registrar", usuario);
                    }
                }
                int newUserId ;
                // Crear el nuevo usuario
                using (SqlCommand cmd = new SqlCommand("crear_usuariothree", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NombreUsu", SqlDbType.VarChar).Value = usuario.NombreUsu;
                    cmd.Parameters.Add("@CedulaUsu", SqlDbType.VarChar).Value = usuario.CedulaUsu;
                    cmd.Parameters.Add("@EmailUsu", SqlDbType.VarChar).Value = usuario.EmailUsu;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = usuario.Password;
                    con.Open();
                     newUserId = Convert.ToInt32(cmd.ExecuteScalar());
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

                // Iniciar sesión después de registrar al usuario
                var claims = new List<Claim>
               {
                    new Claim(ClaimTypes.Name, usuario.NombreUsu),
                    new Claim(ClaimTypes.Email, usuario.EmailUsu),
                    new Claim("Cedula", usuario.CedulaUsu),
                    new Claim(ClaimTypes.NameIdentifier, newUserId.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), 
                    authProperties);
            }
            return RedirectToAction("Index", "Reservas");
        }
    }
    catch(Exception ex)
    {
        // Imprime el mensaje de la excepción en la consola
        Console.WriteLine(ex.Message);
        ViewData["error"] = "Error al registrar el usuario";
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
public async Task<ActionResult> Login(LoginResult l)
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
                    if (userType != null && userType.ToString() != "Usuario no encontrad")
                    {
                        // Almacenar el tipo de usuario en una cookie
                        Response.Cookies.Append("User", "BIENVENIDO " + l.EmailUsu);

                        // Obtener el ID del usuario
                        using (SqlCommand cmdGetUserId = new SqlCommand("GetUserId", con))
                        {
                            cmdGetUserId.CommandType = CommandType.StoredProcedure;
                            cmdGetUserId.Parameters.Add("@EmailUsu", SqlDbType.VarChar).Value = l.EmailUsu;
                            cmdGetUserId.Parameters.Add("@Password", SqlDbType.VarChar).Value = l.Password;
                            var outputUserId = cmdGetUserId.Parameters.Add("@UsuarioId", SqlDbType.Int);
                            outputUserId.Direction = ParameterDirection.Output;

                            cmdGetUserId.ExecuteNonQuery();

                            var userId = outputUserId.Value;

                            Response.Cookies.Append("UserId", userId.ToString());
                            // Set user identity
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, l.EmailUsu),
                                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                            };

                            var claimsIdentity = new ClaimsIdentity(
                                claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            var authProperties = new AuthenticationProperties();

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme, 
                                new ClaimsPrincipal(claimsIdentity), 
                                authProperties);
                        }

                        // Redirigir al usuario a diferentes páginas dependiendo de su tipo
                        if (userType.ToString() == "Cliente")
                        {
                            return RedirectToAction("Index", "Reservas");
                        }
                        else if (userType.ToString() == "Otro")
                        {
                            return RedirectToAction("Index", "Reservas");
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
    public async Task<ActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    Response.Cookies.Delete("User"); 
    Response.Cookies.Delete("UserId");  // Asegúrate de eliminar la cookie UserId cuando el usuario cierre sesión 
    return RedirectToAction("Index", "Home");
}

}
using System.ComponentModel.DataAnnotations;

public static class ValidacionCedula
{
  public static ValidationResult EsCedulaValida(string cedula)
{
    int sum = 0;
    if (cedula == null)
    {
        return new ValidationResult("La cédula es requerida");
    }

    if (cedula.Length < 10 || !int.TryParse(cedula, out int _))
    {
        return new ValidationResult("La cédula es mayor y/o menor a 10 dígitos");
    }
    else
    {
        for (int i = 0; i < cedula.Length - 1; i++)
        {
            int x = int.Parse(cedula[i].ToString());
            if (i % 2 == 0)
            {
                x *= 2;
                if (x > 9)
                {
                    x -= 9;
                }
            }
            sum += x;
        }

        int result = 10 - (sum % 10);
        if (int.Parse(cedula[^1].ToString()) == result)
        {
            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult("Cédula inválida, no es ecuatoriana");
        }
    }
}
}
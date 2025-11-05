using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using SRQCC.Models;

namespace SRQCC.Services
{
    internal class DatabaseService
    {

        private readonly string _connectionString;
        public DatabaseService()
        {
            //CONEXION A LA BD
            //BD DESARROLLO
            //_connectionString = "Server=sql5113.site4now.net;Database=db_9cac4b_gestor;User Id=sistemas01;Password=Sistemas01;Encrypt=true;TrustServerCertificate=true;Timeout=30;";
            //BD PRODUCCION
            _connectionString = "Server=SQL1003.site4now.net;Database=db_ab8b05_gestor;User Id=db_ab8b05_gestor_admin;Password=Srqcc01!;Encrypt=true;TrustServerCertificate=true;Timeout=30;";
        }

        //---------------------------------------   <LoginPage>  ---------------------------------------//

        public async Task<bool> ValidatePasswordAsync(string user, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT u.pass FROM usuario u
                          LEFT JOIN persona p ON u.id_persona = p.id_persona
                          WHERE p.numero_documento = @User";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@User", user);

                        var storedPassword = await command.ExecuteScalarAsync() as string;

                        if (storedPassword == null)
                            return false; // Usuario no encontrado
                        
                        //Dejar logear a contraseñas con HASH y sin HASH
                        // Verificar si la contraseña está hasheada
                        if (IsPasswordHashed(storedPassword))
                        {
                            // Si está hasheada, usar el método de verificación
                            return VerifyPassword(password, storedPassword);
                        }
                        else
                        {
                            // Si está en texto plano, comparar directamente
                            return password == storedPassword;
                        }
                        /*
                        //Solo dejar logear a contraseñas con HASH
                        return VerifyPassword(password, storedPassword);
                        */
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validando contraseña: {ex.Message}");
                return false;
            }
        }

        // Método para verificar si la contraseña está hasheada
        private bool IsPasswordHashed(string password)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(password);
                return hashBytes.Length == 48; // 16 (salt) + 32 (hash)
            }
            catch
            {
                return false;
            }
        }

        // Método original para verificar contraseñas hasheadas
        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }
            return true;
        }

        //Obtener el codigo de asociado con el dni
        public async Task<string> GetCodigoAsociadoAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"select a.codigo_asociado from asociado a
                                  left join persona p
                                  on a.id_persona = p.id_persona
                                  where p.numero_documento = @Dni";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Dni", dni);

                        var result = await command.ExecuteScalarAsync();
                        return result?.ToString() ?? "Codigo_asociado";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo el codigo de asociado: {ex.Message}");
                return "Codigo_asociado";
            }
        }

        //Metodo para obtener el nombre completo
        public async Task<string> GetNombreCompletoAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT p.nombre1 + ' ' + p.apellido_paterno 
                        FROM persona p 
                        INNER JOIN asociado a ON p.id_persona = a.id_persona 
                        WHERE p.numero_documento = @Dni";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Dni", dni);

                        var result = await command.ExecuteScalarAsync();
                        return result?.ToString() ?? "Usuario";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo nombre: {ex.Message}");
                return "Usuario";
            }
        }

        public async Task<int> GetEstadoPagoAsync(string dni)
        {
            try
            {
                

                // PRIMERO VERIFICAR SI ESTÁ ACTIVO O INACTIVO
                var idEstadoAsociado = await GetEstadoAsociadoAsync(dni);

                if (idEstadoAsociado == 2) // 2 = INACTIVO
                {
                    
                    return 0; // 0 = ASOCIADO INACTIVO
                }

                // Obtener estados de pago (mes actual y anterior solamente)
                var resultados = await ObtenerEstadosPagosAsync(dni);
                var diaActual = DateTime.Now.Day;
                var mesActual = DateTime.Now.Month;
                var añoActual = DateTime.Now.Year;

                

                // Si no hay resultados de pagos, considerar como pendiente
                if (resultados == null || resultados.Count == 0)
                {
                    

                    // Buscar primer mes pendiente para determinar la deuda
                    var primerMesPendiente = await ObtenerPrimerMesPendienteAsync(dni);

                    return -1; // Pendiente
                }


                //  Buscar estado de mes actual
                var pagoMesActual = resultados.FirstOrDefault(p =>
                    p.Mes == mesActual && p.Anio == añoActual);

                // Buscar estado de mes anterior
                var mesAnterior = DateTime.Now.AddMonths(-1);
                var pagoMesAnterior = resultados.FirstOrDefault(p =>
                    p.Mes == mesAnterior.Month && p.Anio == mesAnterior.Year);

                bool estaAlDia;

                //Vencimiento los 15 de cada mes
                if (pagoMesActual != null && pagoMesActual.IdEstadoPago == 2)
                {
                    estaAlDia = true; //  Mes actual pagado
                }
                else if (diaActual <= 15)
                {
                    // Antes del 15: verificar si mes anterior está pagado
                    estaAlDia = (pagoMesAnterior != null && pagoMesAnterior.IdEstadoPago == 2);
                }
                else
                {
                    // Después del 15: mes actual debe estar pagado
                    estaAlDia = false; //  Pasó el 15 y no está pagado mes actual
                }

                // SOLO si está pendiente, buscar desde cuándo debe
                if (!estaAlDia)
                {
                    var primerMesPendiente = await ObtenerPrimerMesPendienteAsync(dni);
                }

                return estaAlDia ? 2 : 1; // 2 = Al día, 1 = Pendiente
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando estado de pago: {ex.Message}");
                return -1; // Por seguridad, retornar no hay registros
            }
        }


        public async Task<int> GetEstadoAsociadoAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                SELECT a.id_estado 
                FROM asociado a
                LEFT JOIN persona p ON a.id_persona = p.id_persona
                WHERE p.numero_documento = @Dni";

                    var result = await connection.ExecuteScalarAsync(query, new { Dni = dni });

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }

                    return 1; // Por defecto asumir activo
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo estado del asociado: {ex.Message}");
                return 1; // Por defecto asumir activo
            }
        }



        private async Task<List<EstadoPago>> ObtenerEstadosPagosAsync(string dni)
        {
            try
            {
                var estados = new List<EstadoPago>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // CONSULTA SIMPLIFICADA: Usar parámetros de fecha actual
                    var mesActual = DateTime.Now.Month;
                    var añoActual = DateTime.Now.Year;
                    var mesAnterior = DateTime.Now.AddMonths(-1).Month;
                    var añoAnterior = DateTime.Now.AddMonths(-1).Year;

                    string query = @"
                SELECT 
                    pa.mes,
                    pa.anio,
                    pa.id_estado_pago_detalle
                FROM pago_detalle pa
                WHERE pa.id_asociado = (SELECT a.id_asociado FROM asociado a
                                        INNER JOIN persona p ON a.id_persona = p.id_persona
                                        WHERE p.numero_documento = @dni)
                AND (
                    (pa.mes = @mesActual AND pa.anio = @añoActual)
                    OR
                    (pa.mes = @mesAnterior AND pa.anio = @añoAnterior)
                )
                ORDER BY pa.anio DESC, pa.mes DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dni", dni);
                        command.Parameters.AddWithValue("@mesActual", mesActual);
                        command.Parameters.AddWithValue("@añoActual", añoActual);
                        command.Parameters.AddWithValue("@mesAnterior", mesAnterior);
                        command.Parameters.AddWithValue("@añoAnterior", añoAnterior);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var estado = new EstadoPago
                                {
                                    Mes = Convert.ToInt32(reader["mes"]),
                                    Anio = Convert.ToInt32(reader["anio"]),
                                    IdEstadoPago = Convert.ToInt32(reader["id_estado_pago_detalle"])
                                };
                                estados.Add(estado);
                            }
                        }
                    }
                }

                return estados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo estados de pago: {ex.Message}");
                return new List<EstadoPago>();
            }
        }


        public async Task<EstadoPago> ObtenerPrimerMesPendienteAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    //  Buscar cualquier mes pendiente hasta la fecha actual
                    string query = @"
                SELECT TOP 1 
                    pa.id_estado_pago_detalle, 
                    pa.mes, 
                    pa.anio  
                FROM pago_detalle pa 
                WHERE id_asociado = (SELECT a.id_asociado FROM asociado a
                                    INNER JOIN persona p ON a.id_persona = p.id_persona
                                   WHERE p.numero_documento = @dni)
                AND id_estado_pago_detalle != 2
                AND (pa.anio < YEAR(GETDATE()) 
                     OR (pa.anio = YEAR(GETDATE()) AND pa.mes <= MONTH(GETDATE())))
                ORDER BY pa.anio ASC, pa.mes ASC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dni", dni);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var estadoPago = new EstadoPago
                                {
                                    Mes = Convert.ToInt32(reader["mes"]),
                                    Anio = Convert.ToInt32(reader["anio"]),
                                    IdEstadoPago = Convert.ToInt32(reader["id_estado_pago_detalle"])
                                };

                                return estadoPago;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo primer mes pendiente: {ex.Message}");
                return null;
            }
        }

        //---------------------------------------   <LoginPage/>   ---------------------------------------//


        //---------------------------------------   <MainPage>     ---------------------------------------//

        //Obtener el codigo para la imagen (dni)FOTO
        public async Task<string> GetCodigoImagenAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT p.numero_documento as codigo_imagen
                        FROM asociado a
                        INNER JOIN persona p ON a.id_persona = p.id_persona
                        WHERE p.numero_documento = @dni";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dni", dni);
                        var result = await command.ExecuteScalarAsync();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo código de imagen: {ex.Message}");
                return null;
            }
        }

        public static class EncryptionHelper
        {
            private static readonly byte[] Key = Encoding.UTF8.GetBytes("16byte-key-12345"); // 16 bytes para AES-128

            public static string Encrypt(string plainText)
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.Mode = CipherMode.ECB; // Más compacto (menos seguro pero más pequeño)
                aes.Padding = PaddingMode.PKCS7;

                var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Usar Base64 URL-safe
                return Convert.ToBase64String(encryptedBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");
            }

            public static string Decrypt(string encryptedText)
            {
                try
                {
                    // Revertir URL-safe
                    var base64 = encryptedText.Replace('-', '+').Replace('_', '/');
                    switch (base64.Length % 4)
                    {
                        case 2: base64 += "=="; break;
                        case 3: base64 += "="; break;
                    }

                    using var aes = Aes.Create();
                    aes.Key = Key;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;

                    var decryptor = aes.CreateDecryptor();
                    var encryptedBytes = Convert.FromBase64String(base64);
                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    return null;
                }
            }
        }
        //---------------------------------------   <MainPage/>    ---------------------------------------//


        //---------------------------------------   <Mi Perfil>    ---------------------------------------//

        //Datos del asociado
        public async Task<PerfilAsociado> GetPerfilAsociadoAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
            SELECT 
                p.nombre1 + ' ' + ISNULL(p.nombre2, '') as Nombres,
                p.apellido_paterno as ApellidoPaterno,
                p.apellido_materno as ApellidoMaterno,
                d.des_tipo_documento as TipoDocumento,
                p.numero_documento as NumeroDocumento,
                p.telefono as Telefono,
                p.email as Email,
                c.des_estado_persona as Estado
            FROM persona p 
            INNER JOIN cat_estado_persona c ON p.id_estado = c.id_estado_persona 
            INNER JOIN cat_tipo_documento d ON p.id_documento = d.id_tipo_documento
            WHERE p.numero_documento = @Dni";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Dni", dni);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new PerfilAsociado
                                {
                                    Nombres = reader["Nombres"]?.ToString() ?? "",
                                    ApellidoPaterno = reader["ApellidoPaterno"]?.ToString() ?? "",
                                    ApellidoMaterno = reader["ApellidoMaterno"]?.ToString() ?? "",
                                    TipoDocumento = reader["TipoDocumento"]?.ToString() ?? "",
                                    NumeroDocumento = reader["NumeroDocumento"]?.ToString() ?? "",
                                    Telefono = reader["Telefono"]?.ToString() ?? "",
                                    Email = reader["Email"]?.ToString() ?? "",
                                    Estado = reader["Estado"]?.ToString() ?? ""
                                };
                            }
                        }
                    }

                    return new PerfilAsociado();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo perfil: {ex.Message}");
                return new PerfilAsociado();
            }
        }


        //Datos de los Dependientes

        public async Task<List<Dependiente>> GetDependientesAsync(string dni)
        {
            var dependientes = new List<Dependiente>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Primero obtener el id_asociado
                    string idAsociadoQuery = @"
                SELECT a.id_asociado 
                FROM asociado a 
                INNER JOIN persona p on a.id_persona = p.id_persona 
                WHERE p.numero_documento = @dni";

                    int idAsociado = 0;

                    using (SqlCommand idCommand = new SqlCommand(idAsociadoQuery, connection))
                    {
                        idCommand.Parameters.AddWithValue("@dni", dni);
                        var result = await idCommand.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            idAsociado = Convert.ToInt32(result);
                        }
                        else
                        {
                            return dependientes;
                        }
                    }

                    // Luego obtener los dependientes
                    string query = @"
                SELECT 
                    t.des_tipo_dependiente, 
                    p.nombre1, 
                    p.apellido_paterno, 
                    p.apellido_materno,
                    p.numero_documento
                FROM persona p 
                INNER JOIN dependiente d ON p.id_persona = d.id_persona 
                INNER JOIN asociado a ON a.id_asociado = d.id_asociado
                INNER JOIN cat_tipo_dependiente t ON d.id_tipo_dependiente = t.id_tipo_dependiente 
                WHERE a.id_asociado = @idAsociado";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idAsociado", idAsociado);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dependientes.Add(new Dependiente
                                {
                                    TipoDependiente = reader["des_tipo_dependiente"]?.ToString() ?? "",
                                    Nombre = reader["nombre1"]?.ToString() ?? "",
                                    ApellidoPaterno = reader["apellido_paterno"]?.ToString() ?? "",
                                    ApellidoMaterno = reader["apellido_materno"]?.ToString() ?? "",
                                    Dni = reader["numero_documento"]?.ToString() ?? "",
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo dependientes: {ex.Message}");
            }

            return dependientes;
        }

        //---------------------------------------   <Mi Perfil/>   ---------------------------------------//

        //---------------------------------------   <Mis Pagos>    ---------------------------------------//
        //OBTENER EL PRIMER MES QUE REALIZADO PAGO DE MANTENIMIENTO
        public async Task<int> GetPrimerAnioPagoAsync(string dni)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                SELECT MIN(anio) as PrimerAnio 
                FROM pago_detalle 
                WHERE id_asociado = 
                (SELECT id_asociado FROM asociado WHERE id_persona = 
                (SELECT id_persona FROM persona WHERE numero_documento =@Dni))";

                    var result = await connection.ExecuteScalarAsync(query, new { Dni = dni });

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }

                    // Si no tiene pagos, retorna año actual
                    return DateTime.Now.Year;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo primer año de pago: {ex.Message}");
                return DateTime.Now.Year;
            }
        }

        //OBTIENE EL HISTORIAL DE PAGOS
        public async Task<List<PagoDetalle>> GetHistorialPagosAsync(string dni, int anio)
        {
            var historial = new List<PagoDetalle>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // OBTENER ÚLTIMO AÑO DE PAGO PARA ASOCIADOS INACTIVOS
                    var ultimoAnioPago = await connection.ExecuteScalarAsync<int?>(
                        "SELECT MAX(anio) FROM pago_detalle WHERE id_asociado = (SELECT id_asociado FROM asociado WHERE id_persona = (SELECT id_persona FROM persona WHERE numero_documento =@Dni))",
                        new { Dni = dni });

                    var query = @"
                SELECT mes, anio, monto, id_estado_pago_detalle 
                FROM pago_detalle 
                WHERE id_asociado = (SELECT id_asociado FROM asociado WHERE id_persona = 
                (SELECT id_persona FROM persona WHERE numero_documento = @Dni))
                AND anio = @Año
                ORDER BY anio, mes";

                    var resultados = await connection.QueryAsync<PagoDetalle>(query, new
                    {
                        Dni = dni,
                        Año = anio
                    });

                    historial = resultados.ToList();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo historial: {ex.Message}");
            }

            return historial;
        }

        public async Task<int> GetPrimerMesPagoAsync(string dni, int anio)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                SELECT MIN(mes) as PrimerMes 
                FROM pago_detalle 
                WHERE id_asociado = 
                (SELECT id_asociado FROM asociado WHERE id_persona = 
                (SELECT id_persona FROM persona WHERE numero_documento =@Dni))
                AND anio = @Anio";

                    var result = await connection.ExecuteScalarAsync(query, new
                    {
                        Dni = dni,
                        Anio = anio
                    });

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }

                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo primer mes de pago: {ex.Message}");
                return 1;
            }
        }

        //---------------------------------------   <Mis Pagos/>   ---------------------------------------//



        //---------------------------------------    EXTRAS  ---------------------------------------------//
        // MÉTODO NUEVO: Verificar si imagen existe
        public async Task<bool> VerificarImagenExisteAsync(string urlImagen)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    var response = await httpClient.GetAsync(urlImagen);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando imagen: {ex.Message}");
                return false;
            }
        }



        //Obtener el dni, usar mientras usemos el login con codigo de asociado
        public async Task<string> GetDniAsync(string codigo_asociado)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"select p.numero_documento from persona p
                                  left join asociado a 
                                  on p.id_persona = a.id_persona
                                  where a.codigo_asociado = @Codigo_asociado";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Codigo_asociado", codigo_asociado);

                        var result = await command.ExecuteScalarAsync();
                        return result?.ToString() ?? "Dni";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo Dni: {ex.Message}");
                return "Dni";
            }

        }

        //---------------------------    AppVersionManager - CONTROL DE VERSIONES -----------------------------------//

        public async Task<VersionInfo> GetLatestVersionAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = @"
            SELECT TOP 1 * 
            FROM movil_versiones 
            WHERE version_code > @CurrentVersionCode
            ORDER BY version_code DESC";

                return await connection.QueryFirstOrDefaultAsync<VersionInfo>(query, new
                {
                    CurrentVersionCode = AppVersionManager.CurrentVersionCode
                });
            }
            catch
            {
                return null;
            }
        }
    }
}

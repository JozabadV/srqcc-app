using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SRQCC.Services
{
    public static class SessionService
    {
        private const string SessionKey = "user_session";

        public static bool IsLoggedIn { get; private set; }
        public static string CodigoAsociado { get; private set; }
        public static string NombreCompleto { get; private set; }
        public static int EstadoPago { get; private set; } //  Agregar EstadoPago
        public static string Dni { get; private set; }


        public static void Login(string codigoAsociado, string nombreCompleto, int estadoPago, string dni)
        {
            CodigoAsociado = codigoAsociado;
            NombreCompleto = nombreCompleto;
            EstadoPago = estadoPago; //  Guardar estado de pago
            Dni = dni;
            IsLoggedIn = true;

            //  GUARDAR SESIÓN EN PREFERENCES
            SaveSession();

            Console.WriteLine($" Sesión iniciada: {nombreCompleto}");
        }

        public static void Logout()
        {
            CodigoAsociado = null;
            NombreCompleto = null;
            Dni = null;
            EstadoPago = -1;
            IsLoggedIn = false;

            //  LIMPIAR SESIÓN GUARDADA
            ClearSession();

            Console.WriteLine(" Sesión cerrada");
        }

        //  CARGAR SESIÓN AL INICIAR LA APP
        public static async Task LoadSession()
        {
            try
            {
                var sessionJson = await SecureStorage.GetAsync(SessionKey);
                if (!string.IsNullOrEmpty(sessionJson))
                {
                    var sessionData = JsonSerializer.Deserialize<SessionData>(sessionJson);
                    if (sessionData != null)
                    {
                        CodigoAsociado = sessionData.CodigoAsociado;
                        NombreCompleto = sessionData.NombreCompleto;
                        Dni = sessionData.Dni;
                        EstadoPago = sessionData.EstadoPago;
                        IsLoggedIn = true;

                        Console.WriteLine($" Sesión recuperada: {NombreCompleto}");
                    }
                }
                else
                {
                    Console.WriteLine(" No hay sesión guardada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error cargando sesión: {ex.Message}");
                IsLoggedIn = false;
            }
        }

        // GUARDAR SESIÓN
        private static void SaveSession()
        {
            try
            {
                var sessionData = new SessionData
                {
                    CodigoAsociado = CodigoAsociado,
                    NombreCompleto = NombreCompleto,
                    Dni = Dni,
                    EstadoPago = EstadoPago
                };

                var sessionJson = JsonSerializer.Serialize(sessionData);
                SecureStorage.SetAsync(SessionKey, sessionJson);

                Console.WriteLine($" Sesión guardada para: {NombreCompleto}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error guardando sesión: {ex.Message}");
            }
        }

        //  LIMPIAR SESIÓN
        private static void ClearSession()
        {
            try
            {
                SecureStorage.Remove(SessionKey);
                Console.WriteLine("🗑 Sesión eliminada de SecureStorage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error limpiando sesión: {ex.Message}");
            }
        }

        // Método para actualizar solo el estado de pago
        public static void ActualizarEstadoPago(int nuevoEstado)
        {
            EstadoPago = nuevoEstado;
        }

        // CLASE PARA DATOS DE SESIÓN
        private class SessionData
        {
            public string CodigoAsociado { get; set; }
            public string NombreCompleto { get; set; }
            public string Dni { get; set; }
            public int EstadoPago { get; set; } //  Incluir estado de pago
        }
    }
}
